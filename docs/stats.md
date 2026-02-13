# Stats System

This document describes the stats system located in: `Battle/Stats/`


The stats system is responsible for handling:
- base stats and calculated battle stats
- stat stage modifiers (Attack up/down, etc.)
- stat stage multipliers based on Gen 1 mechanics
- safe arithmetic using fractions

---

## 1. Overview

In Pokémon battles, stats are not constant values.
They can be modified during battle using stat-changing moves such as:
- Growl (Attack down)
- Swords Dance (Attack up)
- Tail Whip (Defense down)

To support this, the project separates stats into two layers:

- **base/calculated stats** (HP, Attack, Defense, Speed, Special Attack, Special Defense)[^1]
- **stat stages** (temporary multipliers applied on top of stats)[^2]

---

## 2. BattleStats.cs

### Purpose
`BattleStats` stores the Pokémon's calculated battle stats that are treated as **constant**
during the battle.

These stats are calculated at the start of the battle based on:
- Pokémon base stats
- Pokémon level
- additional formulas such as IV/EV-like values[^3]

### Responsibilities
- storing the base battle stats (Attack, Defense, Speed, Special Attack, Special Defense, etc.)
- acting as the immutable "starting point" for all further calculations

### Important note
`BattleStats` does **not** change during battle when stat-changing moves are used.

Stat-changing moves modify `StatStages`, which are applied dynamically
to produce the effective stats used in calculations.

---

## 3. StatStages.cs

### Purpose
`StatStages` stores the current stage modifiers for each stat.

Stat stages are integer values, in range:

- **-6 to +6**

### Responsibilities
- storing stage values for each stat (as a Dictionary)
- allowing stage modification (increase/decrease)
- clamping stage values to valid range

### Example
If a Pokémon uses Swords Dance:
- Attack stage increases sharply (+2 stages)

If a Pokémon uses Growl:
- Attack stage decreases (-1 stage)

---

## 4. StatStageMultipliers.cs

### Purpose
`StatStageMultipliers` contains the multiplier values used for each stage.

Example:
- stage +1 increases a stat by a certain ratio
- stage -1 decreases a stat by a certain ratio

This file acts as a lookup table for converting stages into multipliers.[^4]

### Why this is needed
Stat stages should not be applied using floating-point math,
because it may cause rounding errors.

Instead, multipliers are often stored as rational numbers (fractions).

---

## 5. Fraction.cs

### Purpose
`Fraction` represents a rational number (`numerator/denominator`).

This is used to avoid floating-point inaccuracies when applying multipliers.

### Responsibilities
- storing numerator and denominator
- supporting multiplication and division
- allowing conversion to integer results

### Example usage
A multiplier might be stored as:

- `3/2` instead of `1.5`

This ensures calculations remain consistent and deterministic (just like in the original game).

---
## 6. StatCalculator.cs

### Purpose
`StatCalculator` is responsible for calculating a Pokémon's **base battle stats**
(`BattleStats`) from its base stats and level.

This calculation is performed once when a Pokémon is initialized for battle.

### Responsibilities
- calculating battle stats from:
    - Pokémon base stats
    - Pokémon level
- producing the `BattleStats` object used as a constant reference during the battle

### When it is used
`StatCalculator` is used:
- during Pokémon initialization
- when creating `BattlePokemon`

It is **not** used for stat stage changes (Growl, Swords Dance, etc.).
Those are handled by `StatStages` + `StatStageMultipliers`.

---

## 7. How Stat Changes Are Applied in Battle

Stat changes during battle do not modify `BattleStats`.

Instead, they modify the Pokémon's `StatStages`.

Flow:
1. A move effect modifies a stat stage (example: Attack stage +2)
2. When a calculator needs a stat value, it computes the effective stat:

`effectiveStat = BattleStats.Stat * StageMultiplier(stage)`

This means:
- `BattleStats` is the constant base value
- `StatStages` provides temporary modifiers
- `StatStageMultipliers` defines the multiplier values
- the final effective stat is calculated dynamically when needed
---

## 8. Interaction With Damage and Accuracy Calculators

The stats system is heavily used by battle calculators.

### DamageCalculator.cs
Uses:
- Attack and Defense stat (physical attacks)
- Sp.Attack and Sp.Defense stat (special attacks)

### AccuracyCalculator.cs
Uses:
- Accuracy stage
- Evasion stage

Accuracy and Evasion do not have base values so they are always equal to `moveAccuracy * StageMultiplier(stage)`

---

## 9. Key Relationships Diagram
## Stats Flow Diagram

```
                  ┌─────────────────────┐
                  │   BaseStats.cs      │
                  │ (species base stats)│
                  └─────────┬───────────┘
                            │
                            │ + Level
                            ▼
                  ┌─────────────────────┐
                  │  StatCalculator.cs  │
                  │ calculates stats    │
                  └─────────┬───────────┘
                            │
                            ▼
                  ┌─────────────────────┐
                  │   BattleStats.cs    │
                  │ (constant in battle)│
                  └─────────┬───────────┘
                            │
                            │ used together with
                            │
                            ▼
                  ┌─────────────────────┐
                  │    StatStages.cs    │
                  │ (changes in battle) │
                  └─────────┬───────────┘
                            │
                            ▼
                  ┌──────────────────────────┐
                  │ StatStageMultipliers.cs  │
                  │  stage → Fraction        │
                  └─────────┬────────────────┘
                            │
                            ▼
                  ┌──────────────────────────┐
                  │ Effective Stat Value     │
                  │ used in battle formulas  │
                  │ (damage, accuracy, etc.) │
                  └──────────────────────────┘
```
Formula:
`effectiveStat = BattleStats.Stat * StageMultiplier(stage)`


## 10. Summary

The stats system is built around two layers:

### Constant stats
- `BattleStats` contains base battle stats calculated once at battle start.
- `StatCalculator` calculates these stats using base stats and Pokémon level.

### Temporary battle modifiers
- `StatStages` stores stat stage changes caused by moves.
- `StatStageMultipliers` defines multipliers for each stage using `Fraction`.

During calculations, the engine uses:

`effectiveStat = baseBattleStat * stageMultiplier`

This approach keeps battle logic modular and avoids permanently modifying base stats.


[^1]: Note that in gen 1 only there was a Special stat instead of Sp.Attack and Sp.Defense. However, due to problems with access to data of this stat,
this project does not implement it - reference: https://bulbapedia.bulbagarden.net/wiki/Stat#List_of_stats
[^2]: https://bulbapedia.bulbagarden.net/wiki/Stat_modifier
[^3]: Not implemented - reference (see Determination of stats): https://bulbapedia.bulbagarden.net/wiki/Stat#Determination_of_stats
[^4]: Tables: https://bulbapedia.bulbagarden.net/wiki/Stat_modifier#Stage_multipliers