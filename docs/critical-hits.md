# Critical Hit System


The `CriticalHitCalculator` determines  
whether a move results in a critical hit.

It follows Pokémon Stadium–style mechanics  
based on the attacker’s base Speed stat.[^1]

---

## Purpose

`CriticalHitCalculator.IsCriticalHit(BattleContext context, CriticalRatio ratio)`

Returns:

- `true`  → the move is a critical hit
- `false` → normal hit

The calculator:

- Uses the attacker’s base Speed
- Applies ratio-based threshold logic
- Performs a 0–255 RNG roll

It does not:

- Modify damage directly
- Apply stat overrides
- Log battle messages

It only determines whether the hit is critical.

---

## Critical Hit Resolution Flow

The calculation follows this order:

---

### 1. Base Speed

Critical chance is based on:

`Attacker.ActivePokemon.Species.BaseStats.Speed`

Important:

- Uses base Speed from species
- Does NOT use modified battle Speed
- Ignores stat stages
- Ignores temporary boosts

This mirrors classic mechanics.

---

### 2. Threshold Calculation

Base threshold:

`threshold = (speed + 76) / 4`

This converts Speed into
a 0–255 probability space.

Higher base Speed
increases critical hit chance.

---

### 3. Critical Ratio Handling

If the move has:

- Normal critical ratio  
  → threshold is capped at 255

- High critical ratio  
  → threshold is NOT capped

High critical ratio moves
naturally gain increased chance
through the same formula.

Ratio is controlled by:

`CriticalRatio` enum.

---

### 4. Random Roll

A random integer is generated:

`roll = Range.Next(256)`

This produces a value:
0 to 255 inclusive

The move is critical if:

`roll < threshold`

Otherwise, it is a normal hit.

---

## Important Notes

- The RNG source is `BattleContext.Range`,
  allowing deterministic replays with seeded RNG.
- Critical hit logic is separated
  from damage calculation.
- The damage multiplier (×2)
  is applied later inside `DamageCalculator`.
- Stat stage interaction
  (e.g., ignoring boosts on critical hits)
  is handled inside stat getter methods,
  not in this calculator.

---

## Design Principles

The critical hit system is:

- Stateless
- Deterministic (given RNG seed)
- Isolated from damage logic
- Based solely on species base stats

It answers one question:

**Is this hit critical?**

## Notes
[^1]: https://bulbapedia.bulbagarden.net/wiki/Critical_hit#Pok%C3%A9mon_Stadium