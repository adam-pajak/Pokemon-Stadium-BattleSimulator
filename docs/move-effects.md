# Effects system

This document describes the **move effect system** used in the battle engine.

Move effects are modular components responsible for applying specific battle logic
such as dealing damage, changing stats, executing multiple hits, etc.

Instead of hardcoding every move individually, moves are built from reusable effects.

---

## 1. Overview

A move effect is a class implementing the interface:

- `IMoveEffect`

Each effect represents a single reusable piece of battle logic.

Examples:
- dealing damage
- applying stat stage changes
- executing multiple hits
- applying special move behavior

Effects are executed during move resolution inside the battle loop.

---

## 2. IMoveEffect.cs

### Purpose
`IMoveEffect` is the common interface implemented by all move effect classes.

It defines the minimal API required to apply an effect to the battle state.

### Responsibilities
All move effects should:
- modify the battle state (HP, stat stages, context, etc.)
- follow Gen 1 / Stadium mechanics rules
- remain reusable across multiple moves

### Notes
Effects should not be responsible for:
- reading JSON
- building moves from DTOs
- selecting targets
- trainer decision making

Effects only apply logic after the battle engine has determined
who attacks and who is targeted.

---

## 3. Common Execution Context

Every move effect is executed with access to objects such as:
- current battle context (`BattleContext`), including:
    - attacker active Pokémon (`BattlePokemon`)
    - defender active Pokémon (`BattlePokemon`)
- move information (`BattleMove` or `Move`)

This allows effects to:
- read current stat stages
- apply damage
- update battle flags
- log results

---

## 4. ChargingTurnEffect.cs

### Purpose
`ChargingTurnEffect` implements multi-turn move logic for moves that require a **charging turn**[^1]
(e.g. SolarBeam, Fly, Dig) or a **recharge turn**[^2] (Hyper Beam).

This effect is responsible for toggling charging states and preventing the attacker
from selecting a different action while the move is still being resolved.

---

### Key Mechanics Implemented

This effect supports two main mechanics:

#### 1) Charging moves (2-turn moves)
Moves such as:
- Dig
- Fly
- SolarBeam
- RazorWind
- SkullBash
- SkyAttack

These moves require:
- a **charging turn** (no damage dealt yet)
- an **execution turn** (damage dealt on the next turn)

The effect toggles:

- `ActivePokemon.IsCharging`

When charging begins:
- a battle log message is printed (e.g. *"Pikachu absorbed light!"*)
- `Trainer.LockChoice` is enabled to prevent changing move choice mid-charge

When charging ends:
- `Trainer.LockChoice` is disabled

---

#### 2) Recharge moves (Hyper Beam logic)
For `HyperBeam`, the effect implements the Gen 1 recharge mechanic:

- `ActivePokemon.IsRecharging`

If the Pokémon is recharging:
- a battle log message is printed (*"X must recharge!"*)
- the trainer is unlocked after the recharge turn

Otherwise:
- the trainer is locked until recharge is completed

This simulates the fact that the attacker cannot select a move during the recharge phase.

---

### Semi-invulnerable turn handling

The constructor takes a flag:

- `semiInvulnerableTurn`

If enabled, the effect toggles:

- `ActivePokemon.IsInvulnerable`

This is used for moves like:
- Dig
- Fly

During the semi-invulnerable phase the Pokémon becomes temporarily invulnerable.

---

### Logging

The effect prints a specific log message depending on move name:

| Move | Log message |
|------|------------|
| Dig | "burrowed its way under the ground!" |
| Fly | "flew up high!" |
| HyperBeam | "must recharge!" |
| RazorWind | "made a whirlwind!" |
| SkullBash | "lowered its head!" |
| SkyAttack | "is glowing!" |
| SolarBeam | "absorbed light!" |

These messages are consistent with Pokémon Stadium-style battle text.

---

### Summary

`ChargingTurnEffect` controls multi-turn move states by toggling:
- `IsCharging`
- `IsRecharging`
- `IsInvulnerable`

It also controls whether the attacker is forced to repeat the same move
using `Trainer.LockChoice`.

This effect does not deal damage itself — it only manages battle state
required for multi-turn move execution.

---

## 5. CopyMoveEffect.cs

### Purpose
`CopyMoveEffect` implements move-copying mechanics for Gen 1 style moves:

- **Mimic**[^4]
- **Mirror Move**[^5]

The behavior depends on the `_replace` flag passed through the constructor.

---

### 1. Mimic Behavior (`_replace = true`)

When `_replace` is enabled, the effect simulates **Mimic**.

#### Logic
- a random move is selected from the defender's current moveset
- the attacker searches for `"Mimic"` in its own moveset
- the copied move is inserted into the attacker’s moveset at Mimic’s index

#### Implementation details
- the copied move is taken directly from:
  - `context.Defender.ActivePokemon.Moves`
- the move is chosen randomly using:
  - `context.Range.Next(...)`
- `"Mimic"` is not permanently removed from memory, but is effectively replaced
  by inserting the copied move into the same slot

#### Output
The battle log prints:
> {pokemonName} learned!

---

### 2. Mirror Move Behavior (`_replace = false`)

When `_replace` is disabled, the effect simulates **Mirror Move**.

#### Logic
- if `context.LastMove` is null, the move fails
- otherwise, the attacker copies the last move used in battle
- Mirror Move cannot copy itself (prevents infinite recursion)

#### Failure conditions
Mirror Move fails if:
- there is no previously used move (`context.LastMove == null`)
- the last move is also `"MirrorMove"`

In both cases the battle log prints:
> But it failed!

#### Successful execution
If the last move exists and is not Mirror Move:
- the log prints:
> {pokemonName} copied {moveName}!
- then the last move is executed using:
`context.LastMove.Use(context);`

This means Mirror Move directly triggers full execution of the copied move,
including accuracy checks and all effects.

---

### Summary
`CopyMoveEffect` is a dual-purpose move effect implementing two separate mechanics:
- **Mimic**: modifies the attacker’s moveset during battle by copying a random defender move.
- **Mirror Move**: re-executes the last used move, with safety checks to avoid infinite recursion.
The effect depends heavily on battle state stored in `BattleContext`, especially:
- `context.LastMove`
- `attacker` and `defender` Pokémon move lists

---

## XX. Summary

Move effects are the core building blocks of move behavior.

Moves are not implemented as separate hardcoded classes.
Instead, each move is a combination of:
- data (power, type, accuracy)
- one or more effect objects

This design makes the battle engine easy to extend
while keeping mechanics modular and testable.

## Notes
[^1]: https://m.bulbapedia.bulbagarden.net/wiki/Category:Moves_with_a_charging_turn
[^2]: https://m.bulbapedia.bulbagarden.net/wiki/Category:Moves_that_require_recharging
[^3]: https://m.bulbapedia.bulbagarden.net/wiki/Category:Moves_with_a_semi-invulnerable_turn
[^4]: https://m.bulbapedia.bulbagarden.net/wiki/Mimic_(move)
[^5]: https://m.bulbapedia.bulbagarden.net/wiki/Mirror_Move_(move)
