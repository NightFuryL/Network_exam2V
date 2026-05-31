using ClientWFTask.Services;
using GameCore;

namespace ClientWFTask;

public partial class WatchForm : Form
{
    private List<RoomInfo> _rooms = new();

    public WatchForm() => InitializeComponent();

    private void WatchForm_Load(object sender, EventArgs e)
    {
        GameConnection.Instance.PacketReceived += OnPacket;
        RefreshList();
    }

    private void OnPacket(INetworkPacket packet)
    {
        if (packet is RoomsListManagePacket list)
        {
            _rooms = list.Rooms;
            if (InvokeRequired) { BeginInvoke(() => OnPacket(packet)); return; }
            lbGames.Items.Clear();
            if (_rooms.Count == 0)
            {
                lbGames.Items.Add("Нет активных игр");
                return;
            }
            foreach (var r in _rooms)
                lbGames.Items.Add($"#{r.Id}  {r.WhiteName}  vs  {r.BlackName}");
        }
        else if (packet is MatchStartedPacket match)
        {
            if (InvokeRequired) { BeginInvoke(() => OpenRoom(match)); return; }
            OpenRoom(match);
        }
    }

    private void OpenRoom(MatchStartedPacket match)
    {
        Session.RoomId = match.RoomId;
        Session.Role = ClientType.Watcher;
        Session.Color = null;
        Hide();
        new RoomForm(match).ShowDialog();
        Show();
        RefreshList();
    }

    private void RefreshList() => GameConnection.Instance.Send(new RoomsListManagePacket());

    private void btnRefresh_Click(object sender, EventArgs e) => RefreshList();

    private void btnWatch_Click(object sender, EventArgs e)
    {
        if (lbGames.SelectedIndex < 0 || _rooms.Count == 0) return;
        var room = _rooms[lbGames.SelectedIndex];
        GameConnection.Instance.Send(new ManageConnectionPacket
        {
            RoomId = room.Id,
            ConnectionType = ConnectionType.Connect
        });
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        GameConnection.Instance.PacketReceived -= OnPacket;
        base.OnFormClosed(e);
    }
}
