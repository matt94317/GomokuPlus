namespace GomokuPlus.Models.Moves;

// One board action a player can take. Each move type validates and applies
// itself, so adding a new stone type means adding a class here rather than
// editing a central switch in GameEngine.
//
// Validate/Apply are deliberately split: the turn loop needs to know a move
// is legal *before* anything mutates, so an illegal move can be reported
// and retried without the player losing their turn or part of their
// inventory.
public interface IMove
{
    Position Target { get; }

    // Human-readable move type ("Ordinary"/"Heavy"/"Eraser"), used when
    // reporting an illegal move in automated mode.
    string Label { get; }

    // True if this move puts a stone on the board. Only a placement can
    // create a new win, so the turn loop skips the win check otherwise.
    bool PlacesStone { get; }

    // Null if the move is legal right now, otherwise the reason it isn't.
    // Must not mutate anything.
    string? Validate(Board board, Player player);

    // Commits the move. Assumes Validate returned null for this same
    // board/player - it does not re-check.
    void Apply(Board board, Player player);
}
