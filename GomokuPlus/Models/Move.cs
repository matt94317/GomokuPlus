namespace GomokuPlus.Models;

public enum MoveKind
{
    Ordinary,
    Heavy,
    Eraser
}

// A parsed action: what kind of move, and which cell it targets.
// This is a plain data carrier — it doesn't know whether the move is
// actually legal. Legality (bounds, occupancy, stones remaining) is decided
// later by whatever applies the move to the Board, keeping "can this be
// parsed" separate from "is this allowed right now".
public sealed record Move(MoveKind Kind, Position Target)
{
    // Parses one move token, e.g. "O3:4", "H5:5", "E2:2". Shared by every
    // move source — interactive console input and the automated-mode
    // script string both funnel through this, so "O3:4" means the same
    // thing regardless of where it came from.
    public static bool TryParse(string? raw, out Move move)
    {
        move = default!;
        if (string.IsNullOrWhiteSpace(raw) || raw.Length < 2)
            return false;

        MoveKind? kind = char.ToUpperInvariant(raw[0]) switch
        {
            'O' => MoveKind.Ordinary,
            'H' => MoveKind.Heavy,
            'E' => MoveKind.Eraser,
            _ => null
        };
        if (kind is null) return false;

        var parts = raw[1..].Split(':');
        if (parts.Length != 2) return false;
        if (!int.TryParse(parts[0], out var row)) return false;
        if (!int.TryParse(parts[1], out var col)) return false;

        // Input is 1-based ("row 1" through "row 10"); everything internal
        // to the domain layer is 0-based. Converting here means the rest
        // of the codebase never has to think about it.
        move = new Move(kind.Value, new Position(row - 1, col - 1));
        return true;
    }
}
