using ClientWFTask.Services;
using GameCore;
using System.Text;
using System.Text.Json;

namespace ClientWFTask;

public partial class WatcherRoom : Form
{
    private readonly MatchStartedData _match;
    private readonly Button[,] _cells = new Button[8, 8];
    private readonly Label _lblEatenWhite;
    private readonly Label _lblEatenBlack;
    private BoardState? _state;

    public WatcherRoom(MatchStartedData match)
    {
        _match = match;
        InitializeComponent();
        BoardFormHelper.StyleForm(this);

        lblPlayer_1.Text = "\u25CB " + match.WhiteName;
        lblPlayer_2.Text = "\u25CF " + match.BlackName;
        BoardFormHelper.StyleLabel(lblPlayer_1, true);
        BoardFormHelper.StyleLabel(lblPlayer_2, true);
        BoardFormHelper.StyleLabel(lblTurn);

        _lblEatenWhite = new Label
        {
            AutoSize = true,
            Location = new Point(504, 50),
            Text = "Captured white: 0"
        };
        _lblEatenBlack = new Label
        {
            AutoSize = true,
            Location = new Point(504, 370),
            Text = "Captured black: 0"
        };
        Controls.Add(_lblEatenWhite);
        Controls.Add(_lblEatenBlack);
        BoardFormHelper.StyleLabel(_lblEatenWhite);
        BoardFormHelper.StyleLabel(_lblEatenBlack);

        BoardFormHelper.BuildBoard(panel1, _cells, true, null);
        BoardFormHelper.StyleFormButton(btnExit);
        btnExit.Text = "Exit";
        lblTurn.Text = "Watcher";
        GameConnection.Instance.PacketReceived += OnPacket;
        if (match.InitialBoard != null)
            ApplyBoard(match.InitialBoard);
    }

    private void OnPacket(NetworkPacket packet)
    {
        if (packet.CommandCode == PacketType.BoardStatePacket)
        {
            var board = BoardFormHelper.ParseBoardState(packet);
            if (board == null) return;
            if (InvokeRequired) { BeginInvoke(() => ApplyBoard(board)); return; }
            ApplyBoard(board);
        }
        else if (packet.CommandCode == PacketType.GameEndPacket)
        {
            var end = BoardFormHelper.ParseGameEnd(packet);
            if (end == null) return;
            if (InvokeRequired) { BeginInvoke(() => { MessageBox.Show("Game finished: " + end.Result); Close(); }); return; }
            MessageBox.Show("Game finished: " + end.Result);
            Close();
        }
    }

    private void ApplyBoard(BoardState board)
    {
        _state = board;
        BoardFormHelper.Draw(board, _cells, lblTurn, _lblEatenWhite, _lblEatenBlack);
        if (board.Result == GameState.InProgress)
            lblTurn.Text = $"Turn: {BoardFormHelper.PlayerName(board.Turn)} (watcher)";
    }

    private async void btnExit_Click(object sender, EventArgs e)
    {
        try
        {
            await GameConnection.Instance.SendAsync(new NetworkPacket
            {
                CommandCode = PacketType.ManageConnectionPacket,
                PacketTo = PacketWhere.ToServer,
                PResult = PacketResult.Success,
                Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new ManageConnectionData
                {
                    RoomId = _match.RoomId,
                    UserId = 0,
                    ConnectionType = ConnectionType.Exit
                }))
            });
        }
        catch { }
        Close();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        GameConnection.Instance.PacketReceived -= OnPacket;
        Session.RoomId = 0;
        base.OnFormClosed(e);
    }
}
