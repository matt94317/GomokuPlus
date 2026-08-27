using GomokuPlus.Utils;

namespace GomokuPlus.Models;

public class HumanPlayer : Player
{
    public HumanPlayer(string name, char ordinarySymbol, char heavySymbol)
        : base(name, ordinarySymbol, heavySymbol)
    {
    }

    // Loops on the console until it gets something syntactically valid.
    // It does NOT check board state (occupied cell, out of bounds, stones
    // remaining) — that validation belongs to the move itself, so the same
    // rules apply whether the move came from a human, the computer player,
    // or a scripted test string.
    public override PlayerCommand GetNextCommand(Board board)
    {
        while (true)
        {
            var raw = ConsoleInput.ReadLineOrExit(
                $"{Name}, enter your move (e.g. O3:4, H5:5, E2:2, SAVE:filename, QUIT, HELP): ");

            if (CommandParser.TryParse(raw, out var command))
                return command;

            Console.WriteLine("Invalid format. Use <O|H|E><row>:<col> (e.g. O3:4), SAVE:<filename>, QUIT, or HELP.");
        }
    }
}
