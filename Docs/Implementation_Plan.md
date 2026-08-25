# GomokuPlus — Implementation Plan

Step-by-step roadmap and architectural blueprint for the GomokuPlus assignment.

---

## 🏗️ 1. Proposed Object-Oriented Architecture

To keep the code clean, modular, and easy to grade for OO quality (Category 6), separate domain logic, board representation, and user interaction.

```
                  ┌──────────────────────┐
                  │     Program.cs       │ (Entry point & CLI args parser)
                  └──────────┬───────────┘
                             │
                  ┌──────────▼───────────┐
                  │     GameEngine       │ (Controls game loop, turns, save/load)
                  └─────┬──────────┬─────┘
                        │          │
         ┌──────────────┴─┐      ┌─┴──────────────┐
         │     Board      │      │  Player (Base) │
         └───────┬────────┘      └───┬────────┬───┘
                 │                   │        │
         ┌───────▼────────┐   ┌──────▼──┐  ┌──▼─────────┐
         │      Cell      │   │  Human  │  │ Computer   │
         └────────────────┘   └─────────┘  └────────────┘
```

### Key Classes & Responsibilities

- **`Board`**:
  - Manages the 10 × 10 grid (`Cell[,]` array).
  - Handles rendering the grid with `+---+` lines, pipe dividers `|`, and row/column numbers.
  - Checks victory conditions (detects 5 consecutive matching symbols horizontally, vertically, or diagonally).

- **`Cell`**:
  - Represents a single coordinate on the grid.
  - Tracks what type of stone is present (`Ordinary`, `Heavy`, or `Empty`) and who owns it.

- **`Player` (Abstract Base Class)**:
  - Properties: Name, Symbol (`X` or `O`), Heavy Symbol (`@` or `#`), remaining Heavy Stone count (2), remaining Eraser count (2).
  - Subclasses:
    - **`HumanPlayer`**: Prompts the user for terminal input.
    - **`ComputerPlayer`**: Generates a valid move automatically (e.g., selecting a random empty cell).

- **`Move` / `Command`**:
  - Represents a parsed action (e.g., Type: `Ordinary`, Row: `3`, Col: `4`).

- **`GameEngine`**:
  - Manages the turn loop, player switching, validation checks, save/load features, and win detection.

---

## 🚀 2. Step-by-Step Implementation Guide

### Phase 1: Project Setup & Core Models

1. **Initialize Project:** Create a new .NET 10 console application:
   ```bash
   dotnet new console -n GomokuPlus
   ```
2. **Build the Grid Structure:**
   - Implement `Cell` and `Board` classes.
   - Write a method in `Board` to render the grid exactly as required by the spec.

### Phase 2: Basic Gameplay (Human vs. Human)

1. **Command Parser:**
   - Create a helper to convert strings like `O3:4` or `H5:5` into row, column, and action type.
2. **Move Validation:**
   - Ensure coordinates fall within bounds (1–10).
   - Prevent placing on already occupied cells.
3. **Win Detection Engine:**
   - Write algorithms to inspect row, column, and diagonal lines passing through the last placed stone to check for 5 matching consecutive stones.
4. **Game Loop:**
   - Implement alternating turns between two `HumanPlayer` objects in `GameEngine`.

### Phase 3: Special Stones & Mechanics

1. **Inventory Management:**
   - Track remaining `Heavy` (2) and `Eraser` (2) stones per player.
2. **Heavy Stone Rule (`H`):**
   - Place stone with symbol `@` (P1) or `#` (P2). Ensure win detection accounts for these.
3. **Eraser Stone Rule (`E`):**
   - Target an opponent's cell.
   - Check validation: ensure the target is an **Ordinary Stone** (reject if empty or targeting a Heavy Stone).
   - Clear the cell and decrement the player's Eraser inventory.

### Phase 4: Save / Load & Computer AI

1. **Save / Load Persistence:**
   - Implement a JSON or custom text file writer/reader to store board contents, turn counts, current player, game mode, and remaining special stone counts.
2. **Computer Player (HvC):**
   - Create `ComputerPlayer` inheriting from `Player`.
   - Implement basic AI (e.g., randomly scanning for valid empty cells and executing standard moves).

### Phase 5: Automated Testing Mode & CLI Parsing

1. **Script Parsing Mode:**
   - In `Program.cs`, check if command-line arguments are present (`args.Length > 0`).
   - If an argument like `"O3:2,O3:3,O3:1..."` is passed, split by comma `,` and execute moves sequentially without interactive prompts, then output the final grid state.

---

## 📹 3. Pre-Submission Checklist

Before recording the `asciinema` video and submitting:

- Ensure no GenAI tools are used directly in the submitted work.
- Run `dotnet clean` to remove build directories (`bin/`, `obj/`).
- Prepare mandatory terminal identification steps (`echo`, `whoami`, `dotnet --info`).
- Verify timestamps for every feature listed in the `README.md` claim table.
