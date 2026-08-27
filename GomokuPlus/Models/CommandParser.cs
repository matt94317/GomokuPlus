using GomokuPlus.Models.Moves;

namespace GomokuPlus.Models;

// Turns one raw input token into a typed PlayerCommand. This is purely a
// *syntax* step — it decides whether the text is a command it recognises,
// never whether that command is legal against the current board. Legality
// is IMove.Validate's job, which keeps "I don't understand that" and
// "you're not allowed to do that" as two separately reportable failures.
//
// Shared by every input source: interactive console entry and the
// automated-mode script string both funnel through here, so a given token
// means the same thing regardless of where it came from.
public static class CommandParser
{
    private const string SavePrefix = "SAVE:";

    public static bool TryParse(string? raw, out PlayerCommand command)
    {
        command = default!;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        var trimmed = raw.Trim();

        if (trimmed.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
        {
            command = new QuitCommand();
            return true;
        }

        if (trimmed.Equals("HELP", StringComparison.OrdinalIgnoreCase))
        {
            command = new HelpCommand();
            return true;
        }

        if (trimmed.StartsWith(SavePrefix, StringComparison.OrdinalIgnoreCase))
        {
            var fileName = trimmed[SavePrefix.Length..].Trim();
            if (fileName.Length == 0) return false;
            command = new SaveCommand(fileName);
            return true;
        }

        if (!TryParseMove(trimmed, out var move))
            return false;

        command = new MoveCommand(move);
        return true;
    }

    private static bool TryParseMove(string token, out IMove move)
    {
        move = default!;
        if (token.Length < 2) return false;

        var parts = token[1..].Split(':');
        if (parts.Length != 2) return false;
        if (!int.TryParse(parts[0], out var row)) return false;
        if (!int.TryParse(parts[1], out var col)) return false;

        // Input is 1-based ("row 1" through "row 10"); everything internal
        // to the domain layer is 0-based. Converting here means the rest
        // of the codebase never has to think about it.
        var target = new Position(row - 1, col - 1);

        move = char.ToUpperInvariant(token[0]) switch
        {
            'O' => new OrdinaryStoneMove(target),
            'H' => new HeavyStoneMove(target),
            'E' => new EraserStoneMove(target),
            _ => null!
        };

        return move is not null;
    }
}
