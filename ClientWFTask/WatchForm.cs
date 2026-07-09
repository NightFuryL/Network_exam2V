using ClientWFTask.Services;
using GameCore;
using System.Text;
using System.Text.Json;

namespace ClientWFTask;

public partial class WatchForm : Form
{
    private List<RoomInfoData> _rooms = new();

    public WatchForm()
    {
        InitializeComponent();
        BoardFormHelper.StyleForm(this);
        BoardFormHelper.StyleList(lbGames);
        BoardFormHelper.StyleFormButton(btnRefresh);
        BoardFormHelper.StyleFormButton(btnWatch);
    }

    private void WatchForm_Load(object sender, EventArgs e)
    {
        GameConnection.Instance.PacketReceived += OnPacket;
        RefreshList();
    }

    private void OnPacket(NetworkPacket packet)
    {
        if (packet.CommandCode == PacketType.RoomsListForWatchersPacket && packet.PResult == PacketResult.Success)
        {
            _rooms = JsonSerializer.Deserialize<List<RoomInfoData>>(packet.Data) ?? new();
            if (InvokeRequired) { BeginInvoke(() => FillList()); return; }
            FillList();
        }
        else if (packet.CommandCode == PacketType.MatchStartedPacket)
        {
            var match = BoardFormHelper.ParseMatchStarted(packet);
            if (match == null || match.Role != ClientType.Watcher) return;
            if (InvokeRequired) { BeginInvoke(() => OpenRoom(match)); return; }
            OpenRoom(match);
        }
    }

    private void FillList()
    {
        lbGames.Items.Clear();
        if (_rooms.Count == 0)
        {
            lbGames.Items.Add("No active games");
            return;
        }
        foreach (var r in _rooms)
            lbGames.Items.Add($"#{r.Id}  {r.WhiteName}  vs  {r.BlackName}");
    }

    private void OpenRoom(MatchStartedData match)
    {
        Session.RoomId = match.RoomId;
        Session.Role = ClientType.Watcher;
        Session.Color = null;
        Hide();
        new WatcherRoom(match).ShowDialog();
        Show();
        RefreshList();
    }

    private async void RefreshList()
    {
        try
        {
            GameConnection.Instance.EnsureConnected();
            await GameConnection.Instance.SendAsync(new NetworkPacket
            {
                CommandCode = PacketType.RoomsListForWatchersPacket,
                PacketTo = PacketWhere.ToServer,
                PResult = PacketResult.Success,
                Data = Array.Empty<byte>()
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void btnRefresh_Click(object sender, EventArgs e) => RefreshList();

    private async void btnWatch_Click(object sender, EventArgs e)
    {
        if (lbGames.SelectedIndex < 0 || _rooms.Count == 0) return;
        var room = _rooms[lbGames.SelectedIndex];
        try
        {
            await GameConnection.Instance.SendAsync(new NetworkPacket
            {
                CommandCode = PacketType.ManageConnectionPacket,
                PacketTo = PacketWhere.ToServer,
                PResult = PacketResult.Success,
                Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new ManageConnectionData
                {
                    RoomId = room.Id,
                    UserId = 0,
                    ConnectionType = ConnectionType.Connect
                }))
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        GameConnection.Instance.PacketReceived -= OnPacket;
        base.OnFormClosed(e);
    }
}
