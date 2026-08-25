using System.Text;

namespace GomokuPlus.Models;

public class Board
{
    private readonly Cell?[,] _cells;

    public int Rows { get; }
    public int Columns { get; }

    public Board(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        _cells = new Cell?[rows, columns];
    }

    public bool IsInBounds(Position p) =>
        p.Row >= 0 && p.Row < Rows && p.Col >= 0 && p.Col < Columns;

    public bool IsEmpty(Position p) => IsInBounds(p) && _cells[p.Row, p.Col] is null;

    public Cell? GetCell(Position p) => IsInBounds(p) ? _cells[p.Row, p.Col] : null;

    // The only way to mutate the grid is through an explicit, validated method.
    public bool TryPlace(Position p, Cell cell)
    {
        if (!IsEmpty(p)) return false;
        _cells[p.Row, p.Col] = cell;
        return true;
    }

    public bool TryRemove(Position p)
    {
        if (IsInBounds(p) && _cells[p.Row, p.Col] is not null)
        {
            _cells[p.Row, p.Col] = null;
            return true;
        }
        return false;
    }

    // Renders the grid as a bordered ASCII table with 1-based row/column
    // numbers, e.g. for a 3x3 board:
    //
    //       1   2   3
    //     +---+---+---+
    //   1 | . | . | . |
    //     +---+---+---+
    //   2 | . | . | . |
    //     +---+---+---+
    //   3 | . | . | . |
    //     +---+---+---+
    //
    // Returns a plain string — no Console calls here — so the presentation
    // layer decides where/whether to print it.
    public string Render()
    {
        var border = "   +" + string.Concat(Enumerable.Repeat("---+", Columns));
        var sb = new StringBuilder();

        sb.Append("   ");
        for (var col = 1; col <= Columns; col++)
            sb.Append($"{col,3} ");
        sb.AppendLine();

        sb.AppendLine(border);
        for (var row = 0; row < Rows; row++)
        {
            sb.Append($"{row + 1,2} |");
            for (var col = 0; col < Columns; col++)
                sb.Append($" {_cells[row, col]?.Symbol ?? '.'} |");
            sb.AppendLine();
            sb.AppendLine(border);
        }

        return sb.ToString();
    }

    private static readonly (int DRow, int DCol)[] WinDirections =
    {
        (0, 1),  // horizontal
        (1, 0),  // vertical
        (1, 1),  // diagonal ↘
        (1, -1), // diagonal ↙
    };

    // True if the stone just placed at `lastMove` completes `winLength` in
    // a row, by the same owner, in any direction. Ordinary and Heavy stones
    // count the same for this check — only who owns the cell matters.
    // Only lines through lastMove are checked: a single placement can only
    // ever create a new win through the cell it was just placed in, so
    // scanning the whole board every turn would be wasted work.
    public bool CheckWin(Position lastMove, int winLength = 5)
    {
        if (GetCell(lastMove)?.Owner is not { } owner) return false;

        foreach (var (dRow, dCol) in WinDirections)
        {
            var count = 1
                + CountInDirection(lastMove, dRow, dCol, owner)
                + CountInDirection(lastMove, -dRow, -dCol, owner);
            if (count >= winLength) return true;
        }
        return false;
    }

    private int CountInDirection(Position from, int dRow, int dCol, Player owner)
    {
        var count = 0;
        var pos = new Position(from.Row + dRow, from.Col + dCol);
        while (IsInBounds(pos) && _cells[pos.Row, pos.Col]?.Owner == owner)
        {
            count++;
            pos = new Position(pos.Row + dRow, pos.Col + dCol);
        }
        return count;
    }

    // No empty cells left — used to detect a draw.
    public bool IsFull()
    {
        for (var row = 0; row < Rows; row++)
            for (var col = 0; col < Columns; col++)
                if (_cells[row, col] is null)
                    return false;
        return true;
    }
}
