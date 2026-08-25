namespace GomokuPlus.Models;

public abstract class Player
{
    public string Name { get; }
    public char OrdinarySymbol { get; }
    public char HeavySymbol { get; }

    // Each player starts with 2 of each special stone, per the rules.
    // Private setters: the only way to spend one is UseHeavyStone/UseEraser,
    // so the count can never go out of sync with what was actually placed.
    public int HeavyRemaining { get; private set; } = 2;
    public int EraserRemaining { get; private set; } = 2;

    protected Player(string name, char ordinarySymbol, char heavySymbol)
    {
        Name = name;
        OrdinarySymbol = ordinarySymbol;
        HeavySymbol = heavySymbol;
    }

    // Callers (the game loop) are expected to check *Remaining > 0 before
    // calling these — they just record that a stone was spent, they don't
    // re-validate, since Player has no board/move context of its own.
    public void UseHeavyStone() => HeavyRemaining--;
    public void UseEraser() => EraserRemaining--;

    // Subclasses decide how a move is produced: HumanPlayer reads Console
    // input, ComputerPlayer picks one automatically. The game loop doesn't
    // need to know which.
    public abstract Move GetNextMove(Board board);
}
