namespace GameCore;

public static class Session
{
    public static int Id;
    public static string Name = "";
    public static int Points;
    public static PlayerColor? Color;
    public static ClientType Role;
    public static int RoomId;
}

public class MoveInfo
{
    public int FromRow { get; set; }
    public int FromColumn { get; set; }
    public int ToRow { get; set; }
    public int ToColumn { get; set; }
}

public class BoardState
{
    public string BoardLine { get; set; } = "";
    public PlayerColor Turn { get; set; }
    public GameState Result { get; set; }
    public int? ContinueRow { get; set; }
    public int? ContinueColumn { get; set; }
    public int WhiteCaptured { get; set; }
    public int BlackCaptured { get; set; }

    public int[,] GetBoard() => BoardHelper.FromLine(BoardLine);

    public static BoardState FromGame(CurrentGameManager game) => new()
    {
        BoardLine = BoardHelper.ToLine(game.Board),
        Turn = game.Turn,
        Result = game.Result,
        ContinueRow = game.ContinueRow,
        ContinueColumn = game.ContinueColumn,
        WhiteCaptured = game.WhiteCaptured,
        BlackCaptured = game.BlackCaptured
    };
}
