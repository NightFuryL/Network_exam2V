namespace GameCore;

public class FigureCell
{
    public CellType Type { get; }
    public PlayerColor Color { get; }
    public IsKing King { get; } 

    public FigureCell(CellType type)
    {
        Type = type;
        King = type is CellType.BlackKing or CellType.WhiteKing ? IsKing.Yes : IsKing.No;
        Color = type is CellType.BlackPawn or CellType.BlackKing ? PlayerColor.Black : PlayerColor.White;
    }

    public static FigureCell? At(int[,] board, int row, int col)
    {
        var type = (CellType)board[row, col];
        return type == CellType.Empty ? null : new FigureCell(type);
    }

    public static void Set(ref int[,] board, int row, int col, FigureCell? figure) =>
        board[row, col] = ((figure == null) ? (int)CellType.Empty : (int)figure.Type);

    public bool IsEnemy(FigureCell other) => Color != other.Color;

    public CellType PromoteIfNeeded(int toRow)
    {
        if (King == IsKing.Yes)
            return Type;
        if (Color == PlayerColor.Black && toRow == 7)
            return CellType.BlackKing;
        if (Color == PlayerColor.White && toRow == 0)
            return CellType.WhiteKing;
        return Type;
    }

    public static string ToEmoji(CellType type) => type switch
    {
        CellType.BlackPawn => "⚫",
        CellType.BlackKing => "♚",
        CellType.WhitePawn => "⚪",
        CellType.WhiteKing => "♔",
        _ => ""
    };
}
