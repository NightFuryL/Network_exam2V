namespace GameCore;

public class CurrentGameManager
{
    public int[,] Board { get; private set; } = BoardHelper.NewGameBoard();
    public PlayerColor Turn { get; private set; } = PlayerColor.White;
    public GameState Result { get; private set; } = GameState.InProgress;
    public int WhiteCaptured { get; private set; }
    public int BlackCaptured { get; private set; }
    public int QuietMoves { get; private set; }
    public int? ContinueRow { get; private set; }
    public int? ContinueColumn { get; private set; }

    public void Reset()
    {
        Board = BoardHelper.NewGameBoard();
        Turn = PlayerColor.White;
        Result = GameState.InProgress;
        WhiteCaptured = BlackCaptured = QuietMoves = 0;
        ContinueRow = ContinueColumn = null;
    }

    public void SetResult(GameState result) => Result = result;

    public bool TryMove(int fromRow, int fromCol, int toRow, int toCol, out string error, out bool captured)
    {
        error = "";
        captured = false;

        if (Result != GameState.InProgress)
        {
            error = "Игра окончена";
            return false;
        }

        FigureCell? piece = FigureCell.At(Board, fromRow, fromCol);
        if (piece == null)
        {
            error = "Нет фигуры";
            return false;
        }

        if (piece.Color != Turn && ContinueRow == null)
        {
            error = "Не ваш ход";
            return false;
        }

        var moves = GetMoves(Turn);
        if (ContinueRow != null)
            moves = moves.Where(m => m.FromRow == ContinueRow && m.FromColumn == ContinueColumn).ToList();

        var step = moves.FirstOrDefault(m =>
            m.FromRow == fromRow && m.FromColumn == fromCol && m.ToRow == toRow && m.ToColumn == toCol);

        if (step == null)
        {
            error = "Недопустимый ход";
            return false;
        }

        captured = Apply(step);
        CheckEnd();
        return true;
    }

    private bool Apply(MoveStep step)
    {
        FigureCell piece = FigureCell.At(Board, step.FromRow, step.FromColumn)!;
        Board[step.FromRow, step.FromColumn] = (int)CellType.Empty;

        bool captured = false;
        int dr = Math.Sign(step.ToRow - step.FromRow);
        int dc = Math.Sign(step.ToColumn - step.FromColumn);
        int r = step.FromRow + dr;
        int c = step.FromColumn + dc;

        while (r != step.ToRow || c != step.ToColumn)
        {
            if (Board[r, c] != (int)CellType.Empty)
            {
                Board[r, c] = (int)CellType.Empty;
                captured = true;
                if (piece.Color == PlayerColor.White) WhiteCaptured++;
                else BlackCaptured++;
                QuietMoves = 0;
            }
            r += dr;
            c += dc;
        }

        CellType finalType = piece.PromoteIfNeeded(step.ToRow);
        Board[step.ToRow, step.ToColumn] = (int)finalType;

        if (!captured) QuietMoves++;

        if (captured)
        {
            var more = GetMoves(Turn).Where(m => m.IsCapture && m.FromRow == step.ToRow && m.FromColumn == step.ToColumn).ToList();
            if (more.Count > 0)
            {
                ContinueRow = step.ToRow;
                ContinueColumn = step.ToColumn;
                return true;
            }
        }

        ContinueRow = ContinueColumn = null;
        Turn = Turn == PlayerColor.White ? PlayerColor.Black : PlayerColor.White;
        return captured;
    }

    private void CheckEnd()
    {
        if (QuietMoves >= AppSettings.DrawAfterQuietMoves)
        {
            Result = GameState.Draw;
            return;
        }

        if (Count(PlayerColor.Black) == 0) { Result = GameState.WhiteWin; return; }
        if (Count(PlayerColor.White) == 0) { Result = GameState.BlackWin; return; }

        if (GetMoves(Turn).Count == 0)
            Result = Turn == PlayerColor.White ? GameState.BlackWin : GameState.WhiteWin;
    }

    private int Count(PlayerColor color)
    {
        int n = 0;
        for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
        {
            var f = FigureCell.At(Board, r, c);
            if (f != null && f.Color == color) n++;
        }
        return n;
    }

    private List<MoveStep> GetMoves(PlayerColor who)
    {
        var list = new List<MoveStep>();
        for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
        {
            var f = FigureCell.At(Board, r, c);
            if (f == null || f.Color != who) continue;
            AddFrom(r, c, f, list);
        }

        if (list.Any(m => m.IsCapture))
            return list.Where(m => m.IsCapture).ToList();
        return list;
    }

    private void AddFrom(int row, int col, FigureCell piece, List<MoveStep> list)
    {
        int[] dirs = { -1, 1 };
        foreach (int dr in dirs)
        foreach (int dc in dirs)
        {
            if (piece.King == IsKing.No && piece.Color == PlayerColor.Black && dr < 0) continue;
            if (piece.King == IsKing.No && piece.Color == PlayerColor.White && dr > 0) continue;

            int max = piece.King == IsKing.Yes ? 7 : 1;
            for (int len = 1; len <= max; len++)
            {
                int tr = row + dr * len;
                int tc = col + dc * len;
                if (tr < 0 || tr > 7 || tc < 0 || tc > 7 || !BoardHelper.IsDark(tr, tc)) break;

                FigureCell? target = FigureCell.At(Board, tr, tc);
                if (target == null)
                {
                    if (len == 1 || piece.King == IsKing.Yes)
                        list.Add(new MoveStep(row, col, tr, tc, false));
                    continue;
                }

                if (!piece.IsEnemy(target)) break;

                int lr = tr + dr;
                int lc = tc + dc;
                if (piece.King == IsKing.No)
                {
                    if (lr >= 0 && lr < 8 && lc >= 0 && lc < 8 && Board[lr, lc] == (int)CellType.Empty)
                        list.Add(new MoveStep(row, col, lr, lc, true));
                    break;
                }

                while (lr >= 0 && lr < 8 && lc >= 0 && lc < 8 && BoardHelper.IsDark(lr, lc))
                {
                    if (Board[lr, lc] != (int)CellType.Empty) break;
                    list.Add(new MoveStep(row, col, lr, lc, true));
                    lr += dr;
                    lc += dc;
                }
                break;
            }
        }
    }

    private class MoveStep
    {
        public int FromRow { get; }
        public int FromColumn { get; }
        public int ToRow { get; }
        public int ToColumn { get; }
        public bool IsCapture { get; }

        public MoveStep(int fr, int fc, int tr, int tc, bool capture)
        {
            FromRow = fr;
            FromColumn = fc;
            ToRow = tr;
            ToColumn = tc;
            IsCapture = capture;
        }
    }
}
