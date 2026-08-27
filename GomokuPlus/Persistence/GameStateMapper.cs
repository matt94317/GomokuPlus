using GomokuPlus.Models;

namespace GomokuPlus.Persistence;

// Translates between the live domain objects (Board/Player/Cell) and the
// plain GameState DTO that actually gets written to disk.
public static class GameStateMapper
{
    public static GameState ToGameState(Board board, Player playerOne, Player playerTwo, Player current, int turnCount)
    {
        var mode = playerOne is AIComputerPlayer || playerTwo is AIComputerPlayer
            ? GameMode.HumanVsComputer
            : GameMode.HumanVsHuman;

        var cells = new List<CellState>();
        for (var row = 0; row < board.Rows; row++)
        {
            for (var col = 0; col < board.Columns; col++)
            {
                var cell = board.GetCell(new Position(row, col));
                if (cell is null) continue;
                var ownerIndex = cell.Owner == playerOne ? 1 : 2;
                cells.Add(new CellState(row, col, ownerIndex, cell.StoneType));
            }
        }

        return new GameState(
            board.Rows,
            board.Columns,
            mode,
            ToPlayerState(playerOne),
            ToPlayerState(playerTwo),
            current == playerOne ? 1 : 2,
            turnCount,
            cells);
    }

    // Returns the reconstructed board plus whichever PlayerOne/PlayerTwo/
    // Current instance the caller needs to feed back into a GameEngine.
    public static (Board Board, Player PlayerOne, Player PlayerTwo, Player Current) FromGameState(GameState state)
    {
        var playerOne = FromPlayerState(state.PlayerOne);
        var playerTwo = FromPlayerState(state.PlayerTwo);

        var board = new Board(state.Rows, state.Columns);
        foreach (var cellState in state.Cells)
        {
            var owner = cellState.OwnerIndex == 1 ? playerOne : playerTwo;
            var cell = new Cell(cellState.Row, cellState.Col);
            if (cellState.StoneType == StoneType.Heavy)
                cell.PlaceHeavy(owner);
            else
                cell.PlaceOrdinary(owner);
            board.TryPlace(new Position(cellState.Row, cellState.Col), cell);
        }

        var current = state.CurrentPlayerIndex == 1 ? playerOne : playerTwo;
        return (board, playerOne, playerTwo, current);
    }

    private static PlayerState ToPlayerState(Player player) => new(
        player.Name,
        player.OrdinarySymbol,
        player.HeavySymbol,
        player.HeavyRemaining,
        player.EraserRemaining,
        player is AIComputerPlayer ai ? ai.Difficulty : null);

    private static Player FromPlayerState(PlayerState state)
    {
        Player player = state.AiDifficulty is { } difficulty
            ? new AIComputerPlayer(state.Name, state.OrdinarySymbol, state.HeavySymbol, difficulty)
            : new HumanPlayer(state.Name, state.OrdinarySymbol, state.HeavySymbol);

        // HeavyRemaining/EraserRemaining have no public setter - only
        // TryUseHeavyStone/TryUseEraser can move them, so a freshly-
        // constructed player (starting at 2/2) is wound down to the saved
        // count the same way normal play would have spent them.
        for (var i = player.HeavyRemaining - state.HeavyRemaining; i > 0; i--)
            player.TryUseHeavyStone();
        for (var i = player.EraserRemaining - state.EraserRemaining; i > 0; i--)
            player.TryUseEraser();

        return player;
    }
}
