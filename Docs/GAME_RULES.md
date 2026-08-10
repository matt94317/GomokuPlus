# Gomoku Plus — Game Rules

## 1. Overview

Gomoku Plus is a two-player strategy game played on a square grid. Players take turns placing stones on empty intersections, and the first player to form an unbroken line of five stones of their own color — horizontally, vertically, or diagonally — wins the game.

Gomoku Plus extends classic Gomoku with two special stone types, **Heavy Stone** and **Eraser Stone**, in addition to the standard stone.

## 2. Board

- The board is a grid of intersections, default size **15 x 15**.
- All intersections are empty at the start of the game.
- Rows and columns are addressed as `(row, col)`, zero-indexed from the top-left corner.

## 3. Players and Modes

- **Human vs. Human (HvH)** — two human players alternate turns.
- **Human vs. Computer (HvC)** — a human player alternates turns with a
  computer-controlled opponent.
- One player plays **Black**, the other plays **White**. Black moves first.

## 4. Turn Sequence

1. The current player selects an empty intersection on the board.
2. The player chooses which stone type to place (Standard, Heavy, or Eraser), subject to any per-game limits on special stones.
3. The stone is placed (or the eraser effect is resolved — see §6).
4. The game checks for a win condition (§7).
5. Turn passes to the other player.

## 5. Standard Stone

- Placed on any empty intersection.
- Behaves exactly as in classic Gomoku: it stays on the board for the remainder of the game and counts toward line-of-five win checks.

## 6. Special Stones

> **Note:** The exact mechanics below are placeholders based on common Gomoku variant designs. Confirm these against your assignment specification and update this section if they differ.

### 6.1 Heavy Stone

- A Heavy Stone may be placed on any empty intersection, the same as a Standard Stone.
- Once placed, a Heavy Stone **cannot be removed or overwritten** by an Eraser Stone (see §6.2) — it permanently occupies its square.
- Each player has a limited number of Heavy Stones available per game (default: **1**). Once used, the player may only place Standard or Eraser stones for the rest of the game.
- A Heavy Stone counts as that player's color for the purposes of forming a line of five.

### 6.2 Eraser Stone

- Rather than being placed on an empty square, an Eraser Stone is played by **selecting an occupied intersection** containing an opponent's Standard Stone.
- The targeted stone is removed from the board, returning that intersection to empty. The Eraser Stone itself is **consumed** and is not placed on the board.
- An Eraser Stone **cannot** target a Heavy Stone (§6.1) or an empty intersection.
- Each player has a limited number of Eraser Stones available per game (default: **1**).

## 7. Win Condition

- A player wins immediately when they have **five or more** of their own stones (Standard and/or Heavy) in an unbroken line, horizontally, vertically, or diagonally.
- If the board fills completely with no player achieving five in a row, the game ends in a **draw**.

## 8. Save / Load

- The game state (board contents, stone types, current turn, and remaining special-stone counts for each player) can be saved to disk at any point between turns.
- A saved game can be loaded to resume play from exactly the saved state.

## 9. Automated Testing Script Mode

- The game supports an automated mode that reads a predefined sequence of moves (including special stone placements) from a script file and plays them in order without manual input, for the purpose of demonstrating and verifying game logic.

## 10. Illegal Moves

- Placing any stone on an already-occupied intersection is illegal.
- Placing a Heavy Stone or Eraser Stone beyond a player's per-game allowance is illegal.
- Targeting an empty intersection or a Heavy Stone with an Eraser Stone is illegal.
- Illegal moves are rejected and the player must choose again without advancing the turn.
