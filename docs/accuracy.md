# Accuracy System

## Accuracy Calculator

The `AccuracyCalculator` is responsible for determining  
whether a move successfully hits its target.

It follows Generation I–style accuracy mechanics  
based on a 0–255 internal scale.[^1]


---

## Purpose

`AccuracyCalculator.DoesMoveHit(BattleContext context)`

Returns:

- `true`  → the move hits
- `false` → the move misses

The calculator is:

- deterministic (given RNG state)
- stat-aware (accuracy and evasion stages)
- state-aware (invulnerability, fainted targets)

It does not log results.  
It only determines hit or miss.

---

## Hit Resolution Flow

The accuracy check follows this order:

### 1. Self-Targeting Moves

If the move target is `Self`:

- The move automatically hits.
- No accuracy calculation is performed.

---

### 2. Invalid Target States

If the defender:

- `IsInvulnerable == true`
- `IsFainted == true`

The move automatically misses.

This ensures:

- Dig / Fly immunity during semi-invulnerable turn
- No targeting of fainted Pokémon

---

### 3. Moves Without Accuracy

If `Move.Property.Accuracy` is `null`:

- The move always hits.

This covers:

- Swift-like moves
- Status moves that bypass accuracy
- Special mechanics

---

### 4. Base Accuracy Conversion

Move accuracy is defined as a percentage (e.g. 85%).

It is converted to the Gen I internal scale:

`accuracy = Accuracy% × 255 / 100`

Example:

100% → 255  
85%  → 216  

This converts the move into a 0–255 precision value.

---

### 5. Accuracy Stage Modifier (Attacker)

The value is modified by the attacker’s accuracy stage:

`accuracy = Attacker.ActivePokemon.GetAccuracy(accuracy)`

This applies stage multipliers such as:

- +1 Accuracy
- -2 Accuracy

Stage logic is encapsulated inside the [StatStages](stats.md).

---

### 6. Evasion Stage Modifier (Defender)

The value is then modified by the defender’s evasion stage:

`accuracy = Defender.ActivePokemon.GetEvasion(accuracy)`

Evasion reduces the final hit probability.

Accuracy and evasion stack multiplicatively
through stage calculations.

---

### 7. Clamping

Final accuracy is clamped:

1 ≤ accuracy ≤ 255

This ensures:

- No guaranteed miss (minimum 1)
- No overflow beyond maximum threshold

---

### 8. Random Roll

A random integer is generated:

`roll = Range.Next(256)`

This produces a value from:

0 to 255 inclusive

The move hits if:

`roll ≤ accuracy`

Otherwise, it misses.

---

## Important Notes

- The calculator does not log “It missed!”
  Logging must be handled by the battle engine layer.
- The RNG source comes from `BattleContext.Range`,
  ensuring deterministic battle replay when seeded.
- The system replicates the classic
  byte-based internal accuracy logic.
- Semi-invulnerability bypasses the entire check
  before accuracy stages are applied.

---

## Design Principles

The accuracy system is:

- Stateless
- Pure (aside from RNG usage)
- Encapsulated
- Independent from move effects

It does not:

- Modify battle state
- Apply damage
- Handle secondary effects

It only answers one question:

**Does the move hit?**

## Notes
[^1]: https://m.bulbapedia.bulbagarden.net/wiki/Accuracy#Accuracy_check


