# guardy-angel-2069

# 🎴 Card Matching Game

A simple **Card Matching Game** built with **Unity**, featuring dynamic grids, scoring, combo system, timer, persistent high scores, and basic sound effects. Works on **Desktop** and **Mobile (Android/iOS)** platforms.

---

## 📂 Project Structure

- **Scripts/**
  - `Card.cs` – Handles card behaviour: flipping, fading, selection logic.
  - `CardGridManager.cs` – Manages card grid, game flow, scoring, combo logic, timer, and win conditions.
  - `SaveManager.cs` – Saves and loads scores using `PlayerPrefs`.
  - `AudioPlayer.cs` – Plays sounds: card flip, match, mismatch, and game over.
  - `MainMenuUI.cs` – Displays high score using **TextMeshPro**.

- **Prefabs/**
  - `CardPrefab` – Template for each card (Image + Button).

- **Sprites/**
  - Card faces and card back images.

- **Scenes/**
  - `MainMenu` – Main menu with high score display.
  - `GameScene` – Gameplay scene with card grid.

- **UI Elements**
  - TextMeshPro components for score, combo, high score, and timer.
  - Slider to adjust grid size.
  - Panels for gameplay and informational UI.

- **Audio/**
  - Sound effects for:
    - Card flip
    - Match
    - Mismatch
    - Game over

---

## 🎮 How to Play

1. Launch the game.
2. The main menu displays the **high score**.
3. Click **Start** to begin.
4. Cards are briefly flipped to reveal their faces for memorisation.
5. Click cards to flip them and find matching pairs.
6. Scoring:
   - Correct match: **100 points** + combo bonus.
   - Consecutive matches increase the **combo multiplier**.
7. Timer tracks game duration.
8. Game ends when all pairs are matched.
9. High scores are saved and shown in the main menu.
10. Optional: **Give Up** to end the game early.

---

## ✨ Features

- Dynamic card grid (2x2 and larger)
- Combo-based scoring system
- Persistent high score
- Smooth card flip and fade animations
- Basic sound effects
- Cross-platform (Desktop + Mobile)

---

## ⚙️ Setup Instructions

1. Open project in **Unity** (recommended 2023.x or later).
2. Assign serialised fields in the inspector:
   - `CardPrefab`
   - Sprites (card faces and back)
   - TextMeshPro components (score, combo, high score, timer)
   - Audio clips for sound effects
3. Ensure `SaveManager` and `AudioPlayer` are present in the scene as singletons.
4. Build and run for your target platform.

---

## 📌 Notes

- Focus was on code optimisation and gameplay; visuals are minimal.
- High scores and progress persist between game sessions using `PlayerPrefs`.

---

