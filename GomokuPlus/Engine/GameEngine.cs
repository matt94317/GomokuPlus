using GomokuPlus.Models;

namespace GomokuPlus.Engine;

// Orchestrates a two-player game: alternates turns, asks the current player
// for a move, validates and applies it to the board, checks for a win/draw
// after every placement, and prints the board. This is the only class that
// prints board state — Board/Cell/Player stay Console-free so the rules
// they implement can be exercised without a terminal attached.
public class GameEngine
{
    private const int WinLength = 5;

    private readonly Board _board;
    private readonly Player _playerOne;
    private readonly Player _playerTwo;

    public GameEngine(Board board, Player playerOne, Player playerTwo)
    {
        _board = board;
        _playerOne = playerOne;
        _playerTwo = playerTwo;
    }

    public void Run()
    {
        var current = _playerOne;

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine(_board.Render());

            var move = current.GetNextMove(_board);
            var (success, placedStone, error) = Apply(current, move);

            if (!success)
            {
                // Same player retries — the turn only advances on success.
                Console.WriteLine($"Illegal move: {error}");
                continue;
            }

            if (placedStone && _board.CheckWin(move.Target, WinLength))
            {
                Console.WriteLine();
                Console.WriteLine(_board.Render());
                Console.WriteLine($"{current.Name} wins!");
                return;
            }

            if (_board.IsFull())
            {
                Console.WriteLine();
                Console.WriteLine(_board.Render());
                Console.WriteLine("Board is full — it's a draw.");
                return;
            }

            current = current == _playerOne ? _playerTwo : _playerOne;
        }
    }

    // Automated testing mode: applies a pre-parsed sequence of moves with
    // no prompts and no per-turn printing, then renders the board once at
    // the end. Reuses the exact same Apply/CheckWin/IsFull rules as Run(),
    // so a scripted game and an interactive one are judged identically —
    // only the move source and the print cadence differ.
    public void RunScripted(IReadOnlyList<Move> moves)
    {
        var current = _playerOne;

        foreach (var move in moves)
        {
            var (success, placedStone, error) = Apply(current, move);

            if (!success)
            {
                // Doesn't advance the turn, same as an illegal interactive move.
                Console.WriteLine($"Illegal move {Describe(move)}: {error}");
                continue;
            }

            if (placedStone && _board.CheckWin(move.Target, WinLength))
            {
                Console.WriteLine(_board.Render());
                Console.WriteLine($"{current.Name} wins!");
                return;
            }

            if (_board.IsFull())
            {
                Console.WriteLine(_board.Render());
                Console.WriteLine("Board is full — it's a draw.");
                return;
            }

            current = current == _playerOne ? _playerTwo : _playerOne;
        }

        Console.WriteLine(_board.Render());
    }

    private static string Describe(Move move) =>
        $"{move.Kind} {move.Target.Row + 1}:{move.Target.Col + 1}";

    // Returns whether the move was legal, whether it placed a stone (as
    // opposed to erasing one — only a placement can trigger a win check),
    // and an error message to show the player when it wasn't legal.
    private (bool Success, bool PlacedStone, string? Error) Apply(Player player, Move move)
    {
        if (!_board.IsInBounds(move.Target))
            return (false, false, "Target is off the board.");

        return move.Kind switch
        {
            MoveKind.Ordinary => ApplyPlacement(player, move.Target, heavy: false),
            MoveKind.Heavy => ApplyPlacement(player, move.Target, heavy: true),
            MoveKind.Eraser => ApplyEraser(player, move.Target),
            _ => (false, false, "Unknown move type.")
        };
    }

    private (bool, bool, string?) ApplyPlacement(Player player, Position target, bool heavy)
    {
        if (heavy && player.HeavyRemaining <= 0)
            return (false, false, $"{player.Name} has no heavy stones left.");

        if (!_board.IsEmpty(target))
            return (false, false, "That cell is already occupied.");

        var cell = new Cell(target.Row, target.Col);
        if (heavy)
        {
            cell.PlaceHeavy(player);
            player.UseHeavyStone();
        }
        else
        {
            cell.PlaceOrdinary(player);
        }

        _board.TryPlace(target, cell);
        return (true, true, null);
    }

    private (bool, bool, string?) ApplyEraser(Player player, Position target)
    {
        if (player.EraserRemaining <= 0)
            return (false, false, $"{player.Name} has no erasers left.");

        var cell = _board.GetCell(target);
        if (cell is null)
            return (false, false, "That cell is empty — nothing to erase.");
        if (cell.Owner == player)
            return (false, false, "You can't erase your own stone.");
        if (cell.StoneType != StoneType.Ordinary)
            return (false, false, "Only ordinary stones can be erased.");

        _board.TryRemove(target);
        player.UseEraser();
        return (true, false, null);
    }
}
