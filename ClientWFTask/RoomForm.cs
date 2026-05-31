using ClientWFTask.Services;
using GameCore;

namespace ClientWFTask;

public partial class RoomForm : Form
{
    private readonly MatchStartedPacket _match;
    private readonly bool _spectator;
    private readonly Button[,] _cells = new Button[8, 8];
    private readonly Label _lblEatenWhite;
    private readonly Label _lblEatenBlack;
    private int? _pickRow, _pickCol;
    private BoardState? _state;

    public RoomForm(MatchStartedPacket match)
    {
        _match = match;
        _spectator = match.Role == ClientType.Watcher;
        InitializeComponent();

        lblPlayer_1.Text = "⚪ " + match.WhiteName;
        lblPlayer_2.Text = "⚫ " + match.BlackName;

        _lblEatenWhite = new Label
        {
            AutoSize = true,
            Location = new Point(504, 50),
            Text = "Съели (белые): 0"
        };
        _lblEatenBlack = new Label
        {
            AutoSize = true,
            Location = new Point(504, 370),
            Text = "Съели (чёрные): 0"
        };
        Controls.Add(_lblEatenWhite);
        Controls.Add(_lblEatenBlack);

        BuildBoard();
        GameConnection.Instance.PacketReceived += OnPacket;
    }

    private void BuildBoard()
    {
        panel1.Controls.Clear();
        int size = 48;
        for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
        {
            var btn = new Button
            {
                Size = new Size(size, size),
                Location = new Point(c * size, r * size),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 14F),
                Tag = (r, c),
                BackColor = BoardHelper.IsDark(r, c) ? Color.SaddleBrown : Color.Wheat,
                Enabled = BoardHelper.IsDark(r, c) && !_spectator
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += OnCellClick;
            _cells[r, c] = btn;
            panel1.Controls.Add(btn);
        }
    }

    private void OnPacket(INetworkPacket packet)
    {
        if (packet is BoardStatePacket board && _match.RoomId > 0)
        {
            if (InvokeRequired) { BeginInvoke(() => Draw(board.State)); return; }
            Draw(board.State);
        }
        else if (packet is MoveSyncPacket move && !move.Success)
        {
            if (InvokeRequired) { BeginInvoke(() => MessageBox.Show(move.Error)); return; }
            MessageBox.Show(move.Error);
        }
        else if (packet is GameStatePacket end)
        {
            if (InvokeRequired) { BeginInvoke(() => { MessageBox.Show("Конец: " + end.Result); Close(); }); return; }
            MessageBox.Show("Конец: " + end.Result);
            Close();
        }
    }

    private void Draw(BoardState state)
    {
        _state = state;
        _pickRow = _pickCol = null;
        int[,] board = state.GetBoard();

        for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
            _cells[r, c].Text = FigureCell.ToEmoji((CellType)board[r, c]);

        _lblEatenWhite.Text = $"Съели (белые): {state.WhiteCaptured}";
        _lblEatenBlack.Text = $"Съели (чёрные): {state.BlackCaptured}";
        lblTurn.Text = state.Result != GameState.InProgress
            ? "Итог: " + state.Result
            : $"Ход: {state.Turn}";
    }

    private void OnCellClick(object? sender, EventArgs e)
    {
        if (_spectator || sender is not Button btn || btn.Tag is not ValueTuple<int, int> pos)
            return;
        if (_state == null || _state.Result != GameState.InProgress) return;

        int row = pos.Item1, col = pos.Item2;
        int[,] board = _state.GetBoard();
        var figure = FigureCell.At(board, row, col);

        if (_pickRow == null)
        {
            if (figure == null || figure.Color != Session.Color) return;
            _pickRow = row;
            _pickCol = col;
            btn.Text = FigureCell.ToEmoji(figure.Type) + "^";
            lblTurn.Text = "Взял ^";
            return;
        }

        if (row == _pickRow && col == _pickCol)
        {
            Draw(_state);
            return;
        }

        if (figure != null) return;

        GameConnection.Instance.Send(new MoveSyncPacket
        {
            RoomId = _match.RoomId,
            Move = new MoveInfo
            {
                FromRow = _pickRow.Value,
                FromColumn = _pickCol.Value,
                ToRow = row,
                ToColumn = col
            }
        });
    }

    private void RoomForm_Load(object sender, EventArgs e)
    {
        if (_spectator)
            lblTurn.Text = "Наблюдатель";
    }

    private void btnExit_Click(object sender, EventArgs e)
    {
        GameConnection.Instance.Send(new ManageConnectionPacket
        {
            RoomId = _match.RoomId,
            ConnectionType = ConnectionType.Exit
        });
        Close();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        GameConnection.Instance.PacketReceived -= OnPacket;
        Session.RoomId = 0;
        base.OnFormClosed(e);
    }
}
