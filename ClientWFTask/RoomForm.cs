using ClientWFTask.Services;
using GameCore;
using System.Text;
using System.Text.Json;

namespace ClientWFTask;

public partial class RoomForm : Form
{
    private readonly MatchStartedData _match;
    private readonly Button[,] _cells = new Button[8, 8];
    private readonly Label _lblEatenWhite;
    private readonly Label _lblEatenBlack;
    private readonly PlayerColor? _myColor;
    private int? _pickRow, _pickCol;
    private BoardState? _state;

    public RoomForm(MatchStartedData match)
    {
        _match = match;
        _myColor = match.YourColor;
        InitializeComponent();
        BoardFormHelper.StyleForm(this);

        bool iAmBlack = _myColor == PlayerColor.Black;
        string opponentName = iAmBlack ? match.WhiteName : match.BlackName;
        string myName = iAmBlack ? match.BlackName : match.WhiteName;
        string opponentSymbol = iAmBlack ? "\u25CB" : "\u25CF";
        string mySymbol = iAmBlack ? "\u25CF" : "\u25CB";

        lblPlayer_1.Text = $"{opponentSymbol} Opponent: {opponentName}";
        lblPlayer_2.Text = $"{mySymbol} You: {myName}";
        BoardFormHelper.StyleLabel(lblPlayer_1, true);
        BoardFormHelper.StyleLabel(lblPlayer_2, true);
        BoardFormHelper.StyleLabel(lblTurn);

        BoardFormHelper.StyleFormButton(btnExit);
        btnExit.Text = "Exit";

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

        BoardFormHelper.BuildBoard(panel1, _cells, false, OnCellClick, iAmBlack);
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
        else if (packet.CommandCode == PacketType.MoveSyncPacket && packet.PResult == PacketResult.Failed)
        {
            var move = BoardFormHelper.ParseMoveSync(packet);
            if (move == null || move.Success) return;
            if (InvokeRequired)
            {
                BeginInvoke(async () => await ShowInvalidMove(move));
                return;
            }
            _ = ShowInvalidMove(move);
        }
        else if (packet.CommandCode == PacketType.GameEndPacket)
        {
            var end = BoardFormHelper.ParseGameEnd(packet);
            if (end == null) return;
            if (InvokeRequired) { BeginInvoke(() => HandleGameEnd(end)); return; }
            HandleGameEnd(end);
        }
    }

    private void ApplyBoard(BoardState board)
    {
        _state = board;
        _pickRow = _pickCol = null;
        BoardFormHelper.Draw(board, _cells, lblTurn, _lblEatenWhite, _lblEatenBlack, _myColor);
    }

    private async Task ShowInvalidMove(MoveSyncData move)
    {
        await BoardFormHelper.FlashInvalid(_cells, move.Move.FromRow, move.Move.FromColumn);
        if (_state != null)
            BoardFormHelper.Draw(_state, _cells, lblTurn, _lblEatenWhite, _lblEatenBlack, _myColor);
    }

    private void HandleGameEnd(GameEndData end)
    {
        if (end.WhiteUser?.Id == Session.Id)
            Session.UpdateRating(end.WhiteUser.Rating);
        else if (end.BlackUser?.Id == Session.Id)
            Session.UpdateRating(end.BlackUser.Rating);

        bool won = end.Result switch
        {
            GameState.WhiteWin => Session.Color == PlayerColor.White,
            GameState.BlackWin => Session.Color == PlayerColor.Black,
            _ => false
        };

        string msg = end.ByForfeit
            ? (won ? "Opponent left. You win!" : "You left the room.")
            : won ? "You win!" : "Game finished: " + end.Result;

        MessageBox.Show(msg);
        Close();
    }

    private async void OnCellClick(object? sender, EventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not ValueTuple<int, int> pos)
            return;
        if (_state == null || _state.Result != GameState.InProgress) return;

        int row = pos.Item1, col = pos.Item2;
        int[,] board = _state.GetBoard();
        var figure = FigureCell.At(board, row, col);

        if (_pickRow == null && _state.ContinueCaptureRow.HasValue)
        {
            if (row != _state.ContinueCaptureRow || col != _state.ContinueCaptureCol)
                return;
        }

        if (_pickRow == null)
        {
            if (figure == null || figure.Color != Session.Color) return;
            if (_state.ContinueCaptureRow.HasValue &&
                (row != _state.ContinueCaptureRow || col != _state.ContinueCaptureCol))
                return;
            _pickRow = row;
            _pickCol = col;
            btn.Text = FigureCell.ToSymbol(figure.Type);
            btn.ForeColor = BoardFormHelper.PieceColor(figure.Type);
            btn.BackColor = Color.Gold;
            btn.FlatAppearance.MouseOverBackColor = Color.Orange;
            lblTurn.Text = "Piece selected";
            return;
        }

        if (row == _pickRow && col == _pickCol)
        {
            ApplyBoard(_state);
            return;
        }

        if (figure != null) return;

        try
        {
            await GameConnection.Instance.SendAsync(new NetworkPacket
            {
                CommandCode = PacketType.MoveSyncPacket,
                PacketTo = PacketWhere.ToServer,
                PResult = PacketResult.Success,
                Data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new MoveSyncData
                {
                    RoomId = _match.RoomId,
                    UserId = Session.Id,
                    Move = new MoveInfo
                    {
                        FromRow = _pickRow!.Value,
                        FromColumn = _pickCol!.Value,
                        ToRow = row,
                        ToColumn = col
                    }
                }))
            });
            _pickRow = _pickCol = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
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
                    UserId = Session.Id,
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
