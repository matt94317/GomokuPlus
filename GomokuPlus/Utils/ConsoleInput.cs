namespace GomokuPlus.Utils;

// Console reads that handle end-of-input consistently.
public static class ConsoleInput
{
    // Console.ReadLine returns null when stdin is closed (Ctrl+D, or piped
    // input ran out) rather than blocking. Every prompt in the game treats
    // that the same way — say so and stop — so without this the callers
    // would each re-implement it, and any that forgot would spin forever
    // reading null as "invalid input, ask again".
    public static string ReadLineOrExit(string prompt)
    {
        Console.Write(prompt);
        var raw = Console.ReadLine();
        if (raw is not null)
            return raw;

        Console.WriteLine();
        Console.WriteLine("No more input — exiting.");
        Environment.Exit(0);
        return string.Empty; // unreachable — Exit does not return.
    }
}
