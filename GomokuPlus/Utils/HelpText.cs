using System.Text;
using GomokuPlus.Models;

namespace GomokuPlus.Utils;

// Builds the help-menu text shown to human players. Kept separate from
// both Program.cs (which has no Player instances before a game starts)
// and GameEngine (which has no reason to own static reference text) so
// the same wording is shared by the pre-game "Help" menu option and the
// in-game "HELP" command.
public static class HelpText
{
    private const string MoveSyntax = """
        Move syntax (row/col are 1-based):
          O<row>:<col>   Place an Ordinary stone                e.g. O3:4
          H<row>:<col>   Place a Heavy stone                    e.g. H5:5
          E<row>:<col>   Erase an opponent's Ordinary stone     e.g. E3:3

        Meta-commands (none of these use up your turn):
          SAVE:<filename>   Save the current game to a file
          QUIT              Exit without saving
          HELP              Show this help
        """;

    private const string WinCondition =
        "Win condition: 5 of your own stones in an unbroken line - horizontal, vertical, or diagonal - wins instantly.";

    // Shown from the main menu, before any game (and so any Player) exists.
    public static string BuildGeneric()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== GomokuPlus Help ===");
        sb.AppendLine();
        sb.AppendLine(MoveSyntax);
        sb.AppendLine();
        sb.AppendLine("Special stones (per player, per game):");
        sb.AppendLine("  Heavy stones:  2 available. Once placed, cannot be erased.");
        sb.AppendLine("  Eraser stones: 2 available. Can only target an opponent's");
        sb.AppendLine("                 Ordinary stone - not empty cells, not Heavy");
        sb.AppendLine("                 stones, not your own stones.");
        sb.AppendLine();
        sb.AppendLine(WinCondition);
        return sb.ToString();
    }

    // Shown from the "HELP" in-game command - includes both players' live
    // remaining special-stone counts, not just the static allotment rule.
    public static string BuildForGame(Player current, Player opponent)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== GomokuPlus Help ===");
        sb.AppendLine();
        sb.AppendLine(MoveSyntax);
        sb.AppendLine();
        sb.AppendLine("Special stones remaining:");
        sb.AppendLine($"  {current.Name} (you): Heavy {current.HeavyRemaining}/2, Eraser {current.EraserRemaining}/2");
        sb.AppendLine($"  {opponent.Name}: Heavy {opponent.HeavyRemaining}/2, Eraser {opponent.EraserRemaining}/2");
        sb.AppendLine();
        sb.AppendLine(WinCondition);
        return sb.ToString();
    }
}
