using ClientWFTask.Services;
using GameCore;

namespace ClientWFTask;

public partial class LeaderboardForm : Form
{
    public LeaderboardForm() => InitializeComponent();

    private void LeaderboardForm_Load(object sender, EventArgs e)
    {
        GameConnection.Instance.PacketReceived += OnPacket;
        GameConnection.Instance.Send(new LeaderBoardManagePacket());
    }

    private void OnPacket(INetworkPacket packet)
    {
        if (packet is not LeaderBoardManagePacket board) return;
        if (InvokeRequired) { BeginInvoke(() => OnPacket(packet)); return; }

        lbLeaderboard.Items.Clear();
        int place = 1;
        foreach (var u in board.TopUsers)
            lbLeaderboard.Items.Add($"{place++}. {u.Name} | R:{u.Rating} | W:{u.Wins} D:{u.Draws} L:{u.Loses}");
    }

    private void btnRefresh_Click(object sender, EventArgs e) =>
        GameConnection.Instance.Send(new LeaderBoardManagePacket());

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        GameConnection.Instance.PacketReceived -= OnPacket;
        base.OnFormClosed(e);
    }
}
