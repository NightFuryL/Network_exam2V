namespace GameCore;

public class BoardState
{
    public int RoomId { get; set; }
    public int[] Cells { get; set; } = new int[64];
    public PlayerColor Turn { get; set; } = PlayerColor.White;
    public GameState Result { get; set; } = GameState.InProgress;
    public int WhiteCaptured { get; set; }
    public int BlackCaptured { get; set; }
    public int QuietMoves { get; set; }
    public int? ContinueCaptureRow { get; set; }
    public int? ContinueCaptureCol { get; set; }

    public int[,] GetBoard()
    {
        int[,] board = new int[8, 8];
        for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
            board[r, c] = Cells[r * 8 + c];
        return board;
    }

    public void SetBoard(int[,] board)
    {
        for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
            Cells[r * 8 + c] = board[r, c];
    }

    public static BoardState CreateInitial(int roomId)
    {
        int[,] board = CheckersEngine.CreateInitialBoard();
        var state = new BoardState { RoomId = roomId };
        state.SetBoard(board);
        return state;
    }
}
