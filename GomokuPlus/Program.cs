using GomokuPlus.Engine;
using GomokuPlus.Models;
using GomokuPlus.Persistence;
using GomokuPlus.Utils;

if (args.Length > 0)
{
    RunAutomatedScript(args[0]);
}
else
{
    RunInteractive();
}

// Automated testing mode: one comma-separated execution string, e.g.
// "O3:2,O3:3,O3:1,O4:3,O3:4,O5:3,E3:3,H3:3". Always Human vs Human —
// script tokens carry no player-type information, only which cell/action,
// so both sides are plain HumanPlayer instances whose GetNextCommand is
// never called.
static void RunAutomatedScript(string script)
{
    var playerOne = new HumanPlayer("Player 1", 'X', '@');
    var playerTwo = new HumanPlayer("Player 2", 'O', '#');
    var engine = new GameEngine(Board.CreateStandard(), playerOne, playerTwo);
    engine.RunScripted(ParseScript(script));
}

static List<PlayerCommand> ParseScript(string script)
{
    var commands = new List<PlayerCommand>();
    foreach (var token in script.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
    {
        if (CommandParser.TryParse(token, out var command))
            commands.Add(command);
        else
            Console.WriteLine($"Skipping unparseable move: \"{token}\"");
    }
    return commands;
}

static void RunInteractive()
{
    while (true)
    {
        switch (ConsoleMenu.Select("GomokuPlus", "New Game", "Load Saved Game", "Help"))
        {
            case 0:
                StartNewGame();
                return;

            case 1:
                // A cancelled load (player typed QUIT at the filename
                // prompt) loops back to this menu instead of exiting.
                if (TryRunLoadedGame()) return;
                break;

            default:
                ShowHelpAndWait();
                break;
        }
    }
}

static void StartNewGame()
{
    var mode = PromptGameMode();

    Player playerOne;
    Player playerTwo;
    if (mode == GameMode.HumanVsComputer)
    {
        var human = new HumanPlayer("Player", 'X', '@');
        Player computer = new AIComputerPlayer("Computer", 'O', '#', PromptAiDifficulty());
        var humanFirst = ConsoleMenu.Select("Do you want to move first?", "Yes", "No") == 0;
        playerOne = humanFirst ? human : computer;
        playerTwo = humanFirst ? computer : human;
    }
    else
    {
        playerOne = new HumanPlayer("Player 1", 'X', '@');
        playerTwo = new HumanPlayer("Player 2", 'O', '#');
    }

    new GameEngine(Board.CreateStandard(), playerOne, playerTwo).Run();
}

// Prompts for a save-file name and, on success, resumes play from exactly
// where it left off (board, turn count, current player, remaining special
// stones). Loops on a bad path or corrupt file so one typo doesn't lose
// the player's place in the menu; typing QUIT backs out to the main menu.
// Returns false only in that cancelled case — every other path either
// exits the process or plays out a full game before returning true.
static bool TryRunLoadedGame()
{
    while (true)
    {
        var name = ConsoleInput.ReadLineOrExit("Enter save file to load (or QUIT to cancel): ").Trim();
        if (name.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
            return false;

        GameState state;
        try
        {
            state = GameStateRepository.Load(name);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Couldn't load \"{name}\": {ex.Message}");
            continue;
        }

        var (board, playerOne, playerTwo, current) = GameStateMapper.FromGameState(state);
        Console.WriteLine($"Resuming {state.Mode} game at turn {state.TurnCount + 1} — {current.Name} to move.");
        new GameEngine(board, playerOne, playerTwo).Run(current, state.TurnCount);
        return true;
    }
}

static void ShowHelpAndWait()
{
    if (!Console.IsInputRedirected)
        Console.Clear();

    Console.Write(HelpText.BuildGeneric());
    Console.WriteLine();
    ConsoleInput.ReadLineOrExit("Press Enter to return to the menu: ");
}

static GameMode PromptGameMode() =>
    ConsoleMenu.Select("Select game mode:", "Human vs Human", "Human vs Computer") == 0
        ? GameMode.HumanVsHuman
        : GameMode.HumanVsComputer;

static AiDifficulty PromptAiDifficulty() =>
    ConsoleMenu.Select("Select AI difficulty:",
        "Easy (random moves)",
        "Hard (strategic - blocks threats, looks ahead)") == 0
        ? AiDifficulty.Easy
        : AiDifficulty.Hard;
