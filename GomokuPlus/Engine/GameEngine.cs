using GomokuPlus.Models;
using GomokuPlus.Models.Moves;
using GomokuPlus.Persistence;
using GomokuPlus.Utils;

namespace GomokuPlus.Engine;

// Orchestrates a two-player game: alternates turns, asks the current player
// for a command, lets the move validate/apply itself, checks for a win/draw
// after every placement, and prints the board. This is the only class that
// prints board state — Board/Cell/Player stay Console-free so the rules
// they implement can be exercised without a terminal attached.
//
// Note what this class no longer knows: how any individual stone type
// works. Adding a move type means adding an IMove implementation and a
// line in CommandParser, not editing anything here. The switch below is
// over session control flow (play/save/quit/help), which is genuinely
// this class's business and doesn't grow with new stone types.
public class GameEngine
{
    private const int WinLength = 5;

    private readonly Board _board;
    private readonly Player _playerOne;
    private readonly Player _playerTwo;
    private int _turnCount;

    public GameEngine(Board board, Player playerOne, Player playerTwo)
    {
        _board = board;
        _playerOne = playerOne;
        _playerTwo = playerTwo;
    }

    public void Run() => Run(_playerOne, turnCount: 0);

    // Resumes an interactive game from a previously loaded save: `_board`
    // (passed to the constructor) is expected to already hold the saved
    // stones, and this restores whose turn it is and how many turns have
    // been played, so a fresh save mid-resumed-game reports the right
    // turn count instead of restarting from zero.
    public void Run(Player startingPlayer, int turnCount)
    {
        var current = startingPlayer;
        _turnCount = turnCount;

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine(_board.Render());

            switch (current.GetNextCommand(_board))
            {
                case QuitCommand:
                    Console.WriteLine("Exiting without saving.");
                    return;

                // Meta-commands don't consume a turn — the same player is
                // asked again on the next pass.
                case SaveCommand save:
                    SaveGame(save.FileName, current);
                    continue;

                case HelpCommand:
                    Console.WriteLine();
                    Console.Write(HelpText.BuildForGame(current, Opponent(current)));
                    continue;

                case MoveCommand(var move):
                    var error = move.Validate(_board, current);
                    if (error is not null)
                    {
                        // Same player retries — the turn only advances on success.
                        Console.WriteLine($"Illegal move: {error}");
                        continue;
                    }

                    move.Apply(_board, current);
                    _turnCount++;

                    if (move.PlacesStone && _board.CheckWin(move.Target, WinLength))
                    {
                        Console.WriteLine();
                        Console.WriteLine(_board.Render());
                        Console.WriteLine($"{current.Name} wins!");
                        return;
                    }

                    if (_board.IsFull())
                    {
                        Console.WriteLine();
                        Console.WriteLine(_board.Render());
                        Console.WriteLine("Board is full — it's a draw.");
                        return;
                    }

                    current = Opponent(current);
                    break;
            }
        }
    }

    // Automated testing mode: applies a pre-parsed sequence of commands
    // with no prompts and no per-turn printing, then renders the board once
    // at the end. Reuses the exact same Validate/Apply/CheckWin/IsFull rules
    // as Run(), so a scripted game and an interactive one are judged
    // identically — only the command source and the print cadence differ.
    public void RunScripted(IReadOnlyList<PlayerCommand> commands)
    {
        var current = _playerOne;

        foreach (var command in commands)
        {
            if (command is not MoveCommand(var move))
            {
                // A script is a list of board moves; SAVE/QUIT/HELP have no
                // meaning without a session driving them.
                Console.WriteLine("Skipping meta-command: automated mode runs board moves only.");
                continue;
            }

            var error = move.Validate(_board, current);
            if (error is not null)
            {
                // Doesn't advance the turn, same as an illegal interactive move.
                Console.WriteLine($"Illegal move {Describe(move)}: {error}");
                continue;
            }

            move.Apply(_board, current);
            _turnCount++;

            if (move.PlacesStone && _board.CheckWin(move.Target, WinLength))
            {
                Console.WriteLine(_board.Render());
                Console.WriteLine($"{current.Name} wins!");
                return;
            }

            if (_board.IsFull())
            {
                Console.WriteLine(_board.Render());
                Console.WriteLine("Board is full — it's a draw.");
                return;
            }

            current = Opponent(current);
        }

        Console.WriteLine(_board.Render());
    }

    private Player Opponent(Player player) => player == _playerOne ? _playerTwo : _playerOne;

    private static string Describe(IMove move) =>
        $"{move.Label} {move.Target.Row + 1}:{move.Target.Col + 1}";

    // Failure here (disk full, permission denied, bad path) shouldn't cost
    // the player their in-progress game — report it and let them keep
    // playing or retry the save, same as an illegal-move retry.
    private void SaveGame(string fileName, Player currentPlayer)
    {
        try
        {
            var state = GameStateMapper.ToGameState(_board, _playerOne, _playerTwo, currentPlayer, _turnCount);
            var savedPath = GameStateRepository.Save(fileName, state);
            Console.WriteLine($"Game saved to \"{savedPath}\".");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save game: {ex.Message}");
        }
    }
}
