# Moves system

This document describes how moves are represented and executed in the battle engine.

The project uses a modular move system inspired by Pokémon Stadium / Gen 1 mechanics.
Each move is defined by its data (name, type, power, accuracy etc.) and one or more effects
(damage, stat change, multistrike, etc.).

---

## 1. Overview

A move in this project exists in two main forms:

- **Move (`Models/Moves/Move.cs`)**  
  Represents the move definition loaded from JSON (static move data) and it's effects.

- **BattleMove (`Battle/Core/BattleMove.cs`)**  
  Represents a move in battle context (includes execution logic and battle-related handling).

Moves are executed through a set of effect classes implementing `IMoveEffect`.

---

## 2. Move.cs (Moves/Move.cs)

### Purpose
`Move` represents a "pure move definition" loaded from the JSON database.

It contains all important information about a move, such as:
- name
- type[^1]
- category (physical / special / status)[^2]
- PP[^3]
- base power[^4]
- accuracy[^5]
- priority[^6]
- target (self, enemy, all)[^7]
- effect definition

### Notes
This class should not contain battle-specific state.
It is used as a template for creating `BattleMove`.

---

## 3. BattleMove.cs (Battle/Core/BattleMove.cs)

### Purpose
`BattleMove` is the move object used directly inside battle logic.

It wraps a `Move` definition and adds execution-related behavior.

### Responsibilities
- validating whether a move can be executed
- handling battle-specific execution rules
- connecting move execution with effect system (`IMoveEffect`)

### Example responsibilities in the battle loop
- accuracy check
- applying move effects one by one for example: damage -> recoil

---

## 4. Move Categories (MoveCategory.cs)

Moves are classified into one of the following categories:

- **Physical**
- **Special**
- **Status**

This affects which stat is used in damage calculations.

In Gen 1 mechanics:
- physical moves use Attack / Defense
- special moves use Special Attack / Special Defense

If move category is Status it will not deal any direct damage.   

---

## 5. Targets (Targets.cs)

The `Targets` enum defines what a move can affect.

Examples:
- single enemy
- self
- all enemies (future)[^8]

Target selection is handled by the trainer logic, but the move definition
controls what is allowed (the move can't be chosen when it can't be used).

---

## 6. Move Effects System

The engine uses a modular effect system.

A move does not hardcode its behavior.
Instead, it uses one or more objects implementing: `IMoveEffect`.

This allows moves to be extended easily without rewriting the battle engine.

### Why this approach?
Because Pokémon moves share many repeated patterns:

- deal damage
- modify stat stages
- deal recoil damage, heal HP
- deal fixed damage
- etc.

Instead of hardcoding every move separately, the engine defines reusable effects.

---

## 7. IMoveEffect.cs

### Purpose
`IMoveEffect` defines a common interface for all move effects.

Every effect must implement a method that applies its logic to the battle state.

### Typical effect responsibilities
- modifying properties of BattleContext:
  - `Trainer` (locking action choice)
  - `BattlePokemon` (modifying HP, stat stages, etc.)
- generating battle logs

---

## 8. EffectType.cs

The enum `EffectType` defines the type of effect a move uses.

Example types:
- Damage
- StatChange
- Multistrike
- CounterDamage
- etc.

The effect type is typically loaded from JSON and used to determine which `IMoveEffect`
implementation should be created. Learn more in [data-loader.cs](data-loader.cs).

---

## 9. Effects Folder (Moves/Effects/)

This folder contains implementations of `IMoveEffect`.

The engine supports multiple effects (17 implemented).
Each effect class exists to represent a reusable battle behavior.
Learn more in [this file](move-effects.md).

New effects can be added by:
- creating a new `IMoveEffect` implementation
- adding its `EffectType`
- connecting it in the effect factory (`DataLoader`)
For details see: [adding-effects.md](adding-effects.md)
---

## 10. Move Loading (DTO + JSON)

Moves are stored in:

- `Data/moves_gen1.json`

Loading flow:

1. JSON is parsed into `MoveDto`
2. DTO is mapped into `Move`
3. Each move definition is stored in memory and `BattleContext`
4. When `Pokemon` for battle was chosen, they are becoming `BattlePokemon` and its moves are becoming a list of `BattleMove`

### DTO Classes
Located in:

- `Models/DTO/MoveDto.cs`
- `Models/DTO/MoveEffectDto.cs`

DTOs exist to separate raw JSON structure from internal model structure.

---

## 11. Move Execution Flow

The following simplified flow shows how moves are executed in battle:

```text
Trainer chooses action
    ↓
Battle engine determines move order
    ↓
BattleMove is executed
    ↓
AccuracyCalculator checks if move hits
    ↓

→ If hit:
    ↓
Move effects execute (IMoveEffect)
    
→ Else:
    ↓
Only some effects execute (for example: ChargingTurnEffect / SelfdestructEffect)

    ↓
Effect are applied
    ↓
Battle continues or ends if a Pokémon faints
```
Moves do not directly modify the game state.
Instead, they use the effect system and calculators.

## 12. Notes About Gen 1 / Stadium Mechanics

Pokémon Stadium is based on Gen 1 mechanics, which include special rules such as:
	•	critical hit rate depends on speed (Gen 1 rule)
	•	some moves behave differently than in later generations

These rules are implemented inside calculators and effects rather than inside move data.

## 13. Summary

The move system is built on 3 layers:
	•	Move definitions (`Move`, loaded from JSON)
	•	Battle representation (`BattleMove`, used in battle execution)
	•	Effect system (`IMoveEffect`, reusable modular behaviors)

This architecture makes it easy to expand the engine by adding new effects and move data.
## Notes
[^1]: https://bulbapedia.bulbagarden.net/wiki/Type
[^2]: https://bulbapedia.bulbagarden.net/wiki/Damage_category
[^3]: https://bulbapedia.bulbagarden.net/wiki/PP
[^4]: https://bulbapedia.bulbagarden.net/wiki/Power
[^5]: https://bulbapedia.bulbagarden.net/wiki/Accuracy
[^6]: https://bulbapedia.bulbagarden.net/wiki/Priority
[^7]: https://bulbapedia.bulbagarden.net/wiki/Range
[^8]: Currently only move `Haze` and its effect - `StatChangeResetEffect` that always effects both sides