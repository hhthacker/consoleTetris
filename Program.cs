 using System.Diagnostics;
 
 namespace ConsoleTetris;

internal abstract class Program
{
    const int Width = 10, Height = 20;
    private static readonly int[,] Grid = new int[Height, Width];
    private static Tetromino? _current;
    private static Tetromino? _next;
    private static int _score, _level = 1, _linesCleared;
    private static bool _gameOver;

    static void Main()
    {
        Console.CursorVisible = false;


        _next = Tetromino.Random();
        NewTetromino();
        new Thread(InputThread) { IsBackground = true }.Start();

        while (!_gameOver)
        {
            Draw();
            Thread.Sleep(Math.Max(50, 500 - _level * 40));
            Step();
        }

        Console.SetCursorPosition(0, Height + 4);
        Console.WriteLine("GAME OVER!");
    }

    static void NewTetromino()
    {
        _current = _next;
        _current!.X = Width / 2 - 2;
        _current.Y = 0;
        _next = Tetromino.Random();

        if (!IsValid(_current)) _gameOver = true;
    }

    static void Step()
    {
        var moved = _current!.Clone();
        moved.Y++;
        if (IsValid(moved))
            _current = moved;
        else
        {
            Merge(_current);
            ClearLines();
            NewTetromino();
        }
    }

    static void InputThread()
    {
        while (!_gameOver)
        {
            var key = Console.ReadKey(true).Key;
            var clone = _current!.Clone();
            if (key == ConsoleKey.LeftArrow) clone.X--;
            else if (key == ConsoleKey.RightArrow) clone.X++;
            else if (key == ConsoleKey.DownArrow) clone.Y++;
            else if (key == ConsoleKey.UpArrow) clone.Rotate();
            else if (key == ConsoleKey.Spacebar) // <- THIS is the correct name
            {
                while (IsValid(clone)) { _current = clone.Clone(); clone.Y++; }
                Step();
                continue;
            }


            if (IsValid(clone)) _current = clone;
        }
    }

    static void Draw()
    {
        Console.SetCursorPosition(0, 0);

        // Draw top border
        Console.Write("╔");
        for (int x = 0; x < Width * 2; x++) Console.Write("═");
        Console.WriteLine("╗");

        // Draw each row with side borders
        for (int y = 0; y < Height; y++)
        {
            Console.Write("║");
            for (int x = 0; x < Width; x++)
            {
                if (Grid[y, x] == 0)
                {
                    Console.Write("  "); // empty
                }
                else
                {
                    Console.ForegroundColor = GetColor(Grid[y, x]);
                    Console.Write("██"); // filled
                    Console.ResetColor();
                }
            }
            Console.WriteLine("║");
        }

        // Draw bottom border
        Console.Write("╚");
        for (int x = 0; x < Width * 2; x++) Console.Write("═");
        Console.WriteLine("╝");

        // Draw the current piece
        foreach (var (x, y) in _current!.Shape)
        {
            int px = _current.X + x;
            int py = _current.Y + y;
            if (py >= 0 && px is >= 0 and < Width && py < Height)
            {
                Console.SetCursorPosition(1 + px * 2, 1 + py);
                Console.ForegroundColor = GetColor((int)_current.Type + 1);
                Console.Write("██");
                Console.ResetColor();
            }
        }

        // Draw the ghost piece
        var ghost = _current.Clone();
        while (IsValid(ghost))
        {
            Debug.Assert(ghost != null, nameof(ghost) + " != null");
            ghost.Y++;
        }

        ghost.Y--;
        foreach (var (x, y) in ghost.Shape)
        {
            int px = ghost.X + x; 
            int py = ghost.Y + y;
            if (py >= 0 && px is >= 0 and < Width && py < Height)
            {
                Console.SetCursorPosition(1 + px * 2, 1 + py);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("▒▒");
                Console.ResetColor();
            }
        }

        // Draw score
        Console.SetCursorPosition(Width * 2 + 5, 2);
        Console.Write($"Score: {_score}");

        // Draw the next piece label
        Console.SetCursorPosition(Width * 2 + 5, 5);
        Console.Write("Next:");

        // Clear preview area
        int previewOffsetX = Width * 2 + 5;
        int previewOffsetY = 7;
        for (int y = 0; y < 4; y++)
        {
            Console.SetCursorPosition(previewOffsetX, previewOffsetY + y);
            Console.Write("        "); // 4 blocks * 2 spaces
        }

        // Draw the next piece
        var preview = _next!.Shape;
        int minX = preview.Min(p => p.x);
        int minY = preview.Min(p => p.y);

        foreach (var (x, y) in preview)
        {
            Console.SetCursorPosition(previewOffsetX + (x - minX) * 2, previewOffsetY + (y - minY));
            Console.ForegroundColor = GetColor((int)_next.Type + 1);
            Console.Write("██");
            Console.ResetColor();
        }
    }
   
    static ConsoleColor GetColor(int t) => t switch
    {
        1 => ConsoleColor.Cyan,
        2 => ConsoleColor.Yellow,
        3 => ConsoleColor.Magenta,
        4 => ConsoleColor.Green,
        5 => ConsoleColor.Red,
        6 => ConsoleColor.Blue,
        7 => ConsoleColor.DarkYellow,
        _ => ConsoleColor.Gray 
    };

    static bool IsValid(Tetromino? t)
    {
        Debug.Assert(t != null, nameof(t) + " != null");
        foreach (var (x, y) in t.Cells())
            if (x < 0 || x >= Width || y >= Height || (y >= 0 && Grid[y, x] != 0))
                return false;
        return true;
    }

    static void Merge(Tetromino? t)
    {
        Debug.Assert(t != null, nameof(t) + " != null");
        foreach (var (x, y) in t.Cells())
            if (y >= 0) Grid[y, x] = (int)t.Type + 1;
    }

    static void ClearLines()
    {
        int cleared = 0;
        for (int y = Height - 1; y >= 0; y--)
        {
            bool full = true;
            for (int x = 0; x < Width; x++)
                if (Grid[y, x] == 0) full = false;

            if (full)
            {
                for (int yy = y; yy > 0; yy--)
                for (int x = 0; x < Width; x++)
                    Grid[yy, x] = Grid[yy - 1, x];
                cleared++;
                y++; // Re-check the same row after collapsing
            }
        }

        if (cleared > 0)
        {
            int points = cleared switch
            {
                1 => 100,
                2 => 300,
                3 => 500,
                4 => 800,
                _ => cleared * 200 // for >4 lines somehow
            };

            _score += points;
            _linesCleared += cleared;
            _level = 1 + _linesCleared / 5;
        }
    }
 
}