namespace GomokuPlus.Models.Moves;

// Shared behaviour for the two moves that put a stone on an empty cell.
// Subclasses add whatever extra rule applies to their stone type (e.g. a
// heavy stone also has to be in stock) and decide how the cell is filled.
public abstract class PlacementMove : IMove
{
    public Position Target { get; }
    public abstract string Label { get; }
    public bool PlacesStone => true;

    protected PlacementMove(Position target) => Target = target;

    public virtual string? Validate(Board board, Player player)
    {
        if (!board.IsInBounds(Target))
            return "Target is off the board.";
        if (!board.IsEmpty(Target))
            return "That cell is already occupied.";
        return null;
    }

    public void Apply(Board board, Player player)
    {
        var cell = new Cell(Target.Row, Target.Col);
        Fill(cell, player);
        board.TryPlace(Target, cell);
    }

    // Stamps the new cell with this move's stone type, and spends any
    // inventory that stone type costs.
    protected abstract void Fill(Cell cell, Player player);
}
