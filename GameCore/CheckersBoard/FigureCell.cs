namespace GameCore;

public class FigureCell
{
    public CellType Type { get; set; }
    public PlayerColor Color { get; set; }

    public static FigureCell? At(int[,] board, int row, int col)
    {
        if (!BoardHelper.IsInside(row, col)) return null;
        var type = (CellType)board[row, col];
        if (type == CellType.Empty) return null;
        return new FigureCell
        {
            Type = type,
            Color = type is CellType.WhitePawn or CellType.WhiteKing ? PlayerColor.White : PlayerColor.Black
        };
    }

    public static string ToSymbol(CellType type) => type switch
    {
        CellType.WhitePawn => "\u25CF",
        CellType.WhiteKing => "\u2655",
        CellType.BlackPawn => "\u25CF",
        CellType.BlackKing => "\u265B",
        _ => ""
    };

    public static bool IsKing(CellType type) =>
        type is CellType.BlackKing or CellType.WhiteKing;
}
