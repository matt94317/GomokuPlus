# OOP Design Concepts for Grid-Based Turn Games in C#

This is a general reference on object-oriented design patterns commonly used when building turn-based, grid-based console games in C#. It is intentionally generic — it does not map to any specific assignment's requirements, class names, or point values. Use it to build your own understanding and design decisions.

---

## A General Design Workflow

A reasonable order of operations when starting a project like this from scratch:

1. **Model the nouns first.** What are the domain entities? (e.g., a grid, a position, a cell/piece, a player, a game state.) Write these as small, focused classes before writing any game loop.
2. **Model the verbs next.** What actions can happen to those nouns? Each verb is a candidate for a `Validate` + `Apply` pair, per the command pattern above.
3. **Write the domain layer's rules with no I/O at all**, and prove it works with simple console-printed assertions or a test project — before touching `Console.ReadLine`.
4. **Add the presentation layer** (rendering, prompts) as a thin wrapper around the already-working domain layer.
5. **Add persistence** once the in-memory domain model is stable — persistence code should map _to and from_ that model, not define it.
6. **Add alternate input sources last** (e.g., a script/automated mode) — if step 3–4 are properly separated, an alternate input source is just another producer of the same `IMove` objects the interactive mode produces, feeding the same domain layer.

Working in this order tends to prevent the most common structural problem in these projects: rules logic and console I/O becoming so intertwined that neither can be tested, extended, or reused independently.

---

## 1. Encapsulation of State

The core idea: a class that owns data should be the only thing that can mutate it.
Callers interact through behavior (methods), not by reaching in and changing fields directly.

**Why it matters:** if any part of the program can freely poke at a grid's internal array, you lose the ability to guarantee invariants (e.g., "a cell is either empty or holds exactly one piece"). Bugs like double-placement or desync between "what's on the board" and "whose turn it is" usually trace back to state that wasn't encapsulated.

```csharp
public class Grid
{
    private readonly Cell[,] _cells;

    public int Rows { get; }
    public int Columns { get; }

    public Grid(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        _cells = new Cell[rows, columns];
    }

    public bool IsInBounds(Position p) =>
        p.Row >= 0 && p.Row < Rows && p.Col >= 0 && p.Col < Columns;

    public bool IsEmpty(Position p) => IsInBounds(p) && _cells[p.Row, p.Col] is null;

    public Cell? GetCell(Position p) => IsInBounds(p) ? _cells[p.Row, p.Col] : null;

    // The only way to mutate the grid is through an explicit, validated method.
    public bool TryPlace(Position p, Cell cell)
    {
        if (!IsEmpty(p)) return false;
        _cells[p.Row, p.Col] = cell;
        return true;
    }

    public bool TryRemove(Position p)
    {
        if (IsInBounds(p) && _cells[p.Row, p.Col] is not null)
        {
            _cells[p.Row, p.Col] = null;
            return true;
        }
        return false;
    }
}
```

Notice `_cells` is `private readonly` — nothing outside `Grid` can ever reassign the array or write into it directly. Everything goes through `TryPlace` / `TryRemove`, which is where you'd enforce rules.

**Design tip:** favor "Try" methods that return `bool` (or a result type) over methods that throw exceptions for expected failure cases like "cell occupied." Reserve exceptions for truly exceptional situations (e.g., a corrupted save file), not for routine rule violations a player might trigger every turn.

---

## 2. Polymorphism & the Command Pattern for Move Variants

When a game has several _kinds_ of actions a player can take, and each kind has its own validation and effect on the board, it's tempting to write one giant method with a big `switch` statement. That works initially but gets harder to extend and test as
more move types are added.

A cleaner approach is the **Command pattern**: define an interface that represents "a thing that can be validated against game state and then applied to it," and give each move type its own class implementing that interface.

```csharp
public interface IMove
{
    // Returns null if legal, or a human-readable reason if not.
    string? Validate(GameState state);

    // Assumes Validate() has already passed.
    void Apply(GameState state);
}

public class PlaceMove : IMove
{
    private readonly Position _target;
    private readonly PieceType _pieceType;

    public PlaceMove(Position target, PieceType pieceType)
    {
        _target = target;
        _pieceType = pieceType;
    }

    public string? Validate(GameState state)
    {
        if (!state.Grid.IsInBounds(_target)) return "Target is off the grid.";
        if (!state.Grid.IsEmpty(_target)) return "Target cell is already occupied.";
        return null;
    }

    public void Apply(GameState state)
    {
        state.Grid.TryPlace(_target, new Cell(state.CurrentPlayer, _pieceType));
    }
}

public class RemoveMove : IMove
{
    private readonly Position _target;

    public RemoveMove(Position target) => _target = target;

    public string? Validate(GameState state)
    {
        var cell = state.Grid.GetCell(_target);
        if (cell is null) return "Target cell is empty — nothing to remove.";
        if (cell.Owner == state.CurrentPlayer) return "Cannot target your own piece.";
        return null;
    }

    public void Apply(GameState state) => state.Grid.TryRemove(_target);
}
```

The turn loop then doesn't need to know what kind of move it's dealing with:

```csharp
public bool TryExecuteTurn(IMove move, GameState state)
{
    var error = move.Validate(state);
    if (error is not null)
    {
        Console.WriteLine($"Illegal move: {error}");
        return false;
    }

    move.Apply(move is IMove m ? state : state);
    state.AdvanceTurn();
    return true;
}
```

**Why this pays off:**

- Adding a new move type means adding a new class, not editing a central `switch`.
- Each move type is independently unit-testable (`Validate` and `Apply` in isolation).
- The "illegal move, try again without advancing the turn" rule falls out naturally — you just don't call `AdvanceTurn()` when `Validate()` fails.

---

## 3. Separation of Concerns (Layered Architecture)

A common failure mode in console game projects is letting `Console.ReadLine()` and `Console.WriteLine()` calls creep directly into game-rule classes. This makes the logic hard to test (you'd need to simulate console input) and hard to reuse (e.g., if you later wanted a script-driven "headless" mode).

A simple layering that works well:

```
┌─────────────────────────────┐
│   Presentation / I/O layer  │  Console.ReadLine, Console.WriteLine,
│                              │  rendering the grid as text
└───────────────▲──────────────┘
                │  plain data in/out (strings, DTOs)
┌───────────────┴──────────────┐
│   Application / orchestration│  turn loop, mode selection,
│                              │  wiring parser → rules → renderer
└───────────────▲──────────────┘
                │  method calls on domain objects
┌───────────────┴──────────────┐
│   Domain / rules layer       │  Grid, GameState, IMove implementations,
│                              │  win-condition checking — no Console calls
└───────────────────────────────┘
```

The domain layer should be able to run and be tested with **zero** references to `Console`. A useful gut-check: could you write a unit test for "placing a piece at an occupied cell is rejected" without printing anything or reading input? If not, I/O concerns have leaked into the rules layer.

```csharp
// Domain layer — no Console calls anywhere.
public class GameState
{
    public Grid Grid { get; }
    public Player CurrentPlayer { get; private set; }

    public GameState(Grid grid, Player startingPlayer)
    {
        Grid = grid;
        CurrentPlayer = startingPlayer;
    }

    public void AdvanceTurn()
    {
        CurrentPlayer = CurrentPlayer == Player.One ? Player.Two : Player.One;
    }
}

// Presentation layer — talks to the domain layer, never the other way around.
public class ConsoleRenderer
{
    public void Render(Grid grid)
    {
        for (int r = 0; r < grid.Rows; r++)
        {
            for (int c = 0; c < grid.Columns; c++)
            {
                var cell = grid.GetCell(new Position(r, c));
                Console.Write(cell is null ? "." : cell.Symbol);
            }
            Console.WriteLine();
        }
    }
}
```

This separation is also what lets you support both an interactive mode and a script-driven automated mode without duplicating rule logic: both modes just produc `IMove` instances and hand them to the same domain layer.

---

## 4. Serialization for Save / Load

A frequent mistake is trying to directly serialize your live game objects. This tends to break because:

- Rich domain objects often have behavior, private fields, or circular references that don't serialize cleanly.
- You don't want your on-disk file format tightly coupled to your internal class design — refactoring a class shouldn't silently corrupt old save files.

The cleaner pattern is a **DTO (data transfer object)**: a plain, serialization-only class that mirrors the _data_ you need to persist, with a mapping step to/from your real domain objects.

```csharp
// Plain data, no behavior — this is what gets written to disk.
public class GameStateDto
{
    public int Rows { get; set; }
    public int Columns { get; set; }
    public List<CellDto> OccupiedCells { get; set; } = new();
    public string CurrentPlayer { get; set; } = "";
}

public class CellDto
{
    public int Row { get; set; }
    public int Col { get; set; }
    public string Owner { get; set; } = "";
    public string PieceType { get; set; } = "";
}
```

Mapping and file I/O:

```csharp
using System.Text.Json;

public class SaveLoadService
{
    public void Save(GameState state, string path)
    {
        var dto = new GameStateDto
        {
            Rows = state.Grid.Rows,
            Columns = state.Grid.Columns,
            CurrentPlayer = state.CurrentPlayer.ToString(),
            OccupiedCells = CollectOccupiedCells(state.Grid)
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    public GameState Load(string path)
    {
        var json = File.ReadAllText(path);
        var dto = JsonSerializer.Deserialize<GameStateDto>(json)
                   ?? throw new InvalidDataException("Save file is corrupted or empty.");

        var grid = new Grid(dto.Rows, dto.Columns);
        foreach (var cellDto in dto.OccupiedCells)
        {
            grid.TryPlace(
                new Position(cellDto.Row, cellDto.Col),
                new Cell(Enum.Parse<Player>(cellDto.Owner), Enum.Parse<PieceType>(cellDto.PieceType)));
        }

        return new GameState(grid, Enum.Parse<Player>(dto.CurrentPlayer));
    }

    private List<CellDto> CollectOccupiedCells(Grid grid)
    {
        var result = new List<CellDto>();
        for (int r = 0; r < grid.Rows; r++)
            for (int c = 0; c < grid.Columns; c++)
            {
                var cell = grid.GetCell(new Position(r, c));
                if (cell is not null)
                    result.Add(new CellDto { Row = r, Col = c, Owner = cell.Owner.ToString(), PieceType = cell.PieceType.ToString() });
            }
        return result;
    }
}
```

**Design tip:** wrap file operations (`File.ReadAllText`, `File.WriteAllText`) in try/catch at the boundary (e.g., "file not found," "permission denied") and surface a clear message — don't let an unhandled `IOException` crash the whole program.

---

## 5. Input Parsing & Validation

Keep parsing (turning a raw string into a structured, typed request) separate from validation (deciding whether that request is legal given current game state).
Combining them makes both harder to test and reuse.

```csharp
public record ParsedInput(string Kind, Position Target);

public class InputParser
{
    // Returns null if the string doesn't match the expected shape at all —
    // this is a *syntax* problem, distinct from a *rules* problem.
    public ParsedInput? TryParse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var kind = raw[0].ToString();
        var rest = raw[1..];
        var parts = rest.Split(':');
        if (parts.Length != 2) return null;

        if (!int.TryParse(parts[0], out var row)) return null;
        if (!int.TryParse(parts[1], out var col)) return null;

        // Convert from 1-based human input to 0-based internal coordinates here,
        // in one place, rather than scattering -1s through the domain layer.
        return new ParsedInput(kind, new Position(row - 1, col - 1));
    }
}
```

This gives you two distinct, testable failure modes:

1. **Syntax error** — `TryParse` returns `null` ("that's not a command I recognize").
2. **Rule violation** — parsing succeeded, but `IMove.Validate()` rejected it ("that's a legal-looking command, but not allowed right now").

Reporting these differently to the user (and handling them separately in tests) tends to produce a much more robust input-handling story than one big try/catch around "parse and apply."

---

## Further reading (general, not assignment-specific)

- _Design Patterns_ (Gang of Four) — Command pattern, Strategy pattern (useful for
  swappable AI behavior).
- Microsoft Learn: [Object-oriented programming (C#)](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/object-oriented/)
- Microsoft Learn: [System.Text.Json overview](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview)
