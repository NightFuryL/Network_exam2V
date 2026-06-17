using ClientWFTask.Services;
using DatabaseLibrary.Entities;
using GameCore;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text.Json;

namespace ClientWFTask;

public partial class LeaderboardForm : Form
{
    public LeaderboardForm() => InitializeComponent();
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<UserEntity> board { get; set; }
    private void LeaderboardForm_Load(object sender, EventArgs e)
    {
        JsonSerializer.Ser
    }
    
    private void ManagePacket()
    {
        INetworkPacket packet;

        if (packet is not LeaderBoardManagePacket board) return;
    }
    private void DoByPacket()
    {
        lbLeaderboard.Items.Clear();
        int place = 1;
        foreach (var u in board)
            lbLeaderboard.Items.Add($"{place++}. {u.Name} | R:{u.Rating} | W:{u.Wins} D:{u.Draws} L:{u.Loses}");
    }
    private void btnRefresh_Click(object sender, EventArgs e) =>


    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
    }
}
