namespace GomokuPlus.Models;

// Always 0-based internally. User-facing 1-based coordinates (e.g. typed as
// "3:4") are converted to this at the input boundary — see HumanPlayer.
// A record struct rather than a class: positions are small, immutable, and
// compared by value (two Positions with the same Row/Col are equal), which
// is what win-detection and lookups need.
public readonly record struct Position(int Row, int Col);
