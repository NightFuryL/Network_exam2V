namespace GameCore;

public static class BoardHelper
{
    public static bool IsDark(int row, int col) => (row + col) % 2 == 1;

    public static bool IsInside(int row, int col) =>
        row >= 0 && row < 8 && col >= 0 && col < 8;
}
