namespace GomokuPlus.Models.Moves;

// "E<row>:<col>" - consumes one eraser to remove an opponent's ordinary
// stone, leaving the cell empty. The eraser itself is never placed, so
// this is the one move that doesn't put a stone on the board.
public sealed class EraserStoneMove : IMove
{
    public Position Target { get; }
    public string Label => "Eraser";
    public bool PlacesStone => false;

    public EraserStoneMove(Position target) => Target = target;

    public string? Validate(Board board, Player player)
    {
        if (!board.IsInBounds(Target))
            return "Target is off the board.";

        var cell = board.GetCell(Target);
        if (cell is null)
            return "That cell is empty — nothing to erase.";
        if (cell.Owner == player)
            return "You can't erase your own stone.";
        if (cell.StoneType != StoneType.Ordinary)
            return "Only ordinary stones can be erased.";
        if (player.EraserRemaining <= 0)
            return $"{player.Name} has no erasers left.";

        return null;
    }

    public void Apply(Board board, Player player)
    {
        board.TryRemove(Target);
        player.TryUseEraser();
    }
}
