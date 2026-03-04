# Type Effectiveness System

## Type Effectiveness Chart

The `TypeEffectivenessChart` determines  
how effective a move’s type is against the defender’s type(s).[^1]

It replicates Generation I type interaction rules  
using a multiplier-based system.[^2]

The chart is implemented as a static lookup table. 

---

## Purpose

The system provides two main operations:

`GetMultiplier(PokemonType attackType, IEnumerable<PokemonType> defenderTypes)`

Returns:

- A `double` multiplier representing total type effectiveness.

`GetLogMessage(double multiplier)`

Returns:

- A battle message based on effectiveness.
- `null` if effectiveness is neutral.

The chart does not:

- Apply damage
- Log automatically
- Modify battle state

It only calculates type interaction values.

---

## Internal Structure

The chart is defined as:

`Dictionary<PokemonType, Dictionary<PokemonType, double>>`

Each attacking type contains a row  
mapping defending types to multipliers.

Only non-neutral interactions are stored.

If an interaction is not defined:

Multiplier defaults to 1.0 (neutral).

---

## Multiplier Values

Possible multipliers:

- 2.0  → Super effective
- 0.5  → Not very effective
- 0.0  → No effect (immunity)
- 1.0  → Neutral

---

## Multiplier Calculation

### 1. Single-Type Defender

If the defender has one type:

`multiplier = GetSingleMultiplier(attackType, defenderType)`

If no entry exists in the chart:

`multiplier = 1.0`

---

### 2. Dual-Type Defender

If the defender has two types:

`multiplier = multiplier(type1) × multiplier(type2)`

#### Examples:
- Fire vs Grass/Poison: `2.0 × 1.0 = 2.0`
- Electric vs Water/Flying: `2.0 × 2.0 = 4.0`
- Normal vs Ghost/Poison: `0.0 × 1.0 = 0.0`

Multiplication ensures correct stacking behavior.

---

## Immunity Handling

If any type interaction returns 0.0:

Total multiplier becomes 0.0.

This represents full immunity.

Damage calculation should immediately return 0  
when multiplier equals 0.

---

## Logging Messages

GetLogMessage(double multiplier) returns:

- `0.0`     → `"It has no effect!"`
- `> 1.0`   → `"It's super effective!"`  
- `< 1.0`   → `"It's not very effective!"`  
- `1.0`     → `null`

This allows the battle engine or effects  
to log appropriate feedback conditionally.

The chart itself does not perform logging.

---

## Important Notes

- Neutral interactions are not stored explicitly.
- The chart is static and immutable.
- Multiplier stacking supports 4× and 0.25× results naturally.
- The system is independent from the damage formula.
- STAB is not handled here.
- Accuracy is not handled here.

This class is responsible only for type relationships.

---

## Design Principles

The type system is:

- Deterministic
- Data-driven
- Isolated from battle logic
- Independent from damage and accuracy systems

It answers two questions:

1. What is the total type multiplier?
2. What message corresponds to that multiplier?

## Notes

[^1]: https://bulbapedia.bulbagarden.net/wiki/Type#Type_effectiveness
[^2]: https://bulbapedia.bulbagarden.net/wiki/Type/Type_chart#Generation_I