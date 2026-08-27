namespace GomokuPlus.Models;

public abstract class Player
{
    public string Name { get; }
    public char OrdinarySymbol { get; }
    public char HeavySymbol { get; }

    // Each player starts with 2 of each special stone, per the rules.
    // Private setters: the only way to spend one is TryUseHeavyStone/
    // TryUseEraser, so the count can never go out of sync with what was
    // actually placed, and can never go negative.
    public int HeavyRemaining { get; private set; } = 2;
    public int EraserRemaining { get; private set; } = 2;

    protected Player(string name, char ordinarySymbol, char heavySymbol)
    {
        Name = name;
        OrdinarySymbol = ordinarySymbol;
        HeavySymbol = heavySymbol;
    }

    // Spends one stone and returns true, or leaves the count untouched and
    // returns false if none remain - the class enforces its own invariant
    // rather than trusting callers to check *Remaining first.
    public bool TryUseHeavyStone()
    {
        if (HeavyRemaining <= 0) return false;
        HeavyRemaining--;
        return true;
    }

    public bool TryUseEraser()
    {
        if (EraserRemaining <= 0) return false;
        EraserRemaining--;
        return true;
    }

    // Subclasses decide how a command is produced: HumanPlayer reads
    // Console input, AIComputerPlayer picks a move automatically. The game
    // loop doesn't need to know which.
    public abstract PlayerCommand GetNextCommand(Board board);
}
