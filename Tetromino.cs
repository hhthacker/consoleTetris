namespace ConsoleTetris;

internal enum TetrominoType { I, O, T, S, Z, J, L }

class Tetromino(TetrominoType type, List<(int, int)> shape)
{
    public readonly TetrominoType Type = type;
    public List<(int x, int y)> Shape = shape;
    public int X, Y;

    public static Tetromino Random()
    {
        var types = Enum.GetValues<TetrominoType>();
        var rand = new Random();
        return Create(types[rand.Next(types.Length)]);
    }

    private static Tetromino Create(TetrominoType type) => type switch
    {
        TetrominoType.I => new(type, [(0, 1), (1, 1), (2, 1), (3, 1)]),
        TetrominoType.O => new(type, [(1, 0), (2, 0), (1, 1), (2, 1)]),
        TetrominoType.T => new(type, [(1, 0), (0, 1), (1, 1), (2, 1)]),
        TetrominoType.S => new(type, [(1, 0), (2, 0), (0, 1), (1, 1)]),
        TetrominoType.Z => new(type, [(0, 0), (1, 0), (1, 1), (2, 1)]),
        TetrominoType.J => new(type, [(0, 0), (0, 1), (1, 1), (2, 1)]),
        TetrominoType.L => new(type, [(2, 0), (0, 1), (1, 1), (2, 1)]),
        _ => throw new ArgumentException()
    };

    public void Rotate()
    {
        var newShape = new List<(int, int)>();
        foreach (var (x, y) in Shape)
            newShape.Add((1 - y, x)); // rotate 90°
        Shape = newShape;
    }

    public IEnumerable<(int x, int y)> Cells()
    {
        foreach (var (dx, dy) in Shape)
            yield return (X + dx, Y + dy);
    }

    public Tetromino Clone() => new(Type, [..Shape]) { X = X, Y = Y };
}