namespace GomokuPlus.Models;

public class HumanPlayer : Player
{
    public HumanPlayer(string name, char ordinarySymbol, char heavySymbol)
        : base(name, ordinarySymbol, heavySymbol)
    {
    }

    // Loops on the console until it gets something syntactically valid.
    // It does NOT check board state (occupied cell, out of bounds, stones
    // remaining) — that validation belongs to whatever applies the move,
    // so the same rules apply whether the move came from a human, the
    // computer player, or a scripted test file.
    public override Move GetNextMove(Board board)
    {
        while (true)
        {
            Console.Write($"{Name}, enter your move (e.g. O3:4, H5:5, E2:2): ");
            var raw = Console.ReadLine();

            // Console.ReadLine returns null when stdin is closed (Ctrl+D,
            // or piped input ran out) rather than blocking — without this
            // check that reads as "invalid format" forever and spins in a
            // tight loop instead of stopping.
            if (raw is null)
            {
                Console.WriteLine();
                Console.WriteLine("No more input — exiting.");
                Environment.Exit(0);
            }

            if (Move.TryParse(raw, out var move))
                return move;

            Console.WriteLine("Invalid format. Use <O|H|E><row>:<col>, e.g. O3:4.");
        }
    }
}
