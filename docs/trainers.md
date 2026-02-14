# Trainers System

This document describes the trainer system located in: `Battle/Trainers/`


The trainer system is responsible for decision-making during battle:
choosing moves, switching Pokémon and providing the battle engine with a valid action each turn.

---

## 1. Overview

A trainer represents one side of the battle.

In this project, trainers are implemented as objects that:
- store a team of Pokémon
- store information about the currently active Pokémon
- choose an action each turn

The battle engine does not directly handle user input or AI logic.
Instead, it asks trainers for their decision.

---

## 2. Trainer.cs (Base Class)

### Purpose
`Trainer` is the base (abstract) class for all trainer types.
It provides common functionality shared between player-controlled and AI-controlled trainers.

### Responsibilities
- stores trainer name
- stores Pokémon team
- stores active Pokémon
- provides switching functionality

### Typical data stored in Trainer
A trainer object usually stores:
- `Name`
- list of `BattlePokemon` (team)
- active `BattlePokemon`
- methods for checking if the trainer has remaining Pokémon

### Common helper logic
Trainer class contains logic such as:
- checking if the trainer has any Pokémon alive
- validating if switching is possible
- setting active Pokémon

---

## 3. PlayerTrainer.cs

### Purpose
`PlayerTrainer` represents a trainer controlled by the user.

It is responsible for collecting player decisions through the CLI interface.

### Responsibilities
- displaying available actions
- allowing the player to select a move
- allowing the player to switch Pokémon
- validating the selected action

### Input validation
PlayerTrainer must ensure that:
- selected move exists
- move has remaining PP
- move is not disabled

### Action types supported
The player can usually select:
- **Move action** (choose one of the available moves)
- **Switch action** (choose another Pokémon from the team)

The action is defined as BattleMove?. When value is null that means the player chosen to switch.
If trainer doesn't have any Pokémon to switch, Move action will be chosen automatically.

---

## 4. ComputerTrainer.cs

### Purpose
`ComputerTrainer` represents an AI-controlled trainer.

It selects actions automatically without user input.

### Responsibilities
- selecting random moves each turn
- making basic decisions based on battle state

### Current AI behavior
The current AI logic may be simple (example):
- choose a random move
- switch only if forced

This system is designed so that AI can later be expanded into a smarter decision-making algorithm.

### Possible future AI improvements
- selecting the most effective move based on type effectiveness
- switching to resist incoming moves
- avoiding low-accuracy moves
- finishing low HP targets

---

## 5. Trainer Actions

A trainer must return a valid action each turn.

The engine expects one of the following decisions:

### 5.1 Move Action
The trainer selects one move from the active Pokémon's move set.

Example:
- Pikachu uses Thunderbolt

### 5.2 Switch Action
The trainer switches the currently active Pokémon.

Example:
- Pikachu is recalled
- Golem is sent out

---

## 6. How Trainers Are Used During a Turn

During each battle turn:

1. The battle engine asks the player trainer for an action
2. The battle engine asks the computer trainer for an action
3. Turn order is calculated
4. Both actions are executed

The battle engine does not care if the trainer is controlled by a human or AI.
It only expects a valid decision.

### Simplified logic
PlayerA is a user trainer and PlayerB is a computer trainer.
```
movePlayerA = playerA.ChooseAction()
movePlayerB = playerB.ChooseAction()

AssignAndExecuteOrder(movePlayerA, movePlayerB)
```

---

## 7. Switching Rules

Switching is an important part of the battle system.

A switch action should typically:
- replace the active Pokémon
- reset temporary move-related states[^1]
- update the battle log (inside `Services/Game` module)


Switching may also trigger additional effects in future versions:
- entry hazards (Spikes, etc.)
- abilities (future generations)

---

## 8. Relationship With BattleContext

Trainers are stored inside `BattleContext`.

`BattleContext` allows:
- move effects to access trainer teams:
  - effects can lock trainers choice (two-turn moves) 
  - switching effects to change the active Pokémon (in gen 1 no such move and effect exist)
- victory checks to evaluate remaining Pokémon

Example usage:
- determining whether a trainer has lost all Pokémon

---

## 9. Summary

The trainer system is built using inheritance:

- `Trainer` provides shared team and battle logic
- `PlayerTrainer` provides CLI-controlled decisions
- `ComputerTrainer` provides AI-controlled decisions

This design makes it easy to add new trainer types in the future,
for example:
- online multiplayer trainer
- scripted trainer (Gym Leader behavior)
- advanced AI trainer

## Notes

[^1]: Currently Disable
