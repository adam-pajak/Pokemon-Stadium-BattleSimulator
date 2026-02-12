# Project Architecture

This document describes the internal architecture of **Pokemon Stadium Battle Simulator**.
It explains how the battle engine is structured, which modules are responsible for what,
and how the main battle loop works.

---

## 1. High-level Overview

The project is divided into several main modules:

- **Battle/** – core battle logic (calculators, trainers, stats)
- **Models/Moves/** – move definitions and effect execution system
- **Models/Pokemon/** – Pokémon models and base stats
- **Models/Enums** – enums used for loading data and in battle logic
- **Models/DTO** – data transfer objects
- **Services/** – game loop and data loading
- **Data/** – JSON data files (Currently Gen 1 Pokémon and moves)
- **Program.cs** - entry point of the program

The program is designed so that the battle logic is separated from the UI (currently CLI) but not entirely at this point.

---

## 2. Entry Point

### Program.cs
`Program.cs` is the entry point of the application.

Responsibilities:
- calling `DataLoader`
- collecting game setup information from the user:
  - trainer names
  - number of Pokémon in each team
  - selecting Pokémon for both trainers
- creating both trainers and initializing the game loop in `Services/Game.cs`
- reviewing battle logs after battle

### DataLoader.cs
The `DataLoader` class loads JSON files from `/Data`.

Responsibilities:
- parsing `pokemon_gen1.json`
- parsing `moves_gen1.json`
- mapping DTO objects into internal models

Loaded data is later used to create `Pokemon` and `Move` objects.

---

## 3. Services Layer

### Game.cs
The `Game` is a static class responsible for the overall game flow.

Responsibilities:
- running the battle loop until the game ends
- printing some basic information to the console

Game loop includes:
1. victory condition check
2. turn loop (until battle ends)

---

## 4. Battle Module

The `Battle/` folder contains the main battle engine logic.
The battle engine is designed around a few key objects:

- `BattleContext`
- `BattlePokemon`
- `BattleMove`
- `Trainer`

---

## 5. Battle Core Objects

### BattleContext.cs
`BattleContext` is the most important class in the battle engine.
It stores the current state of the battle.

Responsibilities:
- stores information about both trainers including active Pokémon
- stores battle-related flags and long-term effects
- provides shared access to battle state for move effects and calculators

`BattleContext` exists for the entire battle duration.

Typical data stored in `BattleContext`:
- current turn number
- short-term information - (`Trainer`)attacker, defender, move
- information needed across multiple turns (`LastMove`/`LastDamage`/etc.)

---

### BattlePokemon.cs
`BattlePokemon` represents a Pokémon currently participating in a battle.

Responsibilities:
- stores current HP
- stores stats and stat stages (Attack/Defense/etc.)
- stores active status conditions[^1]
- stores current move set
- stores current types (which can change during the battle)
- provides helper methods for applying damage and stat changes

This class is a battle-specific wrapper around the base Pokémon model.

---

### BattleMove.cs
`BattleMove` represents a move that is being used in battle.

Responsibilities:
- stores move metadata (power, accuracy, PP, type)
- contains the list of effects executed when the move is used
- provides execution pipeline for move effects

A single move may contain multiple effects (example: damage + status effect).

---

## 6. Trainers System

### Trainer.cs
The `Trainer` class represents a battle participant.

Responsibilities:
- stores trainer name
- stores team of Pokémon
- stores currently active Pokémon
- selects actions during a turn

### PlayerTrainer.cs
`PlayerTrainer` is controlled by the user via CLI input.

Responsibilities:
- asking the player for actions (move / switch)
- validating chosen actions

### ComputerTrainer.cs
`ComputerTrainer` is AI-controlled.

Responsibilities:
- automatically selects moves
- can't switch Pokémon as in the original games

---

## 7. Turn System Overview

A battle is executed as a loop of turns.

Each turn consists of:
1. Player selects an action (move or switch)
2. Opponent selects an action
3. Turn order is calculated (based on speed, priority, etc.)
4. Actions are executed in correct order
5. End-of-turn effects are applied
6. Check if battle is finished

Turn order is affected by:
- Speed stat (after modifiers)
- move priority
- status conditions (example: paralysis speed reduction)[^1]

---

## 8. Calculators

The folder `Battle/Calculators` contains independent calculation modules.

### DamageCalculator.cs
Responsible for calculating damage based on:
- move power
- attacker stats
- defender stats
- type effectiveness
- critical hits
- randomness (if implemented)

### AccuracyCalculator.cs
Responsible for hit/miss checks based on:
- move accuracy
- accuracy/evasion stages

### CriticalHitCalculator.cs
Responsible for checking critical hits.

### TypeEffectivenessChart.cs
Stores Gen 1 type effectiveness multipliers.

These calculators are used by move effects to keep the move system modular.

---

## 9. Stats System

The folder `Battle/Stats` provides a stat model used by the battle engine.

### BattleStats.cs
Stores current calculated stats (from `StatCalculator.cs`).

### StatStages.cs
Stores stat stage modifiers (Attack, Defense, etc.) and manages them.

### StatStageMultipliers.cs
Contains multiplier values used for stage calculation.

### Fraction.cs
Represents rational numbers used in calculations
(to avoid floating-point inaccuracies).

### StatCalculator.cs
Responsible for calculating `BattleStats` of `BattlePokemon` basing on its level.

---

## 10. Moves System

The `Moves/` folder contains logic for moves and their effects.

### Move.cs
Represents a move definition.

Responsibilities:
- stores base move information (name, type, category)
- stores list of effects executed on use

---

## 11. Effects System

The folder `Moves/Effects/` contains modular implementations of move effects.

### IMoveEffect.cs
The interface implemented by all effects.

Each effect should implement logic like for example:
- dealing damage
- applying status
- modifying stats
- multi-hit attacks
- recoil

Effects can be combined to create complex moves.

Example move structure:
- DamageEffect
- StatusEffect
- StatChangeEffect

---

## 12. Models Layer

The folder `Models/` contains DTO classes and enums.

### DTO/
DTO classes are used for loading JSON files.

Example:
- `PokemonDto`
- `MoveDto`
- `MoveEffectDto`

### Enums/
Enums provide strongly typed battle constants.

Example:
- `PokemonType`
- `MoveCategory`
- `Status`
- `Stat`
- `Targets`
- `CriticalRatio`
- `EffectType`

---

## 13. Data Layer

The folder `Data/` contains raw JSON files.

- `pokemon_gen1.json` – list of Pokémon and base stats
- `moves_gen1.json` – list of moves and their effects

These files allow easy modification of the available Pokémon and moves.

---

## 14. Design Goals

Main design goals of this project:

- modular move effect system
- separation of data and logic (JSON-driven moves/Pokémon)
- extensibility (adding new effects and mechanics easily)
- readable code and clean structure

---

## 15. Future Improvements

Planned improvements:
- fully implemented status conditions
- UI improvements (HP bars)
- more Pokémon generations
- more move effects
- smarter AI logic

---

## 16. Summary

The engine is based on a modular design where:
- `Game` controls the main loop
- `BattleContext` stores battle state
- `Trainers` decide actions
- `Moves` execute lists of `Effects`
- `Calculators` handle formulas and rules

This structure allows the battle simulator to grow into a complete Pokémon battle engine.

---

[^1]: Not implemented.
