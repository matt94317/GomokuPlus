# Assignment 1 Submission README

- **Student Name:** SHAOKUAN YU
- **Student ID:** n12191434
- **Unit:** IFN584 Object-Oriented Design and Development
- **Recording File:** `StudentID.cast` (replace with your own student ID)

## How to Run

From the repository root:

```bash
cd GomokuPlus
dotnet run
```

This starts a Human vs. Human game on a 10x10 board. On each turn, enter a move as `<O|H|E><row>:<col>` (rows and columns are 1-based):

- `O3:4` — place an ordinary stone at row 3, column 4
- `H5:5` — place a heavy stone at row 5, column 5 (2 per player)
- `E2:2` — erase an opponent's ordinary stone at row 2, column 2 (2 per player)

The game ends when a player gets 5 in a row (horizontally, vertically, or diagonally) or the board fills up.

### Automated Testing Mode

Pass a single comma-separated string of moves as a command-line argument to run a scripted game with no prompts. The moves are applied in order (alternating players, same rules as interactive mode), and only the final board is printed:

```bash
dotnet run "O3:2,O3:3,O3:1,O4:3,O3:4,O5:3,E3:3,H3:3"
```

## Feature Implementation Claim Table

| Requirement                       | Status                | asciinema Timestamp (mm:ss) | Notes |
| :-------------------------------- | :-------------------- | :-------------------------- | :---- |
| **Human vs. human (HvH) mode**    | Working / Not Working | 01:00                       | N/A   |
| **Human vs. computer (HvC) mode** | Working / Not Working | 02:00                       | N/A   |
| **Save / load state persistence** | Working / Not Working | 03:00                       | N/A   |
| **Heavy stone**                   | Working / Not Working | 04:00                       | N/A   |
| **Eraser stone**                  | Working / Not Working | 05:00                       | N/A   |
| **Automated testing script mode** | Working / Not Working | 06:00                       | N/A   |
