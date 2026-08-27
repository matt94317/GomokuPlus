using GomokuPlus.Models.Moves;

namespace GomokuPlus.Models;

// What one line of player input resolved to. Either a board move, or one
// of the meta-commands that acts on the session rather than the board.
// Keeping these as distinct types (rather than one enum-tagged record)
// means the turn loop can't accidentally treat a SAVE as a placement, and
// each carries exactly the data it needs — nothing has a meaningless
// Target or an unused FileName.
public abstract record PlayerCommand;

public sealed record MoveCommand(IMove Move) : PlayerCommand;

public sealed record SaveCommand(string FileName) : PlayerCommand;

public sealed record QuitCommand : PlayerCommand;

public sealed record HelpCommand : PlayerCommand;
