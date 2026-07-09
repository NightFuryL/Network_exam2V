using System.Text;
using System.Text.Json;
using GameCore;

namespace ClientWFTask;

public static class BoardFormHelper
{
    public static readonly Color AppBlue = Color.FromArgb(7, 22, 58);
    public static readonly Color AppBlueSoft = Color.FromArgb(15, 44, 92);
    public static readonly Color AppYellow = Color.FromArgb(255, 205, 0);
    public static readonly Color AppText = Color.FromArgb(245, 248, 255);
    public static readonly Color AppDanger = Color.FromArgb(196, 43, 43);

    public static Color CellBaseColor(int row, int col) =>
        BoardHelper.IsDark(row, col) ? Color.FromArgb(24, 61, 121) : Color.FromArgb(255, 224, 99);

    public static Color CellHoverColor(int row, int col) =>
        BoardHelper.IsDark(row, col) ? Color.FromArgb(36, 88, 164) : Color.FromArgb(255, 236, 145);

    public static void StyleForm(Form form)
    {
        form.BackColor = AppBlue;
        form.ForeColor = AppText;
        form.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        form.StartPosition = FormStartPosition.CenterScreen;
        form.FormBorderStyle = FormBorderStyle.FixedSingle;
        form.MaximizeBox = false;
    }

    public static void StyleLabel(Label label, bool accent = false)
    {
        label.ForeColor = accent ? AppYellow : AppText;
        label.BackColor = Color.Transparent;
        label.Font = new Font("Segoe UI", accent ? 12F : 10F, accent ? FontStyle.Bold : FontStyle.Regular);
    }

    public static void StyleList(ListBox list)
    {
        list.BackColor = Color.FromArgb(10, 30, 72);
        list.ForeColor = AppText;
        list.BorderStyle = BorderStyle.FixedSingle;
        list.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
    }

    public static void StyleFormButton(Button btn)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.UseVisualStyleBackColor = false;
        btn.BackColor = AppYellow;
        btn.ForeColor = AppBlue;
        btn.FlatAppearance.BorderSize = 1;
        btn.FlatAppearance.BorderColor = Color.FromArgb(255, 232, 119);
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 221, 66);
        btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(229, 177, 0);
        btn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
    }

    public static void BuildBoard(Panel panel, Button[,] cells, bool spectator, EventHandler? clickHandler, bool flip = false)
    {
        panel.Controls.Clear();
        panel.BackColor = AppBlueSoft;
        int size = 48;

        for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
        {
            var btn = new Button
            {
                Size = new Size(size, size),
                Location = new Point((flip ? 7 - c : c) * size, (flip ? 7 - r : r) * size),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Symbol", 26F, FontStyle.Bold),
                UseVisualStyleBackColor = false,
                Tag = (r, c),
                BackColor = CellBaseColor(r, c),
                ForeColor = Color.Black,
                Text = "",
                Enabled = BoardHelper.IsDark(r, c) && !spectator
            };

            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(6, 18, 46);
            btn.FlatAppearance.MouseOverBackColor = CellHoverColor(r, c);
            btn.FlatAppearance.MouseDownBackColor = AppYellow;

            if (clickHandler != null)
                btn.Click += clickHandler;

            cells[r, c] = btn;
            panel.Controls.Add(btn);
        }
    }

    public static Color PieceColor(CellType type) => type switch
    {
        CellType.WhitePawn => Color.White,
        CellType.WhiteKing => Color.White,
        CellType.BlackPawn => Color.Black,
        CellType.BlackKing => Color.Black,
        _ => Color.Transparent
    };

    public static string PlayerName(PlayerColor color) =>
        color == PlayerColor.White ? "white" : "black";

    public static void Draw(
        BoardState state,
        Button[,] cells,
        Label lblTurn,
        Label lblEatenWhite,
        Label lblEatenBlack,
        PlayerColor? selfColor = null)
    {
        int[,] board = state.GetBoard();

        for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
        {
            var cellType = (CellType)board[r, c];
            var btn = cells[r, c];

            btn.Text = FigureCell.ToSymbol(cellType);

            btn.ForeColor = PieceColor(cellType);

            btn.Font = new Font("Segoe UI Symbol", FigureCell.IsKing(cellType) ? 26F : 24F, FontStyle.Bold);

            btn.BackColor = CellBaseColor(r, c);
            btn.FlatAppearance.MouseOverBackColor = CellHoverColor(r, c);
        }

        lblEatenWhite.Text = $"Captured white: {state.WhiteCaptured}";
        lblEatenBlack.Text = $"Captured black: {state.BlackCaptured}";

        if (state.Result != GameState.InProgress)
        {
            lblTurn.Text = "Result: " + state.Result;
            return;
        }

        string side = PlayerName(state.Turn);
        if (!selfColor.HasValue)
            lblTurn.Text = $"Turn: {side}";
        else
            lblTurn.Text = state.Turn == selfColor.Value ? $"Turn: you ({side})" : $"Turn: opponent ({side})";
    }



    public static async Task FlashInvalid(Button[,] cells, int row, int col)

    {

        var btn = cells[row, col];

        var orig = btn.BackColor;

        for (int i = 0; i < 3; i++)

        {

            btn.BackColor = AppDanger;

            await Task.Delay(120);

            btn.BackColor = orig;

            await Task.Delay(120);

        }

    }



    public static MoveSyncData? ParseMoveSync(NetworkPacket packet)

    {

        if (packet.CommandCode != PacketType.MoveSyncPacket) return null;

        return JsonSerializer.Deserialize<MoveSyncData>(Encoding.UTF8.GetString(packet.Data));

    }



    public static BoardState? ParseBoardState(NetworkPacket packet)

    {

        if (packet.CommandCode != PacketType.BoardStatePacket) return null;

        return JsonSerializer.Deserialize<BoardState>(Encoding.UTF8.GetString(packet.Data));

    }



    public static MatchStartedData? ParseMatchStarted(NetworkPacket packet)

    {

        if (packet.CommandCode != PacketType.MatchStartedPacket) return null;

        return JsonSerializer.Deserialize<MatchStartedData>(Encoding.UTF8.GetString(packet.Data));

    }



    public static GameEndData? ParseGameEnd(NetworkPacket packet)

    {

        if (packet.CommandCode != PacketType.GameEndPacket) return null;

        return JsonSerializer.Deserialize<GameEndData>(Encoding.UTF8.GetString(packet.Data));

    }

}


