using GomokuPlus.Engine;
using GomokuPlus.Models;

var board = new Board(10, 10);
var playerOne = new HumanPlayer("Player 1", 'X', '@');
var playerTwo = new HumanPlayer("Player 2", 'O', '#');
var engine = new GameEngine(board, playerOne, playerTwo);

if (args.Length > 0)
{
    engine.RunScripted(ParseScript(args[0]));
}
else
{
    engine.Run();
}

// Automated testing mode: a single comma-separated string of move tokens,
// e.g. "O3:2,O3:3,O3:1,O4:3,O3:4,O5:3,E3:3,H3:3". Player identity in this
// mode still comes from HumanPlayer instances above, but GetNextMove is
// never called on them — RunScripted pulls moves from this parsed list
// instead of prompting, so no console interaction happens.
static List<Move> ParseScript(string script)
{
    var moves = new List<Move>();
    foreach (var token in script.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
    {
        if (Move.TryParse(token, out var move))
            moves.Add(move);
        else
            Console.WriteLine($"Skipping unparseable move: \"{token}\"");
    }
    return moves;
}
