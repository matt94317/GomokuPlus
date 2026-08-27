using GomokuPlus.Models;

namespace GomokuPlus.Persistence;

// Plain, JSON-serializable snapshot of everything needed to resume a game
// exactly where it left off. Deliberately separate from Board/Cell/Player —
// those stay free of serialization concerns, the same way they stay
// Console-free; GameStateMapper is the only thing that translates between
// the two.
public sealed record GameState(
    int Rows,
    int Columns,
    GameMode Mode,
    PlayerState PlayerOne,
    PlayerState PlayerTwo,
    int CurrentPlayerIndex, // 1 or 2 - whose turn it is next
    int TurnCount,
    List<CellState> Cells
);

// AiDifficulty is null for a HumanPlayer, set for an AIComputerPlayer -
// that alone is enough to reconstruct the right Player subtype on load.
public sealed record PlayerState(
    string Name,
    char OrdinarySymbol,
    char HeavySymbol,
    int HeavyRemaining,
    int EraserRemaining,
    AiDifficulty? AiDifficulty
);

public sealed record CellState(int Row, int Col, int OwnerIndex, StoneType StoneType);
