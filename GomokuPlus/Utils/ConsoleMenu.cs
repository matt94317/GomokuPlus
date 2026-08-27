namespace GomokuPlus.Utils;

// A reusable arrow-key menu. Knows nothing about Gomoku — it takes a title
// and some option labels and returns the index the player chose.
public static class ConsoleMenu
{
    // Up/Down (or W/S) moves the highlighted option, Enter confirms.
    //
    // When input is redirected — piped stdin, or an automated harness —
    // Console.ReadKey can't reliably interpret arrow-key escape sequences
    // from a non-terminal stream, so this falls back to a typed-number
    // prompt instead. That keeps every interactive prompt scriptable via
    // stdin even though the live UX is arrow-driven.
    public static int Select(string title, params string[] options)
    {
        if (options.Length == 0)
            throw new ArgumentException("A menu needs at least one option.", nameof(options));

        if (Console.IsInputRedirected)
            return SelectByNumber(title, options);

        var selected = 0;
        Draw(title, options, selected);

        while (true)
        {
            var key = Console.ReadKey(intercept: true).Key;
            if (key == ConsoleKey.Enter)
                return selected;

            var next = key switch
            {
                ConsoleKey.UpArrow or ConsoleKey.W => (selected - 1 + options.Length) % options.Length,
                ConsoleKey.DownArrow or ConsoleKey.S => (selected + 1) % options.Length,
                _ => selected
            };

            if (next == selected) continue;
            selected = next;
            Draw(title, options, selected);
        }
    }

    // Redraws by clearing rather than tracking a cursor row: on Unix,
    // reading Console.CursorTop/SetCursorPosition makes .NET query the
    // terminal for its cursor position over stdin and block on the reply,
    // which some pty/recording setups never send. Clear() only writes an
    // escape sequence — it doesn't wait on anything — so this can't hang.
    private static void Draw(string title, string[] options, int selected)
    {
        Console.Clear();
        Console.WriteLine(title);
        for (var i = 0; i < options.Length; i++)
            Console.WriteLine(i == selected ? $"> {options[i]}" : $"  {options[i]}");
        Console.WriteLine();
        Console.WriteLine("(Use ↑/↓ to choose, Enter to confirm)");
    }

    private static int SelectByNumber(string title, string[] options)
    {
        while (true)
        {
            Console.WriteLine(title);
            for (var i = 0; i < options.Length; i++)
                Console.WriteLine($"  {i + 1}. {options[i]}");

            var raw = ConsoleInput.ReadLineOrExit($"Enter choice (1-{options.Length}): ");
            if (int.TryParse(raw.Trim(), out var choice) && choice >= 1 && choice <= options.Length)
                return choice - 1;

            Console.WriteLine($"Invalid choice. Enter a number from 1 to {options.Length}.");
        }
    }
}
