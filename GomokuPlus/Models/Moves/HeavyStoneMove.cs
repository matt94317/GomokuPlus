namespace GomokuPlus.Models.Moves;

// "H<row>:<col>" - places a heavy stone on an empty cell. Counts toward a
// win exactly like an ordinary stone, but can never be erased, and each
// player only gets two per game.
public sealed class HeavyStoneMove : PlacementMove
{
    public HeavyStoneMove(Position target) : base(target) { }

    public override string Label => "Heavy";

    public override string? Validate(Board board, Player player)
    {
        var placementError = base.Validate(board, player);
        if (placementError is not null)
            return placementError;

        return player.HeavyRemaining > 0
            ? null
            : $"{player.Name} has no heavy stones left.";
    }

    protected override void Fill(Cell cell, Player player)
    {
        cell.PlaceHeavy(player);
        player.TryUseHeavyStone();
    }
}
