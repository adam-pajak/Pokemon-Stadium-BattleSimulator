# Damage Calculation System


The `DamageCalculator` is responsible for computing  
the final damage dealt by a move.

It follows the classic Generation I damage formula  
using integer arithmetic and 0–255 scaling where applicable[^1].

---

## Purpose

`DamageCalculator.Calculate(BattleContext context, bool isCritical, double typeEffectiveness)`

Returns:

- A `byte` representing the final damage dealt.
- `0` if the move has no effect due to type immunity.

The calculator:

- Applies base formula
- Handles critical hits
- Applies STAB (Same Type Attack Bonus)
- Applies type effectiveness
- Applies random variance
- Guarantees minimum damage of 1 (unless immune)

It does not:

- Check accuracy
- Log output
- Apply damage to HP
- Handle recoil or secondary effects

It only computes the number.

---

## Damage Resolution Flow

The calculation follows this order:

---

### 1. Base Power

Power is retrieved from the move:

`power = Move.Property.Power ?? 0`

If the move has no power (e.g. status move),  
damage defaults to 0.

---

### 2. Level

Attacker level is used directly:

`level = Attacker.ActivePokemon.Level`

---

### 3. Critical Hit Modifier

If `isCritical == true`:

`critical = 2`  
Otherwise:

`critical = 1`

This multiplier affects the level term in the formula.

Stat stage handling for critical hits is delegated
to stat getter methods.

---

### 4. Offensive and Defensive Stats

Depending on move category:

If Physical:
- Use Attack
- Use Defense

If Special:
- Use Special Attack
- Use Special Defense

Critical-aware stat retrieval:

- `GetAttack(isCritical)`
- `GetSpAttack(isCritical)`
- `GetDefense(isCritical)`
- `GetSpDefense(isCritical)`

This allows critical hits to ignore stat stage changes.

---

### 5. Gen I Stat Overflow Rule

If either:

`attack > 255`  
or  
`defense > 255`

Then both values are divided by 4.

This replicates the original Generation I overflow behavior.

---

### 6. Base Formula

The core damage formula:

`damage = (2 × level × critical / 5 + 2) × power × attack / defense / 50 + 2`

All calculations use integer arithmetic.

---

### 7. STAB (Same-Type Attack Bonus)

If the attacker’s types contain the move’s type:

damage = damage × 3 / 2

This applies a 1.5× multiplier.

Type comparison uses the mutable active Pokémon types,
not species base types.

---

### 8. Type Effectiveness

If:

typeEffectiveness == 0

Return 0 immediately.

Otherwise:

`damage = damage × typeEffectiveness`

This supports:

- 0× (immunity)
- 0.5× (not very effective)
- 2× (super effective)
- 4× (double weakness)

Type effectiveness is passed in externally,
not calculated inside the damage system.

---

### 9. Random Modifier

A random value is generated:

`randomModifier = Range.Next(217, 256)`

This produces a value from:
217 to 255 inclusive

Final scaling:

`damage = damage × randomModifier / 255`

This results in approximately:
85% – 100% damage range.

---

### 10. Minimum Damage Rule

If:
`damage < 1`

Set:
`damage = 1`

This ensures at least 1 damage
for non-immune damaging moves.

---

### 11. Final Return

Damage is returned as:

`(byte)damage`

This simulates damage overflow that can occur in Gen 1 Pokémon games.

---

## Important Notes

- The calculator is deterministic except for RNG.
- RNG comes from `BattleContext.Range`
  enabling seeded battle replays.
- No floating-point math is used except
  for applying type effectiveness.
- All battle logging must occur outside this class.

---

## Design Principles

The damage system is:

- Pure (no state mutation)
- Stateless
- Isolated from effects
- Independent from accuracy
- Focused solely on numerical output

It answers a single question:

**How much damage should this move deal?**

## Notes
[^1]: https://bulbapedia.bulbagarden.net/wiki/Damage#Damage_calculation