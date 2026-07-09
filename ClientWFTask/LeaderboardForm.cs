using ClientWFTask.Services;
using DatabaseLibrary.Entities;
using GameCore;
using System.Text;
using System.Text.Json;

namespace ClientWFTask;

public partial class LeaderboardForm : Form
{
    public LeaderboardForm()
    {
        InitializeComponent();
        BoardFormHelper.StyleForm(this);
        BoardFormHelper.StyleList(lbLeaderboard);
        BoardFormHelper.StyleFormButton(btnRefresh);
    }

    private async void LeaderboardForm_Load(object sender, EventArgs e) => await RefreshBoard();

    private async Task RefreshBoard()
    {
        try
        {
            GameConnection.Instance.EnsureConnected();
            var packet = new NetworkPacket
            {
                CommandCode = PacketType.LeaderBoardData,
                PacketTo = PacketWhere.ToServer,
                PResult = PacketResult.Success,
                Data = Array.Empty<byte>()
            };
            var response = await GameConnection.Instance.SendAndWaitAsync(packet, PacketType.LeaderBoardData);
            if (response.PResult != PacketResult.Success) return;
            var board = JsonSerializer.Deserialize<List<UserEntity>>(response.Data) ?? new();
            DoByPacket(board);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void DoByPacket(List<UserEntity> board)
    {
        lbLeaderboard.Items.Clear();
        int place = 1;
        foreach (var u in board)
            lbLeaderboard.Items.Add($"{place++}. {u.Name} | {u.Rating}");
    }

    private async void btnRefresh_Click(object sender, EventArgs e) => await RefreshBoard();
}
