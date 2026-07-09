namespace GameCore;

public static class CheckersEngine
{
    private static readonly (int dr, int dc)[] Diagonals =
    [
        (-1, -1), (-1, 1), (1, -1), (1, 1)
    ];

    public static int[,] CreateInitialBoard()
    {
        int[,] board = new int[8, 8];
        for (int r = 0; r < 3; r++)
        for (int c = 0; c < 8; c++)
            if (BoardHelper.IsDark(r, c))
                board[r, c] = (int)CellType.BlackPawn;

        for (int r = 5; r < 8; r++)
        for (int c = 0; c < 8; c++)
            if (BoardHelper.IsDark(r, c))
                board[r, c] = (int)CellType.WhitePawn;

        return board;
    }

    public static bool TryApplyMove(BoardState state, MoveInfo move, PlayerColor player)
    {
        if (state.Result != GameState.InProgress) return false;
        if (state.Turn != player) return false;

        int[,] board = state.GetBoard();
        var legal = GetLegalMoves(board, player, state.ContinueCaptureRow, state.ContinueCaptureCol);
        if (!legal.Any(m => m.FromRow == move.FromRow && m.FromColumn == move.FromColumn &&
                            m.ToRow == move.ToRow && m.ToColumn == move.ToColumn))
            return false;

        bool captured = ApplyMoveOnBoard(board, move, player);
        PromoteIfNeeded(board, move.ToRow, move.ToColumn);
        state.SetBoard(board);

        if (captured)
        {
            state.QuietMoves = 0;
            UpdateCapturedCounts(state);
            // Якщо пішак дійшов до дамки під час бою, далі він вже б'є як дамка.
            // Тут не мудрую: просто дивлюсь з нової клітинки, чи є ще кого забрати.
            if (HasCaptureFrom(board, move.ToRow, move.ToColumn, player))
            {
                state.ContinueCaptureRow = move.ToRow;
                state.ContinueCaptureCol = move.ToColumn;
                CheckEnd(state);
                return true;
            }
            state.ContinueCaptureRow = null;
            state.ContinueCaptureCol = null;
        }
        else
        {
            state.QuietMoves++;
            state.ContinueCaptureRow = null;
            state.ContinueCaptureCol = null;
        }

        state.Turn = player == PlayerColor.White ? PlayerColor.Black : PlayerColor.White;
        CheckEnd(state);
        return true;
    }

    public static List<MoveInfo> GetLegalMoves(int[,] board, PlayerColor color, int? onlyRow = null, int? onlyCol = null)
    {
        var captures = new List<MoveInfo>();
        var simple = new List<MoveInfo>();

        for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
        {
            if (onlyRow.HasValue && (r != onlyRow || c != onlyCol)) continue;
            if (!IsOwnPiece(board[r, c], color)) continue;
            CollectMovesFrom(board, r, c, color, captures, simple);
        }

        return captures.Count > 0 ? captures : onlyRow.HasValue ? captures : simple;
    }

    private static void CollectMovesFrom(int[,] board, int r, int c, PlayerColor color, List<MoveInfo> captures, List<MoveInfo> simple)
    {
        captures.AddRange(GetCapturesFrom(board, r, c, color));

        if (IsKing(board[r, c]))
        {
            foreach (var (dr, dc) in Diagonals)
            {
                int nr = r + dr, nc = c + dc;
                while (BoardHelper.IsInside(nr, nc) && board[nr, nc] == (int)CellType.Empty)
                {
                    simple.Add(new MoveInfo { FromRow = r, FromColumn = c, ToRow = nr, ToColumn = nc });
                    nr += dr;
                    nc += dc;
                }
            }
            return;
        }

        foreach (var (dr, dc) in GetManSimpleDirections(color))
        {
            int nr = r + dr, nc = c + dc;
            if (BoardHelper.IsInside(nr, nc) && board[nr, nc] == (int)CellType.Empty)
                simple.Add(new MoveInfo { FromRow = r, FromColumn = c, ToRow = nr, ToColumn = nc });
        }
    }

    private static bool ApplyMoveOnBoard(int[,] board, MoveInfo move, PlayerColor color)
    {
        var capture = GetCapturesFrom(board, move.FromRow, move.FromColumn, color)
            .FirstOrDefault(m => m.ToRow == move.ToRow && m.ToColumn == move.ToColumn);

        if (capture != null)
        {
            int dr = Math.Sign(move.ToRow - move.FromRow);
            int dc = Math.Sign(move.ToColumn - move.FromColumn);
            int r = move.FromRow + dr, c = move.FromColumn + dc;

            while (BoardHelper.IsInside(r, c) && board[r, c] == (int)CellType.Empty)
            {
                r += dr;
                c += dc;
            }

            board[move.ToRow, move.ToColumn] = board[move.FromRow, move.FromColumn];
            board[move.FromRow, move.FromColumn] = (int)CellType.Empty;
            board[r, c] = (int)CellType.Empty;
            return true;
        }

        board[move.ToRow, move.ToColumn] = board[move.FromRow, move.FromColumn];
        board[move.FromRow, move.FromColumn] = (int)CellType.Empty;
        return false;
    }

    private static void PromoteIfNeeded(int[,] board, int r, int c)
    {
        var type = (CellType)board[r, c];
        if (type == CellType.WhitePawn && r == 0)
            board[r, c] = (int)CellType.WhiteKing;
        else if (type == CellType.BlackPawn && r == 7)
            board[r, c] = (int)CellType.BlackKing;
    }

    private static bool HasCaptureFrom(int[,] board, int r, int c, PlayerColor color) =>
        GetCapturesFrom(board, r, c, color).Count > 0;

    private static List<MoveInfo> GetCapturesFrom(int[,] board, int r, int c, PlayerColor color)
    {
        var moves = new List<MoveInfo>();
        if (IsKing(board[r, c]))
        {
            CollectKingCaptures(board, r, c, color, moves);
            return moves;
        }

        foreach (var (dr, dc) in Diagonals)
        {
            int mr = r + dr, mc = c + dc;
            int lr = r + dr * 2, lc = c + dc * 2;
            if (!BoardHelper.IsInside(lr, lc)) continue;
            if (!IsEnemyPiece(board[mr, mc], color)) continue;
            if (board[lr, lc] != (int)CellType.Empty) continue;

            moves.Add(new MoveInfo { FromRow = r, FromColumn = c, ToRow = lr, ToColumn = lc });
        }

        return moves;
    }

    private static void CollectKingCaptures(int[,] board, int r, int c, PlayerColor color, List<MoveInfo> moves)
    {
        foreach (var (dr, dc) in Diagonals)
        {
            int nr = r + dr, nc = c + dc;

            while (BoardHelper.IsInside(nr, nc) && board[nr, nc] == (int)CellType.Empty)
            {
                nr += dr;
                nc += dc;
            }

            if (!BoardHelper.IsInside(nr, nc) || !IsEnemyPiece(board[nr, nc], color))
                continue;

            int landR = nr + dr, landC = nc + dc;
            while (BoardHelper.IsInside(landR, landC) && board[landR, landC] == (int)CellType.Empty)
            {
                moves.Add(new MoveInfo { FromRow = r, FromColumn = c, ToRow = landR, ToColumn = landC });
                landR += dr;
                landC += dc;
            }
        }
    }

    private static IEnumerable<(int dr, int dc)> GetManSimpleDirections(PlayerColor color)
    {
        int forward = color == PlayerColor.White ? -1 : 1;
        yield return (forward, -1);
        yield return (forward, 1);
    }

    private static bool IsKing(int cell) =>
        cell is (int)CellType.WhiteKing or (int)CellType.BlackKing;

    private static bool IsOwnPiece(int cell, PlayerColor color) =>
        color == PlayerColor.White
            ? cell is (int)CellType.WhitePawn or (int)CellType.WhiteKing
            : cell is (int)CellType.BlackPawn or (int)CellType.BlackKing;

    private static bool IsEnemyPiece(int cell, PlayerColor color) =>
        cell != (int)CellType.Empty && !IsOwnPiece(cell, color);

    private static void UpdateCapturedCounts(BoardState state)
    {
        int[,] board = state.GetBoard();
        int white = 0, black = 0;
        for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
        {
            if (board[r, c] is (int)CellType.WhitePawn or (int)CellType.WhiteKing) white++;
            if (board[r, c] is (int)CellType.BlackPawn or (int)CellType.BlackKing) black++;
        }
        state.WhiteCaptured = 12 - white;
        state.BlackCaptured = 12 - black;
    }

    private static void CheckEnd(BoardState state)
    {
        if (state.Result != GameState.InProgress) return;

        if (state.QuietMoves >= AppSettings.DrawAfterQuietMoves)
        {
            state.Result = GameState.Draw;
            return;
        }

        int[,] board = state.GetBoard();
        int white = CountPieces(board, PlayerColor.White);
        int black = CountPieces(board, PlayerColor.Black);
        if (white == 0) { state.Result = GameState.BlackWin; return; }
        if (black == 0) { state.Result = GameState.WhiteWin; return; }

        if (state.ContinueCaptureRow.HasValue) return;

        var nextTurn = state.Turn;
        if (GetLegalMoves(board, nextTurn).Count == 0)
            state.Result = nextTurn == PlayerColor.White ? GameState.BlackWin : GameState.WhiteWin;
    }

    private static int CountPieces(int[,] board, PlayerColor color)
    {
        int count = 0;
        for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
            if (IsOwnPiece(board[r, c], color)) count++;
        return count;
    }
}
