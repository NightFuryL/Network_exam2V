using DatabaseLibrary;
using GameCore;

namespace ClientWFTask;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        lblNickname.Text = $"Nickname: {Session.Name}";
        lblRating.Text = $"Rating: {Session.Points}";
        lblGuid.Text = $"Guid: {Session.Id}";
    }

    private void btnPlay_Click(object sender, EventArgs e)
    {

    }

    private void btnWatch_Click(object sender, EventArgs e) =>
        new WatchForm().ShowDialog();

    private void btnLeaderBoard_Click(object sender, EventArgs e) =>
        new LeaderboardForm().ShowDialog();

    private void btnLogout_Click(object sender, EventArgs e)
    {
        AutoLogin.Logout();
        Application.Restart();
    }
}
