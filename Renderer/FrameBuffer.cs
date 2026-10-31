public class FrameBuffer
{
    private readonly Cell[,] cells;

    public static readonly Cell Empty = new(' ', ConsoleColor.Gray);

    public int Width { get; }
    public int Height { get; }

    public FrameBuffer(int width, int height)
    {
        Width = width;
        Height = height;

        cells = new Cell[height, width];
        Clear();
    }

    public void Clear()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                cells[y, x] = Empty;
            }
        }
    }

    public void SetCell(int x, int y, Cell cell)
    {
        if (x < 0 || x >= Width)
            throw new ArgumentOutOfRangeException(nameof(x));
        if (y < 0 || y >= Height)
            throw new ArgumentOutOfRangeException(nameof(y));

        cells[y, x] = cell;
    }

    public Cell GetCell(int x, int y)
    {
        return cells[y, x];
    }
}

public readonly record struct Cell(
    char Character,
    ConsoleColor Color
);