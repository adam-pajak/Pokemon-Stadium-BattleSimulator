# move-effects.md

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
- attacker (`BattlePokemon`)
- defender (`BattlePokemon`)
- current battle context (`BattleContext`)
- move information (`BattleMove` or `Move`)

This allows effects to:
- read current stat stages
- apply damage
- update battle flags (if implemented)
- log results

---


## XX. Summary

Move effects are the core building blocks of move behavior.

Moves are not implemented as separate hardcoded classes.
Instead, each move is a combination of:
- data (power, type, accuracy)
- one or more effect objects

This design makes the battle engine easy to extend
while keeping mechanics modular and testable.
