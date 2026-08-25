namespace GomokuPlus.Models;

public enum StoneType
{
    Empty,
    Ordinary,
    Heavy
}

public class Cell
{
    public int Row { get; }
    public int Col { get; }
    public Player? Owner { get; private set; }
    public StoneType StoneType { get; private set; }

    public Cell(int row, int col)
    {
        Row = row;
        Col = col;
        Owner = null;
        StoneType = StoneType.Empty;
    }

    public bool IsEmpty => StoneType == StoneType.Empty;

    public char Symbol
    {
        get
        {
            if (StoneType == StoneType.Empty || Owner is null)
                return '.';

            return StoneType == StoneType.Heavy
                ? Owner.HeavySymbol
                : Owner.OrdinarySymbol;
        }
    }

    // Only Board should call these — Cell holds state, Board decides legality.
    public void PlaceOrdinary(Player owner)
    {
        Owner = owner;
        StoneType = StoneType.Ordinary;
    }

    public void PlaceHeavy(Player owner)
    {
        Owner = owner;
        StoneType = StoneType.Heavy;
    }

    public void Clear()
    {
        Owner = null;
        StoneType = StoneType.Empty;
    }
}
