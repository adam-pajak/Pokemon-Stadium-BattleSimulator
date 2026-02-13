# Battle Core

This document describes the core battle classes located in `Battle/Core/`.
These classes form the heart of the battle engine. They define:
- what a battle state looks like
- how Pokémon are represented during a battle
- how moves are represented and executed

---

## 1. Core Classes Overview

The battle system is built around three main objects:

- `BattleContext` – stores global battle state
- `BattlePokemon` – represents a Pokémon inside a battle
- `BattleMove` – represents a move execution object

These classes are designed to work together and are used by:
- move effects (`Moves/Effects`)
- calculators (`Battle/Calculators`)
- trainers (`Battle/Trainers`)

## 2. BattleContext.cs

### Purpose
`BattleContext` represents the full state of the battle.
It exists for the entire battle duration and contains information shared between all battle systems. 
It can be treated as the "battle memory".
### Responsibilities
- stores both trainers and their teams including current active Pokémon at both sides
- stores battle-wide flags and multi-turn effects
- stores long-term conditions that must persist between turns
- provides a shared object passed into move effects and calculators

### Typical data stored inside BattleContext
Examples:
- current turn number
- field effects that affects both sides (Weather[^1], Terrains[^2], etc.)
- global battle flags
- references to trainers

### Why BattleContext exists
Move effects often require access to data outside a single Pokémon.
For example:
- checking which Pokémon is active
- applying effects to both sides
- reading and modifying long-term conditions

Because of that, `BattleContext` is passed around as a shared reference.

### Example usage
A move effect might use `BattleContext` to:
- determine attacker and defender
- apply damage and/or status to a target
- log battle messages

---

## 3. BattlePokemon.cs

### Purpose
`BattlePokemon` represents a Pokémon during battle.
It contains all runtime information such as HP, stat stages and moves.

It is separate from the base Pokémon model (`Models/Pokemon/Pokemon.cs`),
because battle requires additional dynamic state.

### Responsibilities
- stores current HP (hit points)[^3]
- stores all (battle) stats appropriate to his level[^4]
- stores current stat stages[^5]
- stores current status condition (Sleep, Burn, etc.)[^6]
- stores available moves[^7]
- provides methods for applying damage, healing and stat changes

### Important properties
Common values stored in a battle Pokémon:
- `CurrentHP`
- `Level`
- `Species` - `Pokemon` object which represents its species[^8]
- `BattleStats`
- `StatStages`
- list of known moves
- flags like `IsInvulnerable` or `IsCharging`

### Stat stages
Stat stages represent temporary stat modifiers.
They are applied using the Gen 1 stage multiplier rules.[^9]
Stage modifiers are stored in a separate class which can modify them.

### Example: applying damage
Typical battle flow:
1. `DamageCalculator` computes damage value
2. a `DamageEffect` applies the damage to the target `BattlePokemon`
3. fainting is checked

---

## 4. BattleMove.cs

### Purpose
`BattleMove` represents a move that is used during battle.
It contains all information required to execute a move and apply its effects.

Unlike the base `Move` model (`Models/Moves/Move.cs`), `BattleMove` is used directly inside the battle system.

### Responsibilities
- stores move metadata[^10]
- stores move target rules
- stores list of move effects
- provides the execution pipeline for applying effects

### Effects-based system
Moves in this project are built using modular effects.

A move may consist of multiple effects executed sequentially.

Example:
- `DamageEffect`
- `RecoilEffect`
- `StatChangeEffect`

This allows complex Gen 1 moves to be described without writing custom code for every move.

---

## 5. Battle Flow Using Core Classes

The typical execution of a single move looks like this:

1. Trainer chooses a move
2. A `BattleMove` object is selected
3. Accuracy check is performed
4. If the move hits, effects are executed in order (some effect are executed independent of hit)
5. Each effect modifies either:
    - `BattlePokemon`
    - `BattleContext`
    - `Trainer`
    - all of them

### Simplified pipeline
1. `BattleMove.Use(context)`
2. `AccuracyCalculator.DoesMoveHit(context)`
3. `foreach effect in Effects: effect.Apply(context)`

---

## 6. Core Design Principles

### 6.1 Separation of state and calculations
- `BattlePokemon` stores state
- calculators compute values (damage, accuracy, critical hits)

This makes the system cleaner and easier to debug.

### 6.2 Modular move effects
Instead of hardcoding each move individually,
the move system uses a list of reusable effects.

This makes it easy to add new moves and generations.

### 6.3 Context-driven logic
Battle logic requires a shared object to store global state.
That role is fulfilled by `BattleContext`.

## 7. Key Relationships Diagram
```
┌──────────────────────────┐
│ BattleContext            │
│ - trainers               │
│ - global battle state    │
└──────────────┬───────────┘
               │
               │ used by
               v
┌──────────────────────────┐
│ BattleMove               │
│ - move metadata          │
│ - list of effects        │
└──────────────┬───────────┘
               │
               │ applies effects to
               v
┌──────────────────────────┐
│ BattlePokemon            │
│ - HP                     │
│ - status                 │
│ - stat stages            │
└──────────────────────────┘
```

---

## 8. Common Scenarios

### 8.1 Damage move
- `BattleMove` contains `DamageEffect`
- `DamageEffect` uses `DamageCalculator`
- target Pokémon loses HP

### 8.2 Stat-changing move
- `BattleMove` contains `StatChangeEffect`
- stat stage is modified inside `BattlePokemon`

### 8.3 Multi-hit move
- `BattleMove` contains `MultistrikeEffect`
- effect triggers damage multiple times

---

## 9. Summary

The battle engine core is based on:
- a global battle state (`BattleContext`)
- battle Pokémon state objects (`BattlePokemon`)
- moves built from modular effects (`BattleMove`)

This design is inspired by classic Pokémon mechanics and allows easy extension
through additional effects, calculators and battle rules.

[^1]: Not implemented - reference: https://bulbapedia.bulbagarden.net/wiki/Weather
[^2]: Not implemented - reference: https://bulbapedia.bulbagarden.net/wiki/Terrain
[^3]: https://bulbapedia.bulbagarden.net/wiki/HP
[^4]: Attack, Defense, Special Attack, Special Defense, Speed and in-battle stats: Accuracy, Evasion. Reference: https://bulbapedia.bulbagarden.net/wiki/Stat
[^5]: https://bulbapedia.bulbagarden.net/wiki/Stat_modifier
[^6]: Not implemented - reference: https://bulbapedia.bulbagarden.net/wiki/Status_condition
[^7]: As a class BattleMove. Reference: https://bulbapedia.bulbagarden.net/wiki/Move
[^8]: https://bulbapedia.bulbagarden.net/wiki/Pok%C3%A9mon_(species)
[^9]: See stages multipliers section: https://bulbapedia.bulbagarden.net/wiki/Stat_modifier 
[^10]: Name, Power, Accuracy, Type, Category - Reference: https://bulbapedia.bulbagarden.net/wiki/Move
