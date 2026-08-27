using GomokuPlus.Models.Moves;

namespace GomokuPlus.Models;

public enum AiDifficulty { Easy, Hard }

// The computer opponent, with two selectable difficulties:
//   Easy - places an ordinary stone on a uniformly random empty cell.
//          Always legal, no strategy.
//   Hard - follows a heuristic decision order on every turn:
//          1. Take an immediate win if one is available.
//          2. Block the opponent's immediate win if they have one.
//          3. Otherwise score every reasonable candidate cell by how
//             strong a line it builds for this player and how strong a
//             line it would deny the opponent (open twos/threes/fours
//             etc.), then look one ply ahead among the best-scoring
//             candidates to prefer whichever leaves the opponent's best
//             reply weakest.
// Either difficulty only ever places Ordinary stones — it never touches
// Heavy/Eraser inventory, so it can't violate a limit it doesn't use, and
// every candidate cell is drawn from the board's actual empty cells, so
// every move it proposes is legal by construction.
public class AIComputerPlayer : Player
{
    private const int NeighborhoodRadius = 2;
    private const int LookaheadCandidateCount = 6;
    private const int LookaheadScoreTolerance = 50;
    private const double DefenseWeight = 0.9;

    private static readonly (int DRow, int DCol)[] Axes =
    {
        (0, 1),  // horizontal
        (1, 0),  // vertical
        (1, 1),  // diagonal ↘
        (1, -1), // diagonal ↙
    };

    private readonly Random _random = new();

    public AiDifficulty Difficulty { get; }

    public AIComputerPlayer(string name, char ordinarySymbol, char heavySymbol, AiDifficulty difficulty = AiDifficulty.Hard)
        : base(name, ordinarySymbol, heavySymbol)
    {
        Difficulty = difficulty;
    }

    public override PlayerCommand GetNextCommand(Board board)
    {
        var target = Difficulty == AiDifficulty.Easy
            ? PickRandomCell(board)
            : PickStrategicCell(board);

        return new MoveCommand(new OrdinaryStoneMove(target));
    }

    private Position PickRandomCell(Board board)
    {
        var emptyCells = AllEmptyCells(board);
        return emptyCells[_random.Next(emptyCells.Count)];
    }

    private Position PickStrategicCell(Board board)
    {
        if (IsEmptyBoard(board))
            return new Position(board.Rows / 2, board.Columns / 2);

        var candidates = CandidateCells(board);
        if (candidates.Count == 0)
            candidates = AllEmptyCells(board);

        return ChooseBestCell(board, candidates);
    }

    private Position ChooseBestCell(Board board, List<Position> candidates)
    {
        var opponent = InferOpponent(board);

        foreach (var pos in candidates)
            if (board.MaxRunThrough(pos, this) >= 5)
                return pos;

        if (opponent is not null)
            foreach (var pos in candidates)
                if (board.MaxRunThrough(pos, opponent) >= 5)
                    return pos;

        var scored = candidates
            .Select(pos => (Pos: pos, Score: ScoreCandidate(board, pos, opponent)))
            .OrderByDescending(s => s.Score)
            .ToList();

        var topScore = scored[0].Score;
        if (opponent is null)
            return TieBreak(scored, topScore);

        var topCandidates = scored
            .Take(LookaheadCandidateCount)
            .Where(s => s.Score >= topScore - LookaheadScoreTolerance)
            .ToList();

        if (topCandidates.Count == 1)
            return topCandidates[0].Pos;

        var bestOpponentReply = int.MaxValue;
        var finalists = new List<Position>();
        foreach (var (pos, _) in topCandidates)
        {
            var replyScore = BestOpponentReplyScore(board, pos, opponent);
            if (replyScore < bestOpponentReply)
            {
                bestOpponentReply = replyScore;
                finalists.Clear();
                finalists.Add(pos);
            }
            else if (replyScore == bestOpponentReply)
            {
                finalists.Add(pos);
            }
        }

        return finalists[_random.Next(finalists.Count)];
    }

    private int ScoreCandidate(Board board, Position pos, Player? opponent)
    {
        var offense = PatternScore(board, pos, this);
        var defense = opponent is null ? 0 : PatternScore(board, pos, opponent);
        return offense + (int)(defense * DefenseWeight);
    }

    // Simulates placing this player's stone at `aiMove`, then reports the
    // strongest reply available to `opponent` afterward - the one-ply
    // lookahead. Placement is undone before returning either way.
    private int BestOpponentReplyScore(Board board, Position aiMove, Player opponent)
    {
        var cell = new Cell(aiMove.Row, aiMove.Col);
        cell.PlaceOrdinary(this);
        board.TryPlace(aiMove, cell);
        try
        {
            var replyCandidates = CandidateCells(board);
            var best = 0;
            foreach (var pos in replyCandidates)
            {
                if (board.MaxRunThrough(pos, opponent) >= 5)
                    return 100_000;
                var score = PatternScore(board, pos, opponent);
                if (score > best) best = score;
            }
            return best;
        }
        finally
        {
            board.TryRemove(aiMove);
        }
    }

    private static Position TieBreak(List<(Position Pos, int Score)> scored, int bestScore)
    {
        var best = scored.Where(s => s.Score == bestScore).ToList();
        return best[Random.Shared.Next(best.Count)].Pos;
    }

    // Sums, over all four axes, how strong a line `owner` would have
    // through `pos` if they played there - longer runs score higher, and
    // an unblocked ("open") end scores higher than a blocked one, since an
    // open line can still grow into a win next turn.
    private static int PatternScore(Board board, Position pos, Player owner)
    {
        var total = 0;
        foreach (var (dRow, dCol) in Axes)
        {
            var (run, openEnds) = EvaluateAxis(board, pos, dRow, dCol, owner);
            total += ScoreForRun(run, openEnds);
        }
        return total;
    }

    private static int ScoreForRun(int run, int openEnds) => (run, openEnds) switch
    {
        ( >= 5, _) => 100_000,
        (4, 2) => 10_000,
        (4, 1) => 1_000,
        (3, 2) => 500,
        (3, 1) => 50,
        (2, 2) => 30,
        (2, 1) => 5,
        (1, 2) => 3,
        (1, 1) => 1,
        _ => 0
    };

    private static (int Run, int OpenEnds) EvaluateAxis(Board board, Position pos, int dRow, int dCol, Player owner)
    {
        var forward = WalkOwned(board, pos, dRow, dCol, owner);
        var backward = WalkOwned(board, pos, -dRow, -dCol, owner);
        return (1 + forward.Count + backward.Count, (forward.OpenEnd ? 1 : 0) + (backward.OpenEnd ? 1 : 0));
    }

    private static (int Count, bool OpenEnd) WalkOwned(Board board, Position from, int dRow, int dCol, Player owner)
    {
        var count = 0;
        var p = new Position(from.Row + dRow, from.Col + dCol);
        while (board.IsInBounds(p) && board.GetCell(p)?.Owner == owner)
        {
            count++;
            p = new Position(p.Row + dRow, p.Col + dCol);
        }
        return (count, board.IsInBounds(p) && board.IsEmpty(p));
    }

    // Empty cells within NeighborhoodRadius of any occupied cell - keeps
    // evaluation bounded to where the action actually is instead of
    // scoring the whole board every turn.
    private static List<Position> CandidateCells(Board board)
    {
        var seen = new HashSet<Position>();
        var result = new List<Position>();
        for (var row = 0; row < board.Rows; row++)
        {
            for (var col = 0; col < board.Columns; col++)
            {
                var occupied = new Position(row, col);
                if (board.IsEmpty(occupied)) continue;

                for (var dRow = -NeighborhoodRadius; dRow <= NeighborhoodRadius; dRow++)
                for (var dCol = -NeighborhoodRadius; dCol <= NeighborhoodRadius; dCol++)
                {
                    var neighbor = new Position(row + dRow, col + dCol);
                    if (board.IsInBounds(neighbor) && board.IsEmpty(neighbor) && seen.Add(neighbor))
                        result.Add(neighbor);
                }
            }
        }
        return result;
    }

    private static List<Position> AllEmptyCells(Board board)
    {
        var result = new List<Position>();
        for (var row = 0; row < board.Rows; row++)
            for (var col = 0; col < board.Columns; col++)
            {
                var pos = new Position(row, col);
                if (board.IsEmpty(pos)) result.Add(pos);
            }
        return result;
    }

    private static bool IsEmptyBoard(Board board)
    {
        for (var row = 0; row < board.Rows; row++)
            for (var col = 0; col < board.Columns; col++)
                if (!board.IsEmpty(new Position(row, col)))
                    return false;
        return true;
    }

    // Assumes the standard two-player setup this whole codebase assumes:
    // whichever other Player owns any cell on the board is "the opponent".
    private Player? InferOpponent(Board board)
    {
        for (var row = 0; row < board.Rows; row++)
            for (var col = 0; col < board.Columns; col++)
            {
                var owner = board.GetCell(new Position(row, col))?.Owner;
                if (owner is not null && owner != this)
                    return owner;
            }
        return null;
    }
}
