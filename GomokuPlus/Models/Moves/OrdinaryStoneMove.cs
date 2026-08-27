namespace GomokuPlus.Models.Moves;

// "O<row>:<col>" - places an ordinary stone on an empty cell. Unlimited:
// the only rules are the ones every placement shares.
public sealed class OrdinaryStoneMove : PlacementMove
{
    public OrdinaryStoneMove(Position target) : base(target) { }

    public override string Label => "Ordinary";

    protected override void Fill(Cell cell, Player player) => cell.PlaceOrdinary(player);
}
