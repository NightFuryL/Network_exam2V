using ClientWFTask.Services;
using GameCore;
using System.Text;
using System.Text.Json;

namespace ClientWFTask;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        BoardFormHelper.StyleForm(this);
        lblNickname.Text = $"Nickname: {Session.Name}";
        lblRating.Text = $"Rating: {Session.Points}";
        BoardFormHelper.StyleLabel(lblNickname, true);
        BoardFormHelper.StyleLabel(lblRating);
        BoardFormHelper.StyleFormButton(btnPlay);
        BoardFormHelper.StyleFormButton(btnWatch);
        BoardFormHelper.StyleFormButton(btnLeaderBoard);
        BoardFormHelper.StyleFormButton(btnLogout);
    }

    private async void btnPlay_Click(object sender, EventArgs e)
    {
        try
        {
            btnPlay.Enabled = false;
            GameConnection.Instance.EnsureConnected();
            GameConnection.Instance.PacketReceived += OnMatchStarted;

            var packet = new NetworkPacket
            {
                CommandCode = PacketType.FindGame,
                PacketTo = PacketWhere.ToServer,
                PResult = PacketResult.Success,
                Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new FindGameData
                {
                    UserId = Session.Id,
                    UserName = Session.Name
                }))
            };
            await GameConnection.Instance.SendAsync(packet);
        }
        catch (Exception ex)
        {
            btnPlay.Enabled = true;
            MessageBox.Show(ex.Message);
        }
    }

    private void OnMatchStarted(NetworkPacket packet)
    {
        var match = BoardFormHelper.ParseMatchStarted(packet);
        if (match == null || match.Role != ClientType.Player) return;
        if (InvokeRequired) { BeginInvoke(() => OnMatchStarted(packet)); return; }

        GameConnection.Instance.PacketReceived -= OnMatchStarted;
        btnPlay.Enabled = true;
        Session.RoomId = match.RoomId;
        Session.Role = ClientType.Player;
        Session.Color = match.YourColor;
        Hide();
        new RoomForm(match).ShowDialog();
        lblRating.Text = $"Rating: {Session.Points}";
        Show();
    }

    private void btnWatch_Click(object sender, EventArgs e) =>
        new WatchForm().ShowDialog();

    private void btnLeaderBoard_Click(object sender, EventArgs e) =>
        new LeaderboardForm().ShowDialog();

    private void btnLogout_Click(object sender, EventArgs e)
    {
        GameConnection.Instance.Disconnect();
        Session.Clear();
        Hide();
        new RegisterForm().ShowDialog();
        Close();
    }
}
