namespace GomokuPlus.Models;

public class ComputerPlayer : Player
{
    private readonly Random _random = new();

    public ComputerPlayer(string name, char ordinarySymbol, char heavySymbol)
        : base(name, ordinarySymbol, heavySymbol)
    {
    }

    // Simplest strategy that's always legal: place an ordinary stone on a
    // random empty cell. This is a deliberate baseline, not a placeholder —
    // swapping in a smarter strategy later only means changing this one
    // method, since nothing else depends on how the computer picks moves.
    public override Move GetNextMove(Board board)
    {
        var emptyCells = new List<Position>();
        for (var row = 0; row < board.Rows; row++)
        {
            for (var col = 0; col < board.Columns; col++)
            {
                var position = new Position(row, col);
                if (board.IsEmpty(position))
                    emptyCells.Add(position);
            }
        }

        var target = emptyCells[_random.Next(emptyCells.Count)];
        return new Move(MoveKind.Ordinary, target);
    }
}
