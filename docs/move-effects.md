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
    - move information (`BattleMove`)

This allows effects to:
- read current stat stages
- apply damage
- update battle flags
- log results

---

## 4. Implemented effects

### ChargingTurnEffect.cs

#### Purpose
`ChargingTurnEffect` handles multi-turn move behavior.

It represents moves such as:
- Dig
- Fly
- Razor Wind
- Skull Bash
- Sky Attack
- Solar Beam
- Hyper Beam (recharge mechanic)

The effect manages charging turns[^1],
semi-invulnerability[^2],
and recharge states[^3].

---

#### Constructor

`ChargingTurnEffect(bool semiInvulnerableTurn)`

Parameter:

- `semiInvulnerableTurn`
  - `true`  → move grants temporary invulnerability
  - `false` → move only charges without invulnerability

Examples:
- Dig / Fly → `true`
- Solar Beam → `false`

---

#### Core Mechanic

When `Apply(BattleContext context)` is called:

---

1) Semi-Invulnerability Toggle

If `_semiInvulnerableTurn == true`:

`IsInvulnerable` is toggled.

This allows moves like:
- Dig (underground)
- Fly (in the air)

to grant temporary protection.

---

2) (Hyper Beam) Recharge Logic

If move name is `"HyperBeam"`:

- If `IsRecharging == true`:
  - Log recharge message
  - Unlock move choice
- Otherwise:
  - Lock move choice

Then toggle:

`IsRecharging = !IsRecharging`

This enforces the mandatory recharge turn.

---

3) Standard Charging Logic

For other charging moves:

- Toggle:

  `IsCharging = !IsCharging`

- If entering charging state:
  - Log charge message
  - Lock move choice

- If leaving charging state:
  - Unlock move choice

---

#### Logging

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

---

#### Important Notes

- This effect manages state flags only.
- It does not calculate damage.
- It does not apply stat changes.
- It relies on battle flow logic
  to determine whether damage should occur
  during the charging or execution turn.
- `LockChoice` prevents the player
  from selecting another move mid-sequence.
- Invulnerability is state-based,
  not tied directly to damage resolution.

---

#### Architectural Role

`ChargingTurnEffect` is a state-machine effect.

It:

- mutates temporary Pokémon flags
  (`IsCharging`, `IsRecharging`, `IsInvulnerable`)
- controls move selection locking
- governs multi-turn behavior

It demonstrates that the move system supports:

- persistent multi-turn state
- temporary invulnerability
- forced recharge cycles
- branching execution paths

This is one of the most structurally complex
state-management effects in the engine.

---

### CopyMoveEffect.cs

#### Purpose
`CopyMoveEffect` allows the attacker
to copy and use another move.

It represents moves such as:
- Mimic[^4]
- Mirror Move[^5]

The exact behavior depends on constructor configuration.

---

#### Constructor

`CopyMoveEffect(bool replace)`

Parameter:

- `replace`
  - `true`  → Mimic behavior
  - `false` → Mirror Move behavior

This flag determines whether the move
permanently replaces a slot
or temporarily executes the copied move.

---

#### Core Mechanic

When `Apply(BattleContext context)` is called:

Mode 1 — Mimic (`replace == true`)

1) Randomly select one of the defender’s moves:

   `context.Defender.ActivePokemon.Moves[context.Range.Next(...)]`

2) Find the index of "Mimic"
   in the attacker’s move list.

3) Replace that move with the selected move.

4) Log:

   > `<Attacker>` learned `<Move>`!

The copied move becomes part of the attacker’s move set.

---

Mode 2 — Mirror Move (`replace == false`)

1) If `context.LastMove` is null:
   - Log:
     > But it failed!
   - Exit.

2) Log:
   > `<Attacker>` copied `<Move>`!

3) If the last move was "MirrorMove":
   - Log:
     > But it failed!
   - Exit.
   (Prevents infinite recursion.)

4) Execute the copied move:

   `context.LastMove.Use(context)`

The move is executed immediately
without modifying the attacker’s move list.

---

#### Important Notes

- Mimic modifies the attacker’s move set.
- Mirror Move does not modify move slots.
- Infinite recursion is prevented
  by explicitly checking for "MirrorMove".
- Mirror Move depends on `context.LastMove`.
- The copied move fully controls:
  - damage
  - status effects
  - logging
  - secondary effects

---

 ##### Architectural Role

`CopyMoveEffect` is a delegation-based effect
with optional state mutation.

It demonstrates two distinct behaviors:

- **Permanent mutation** (Mimic)
- **Dynamic delegation** (Mirror Move)

The effect:

- interacts with move collections
- may mutate battle state
- may trigger nested move execution
- depends on previously recorded battle data

It showcases the flexibility of the effect system,
allowing complex Gen 1 mechanics
without special-case logic in the battle engine core.

---

### CounterDamageEffect.cs

#### Purpose
`CounterDamageEffect` reflects previously received damage
back to the opponent, dealing double the amount taken.

It represents moves such as:
- Counter[^6]
- Mirror Coat[^7]

The effect succeeds only if specific conditions are met.

---

#### Constructor

`CounterDamageEffect(MoveCategory category)`

Parameter:

- `category` → the move category that can be countered
  (e.g. Physical)

This allows the effect to restrict which move types
are eligible for reflection.

---

#### Core Mechanic

When `Apply(BattleContext context)` is called:

1) Validate conditions:

   The effect fails if:
   - `context.LastDamage` is null
   - `context.LastMove` is null
   - `context.LastMove.Property.Category`
     does not match `_category`

   If any condition fails:
   - Log:
     "But it failed!"
   - Exit.

2) Log:

   > <Attacker> countered <LastMoveName>!

3) Calculate reflected damage:

   `reflectedDamage = LastDamage * 2`

4) Apply damage to defender:

   `TakeDamage(reflectedDamage)`

5) Log:

   > <Defender> received <damage>[^8] damage!

6) If defender faints:
   - Log:
     > <Defender> fainted!

---

#### Important Notes

- Damage is based on actual HP lost (`LastDamage`),
  not on recalculating the damage formula.
- The effect depends entirely on previously recorded
  battle state.
- It does not calculate type effectiveness.
- It does not roll for critical hits.
- If the previous move does not match the required category,
  the effect fails.

---

#### Architectural Role

`CounterDamageEffect` is a reactive effect.

It:

- depends on battle history (`LastMove`, `LastDamage`)
- enforces category-based validation
- performs deterministic damage reflection
- bypasses `DamageCalculator`

This effect demonstrates that:

- the engine supports history-dependent mechanics
- damage can be derived from recorded outcomes
- reactive logic can be implemented cleanly
  without modifying core battle flow

It is one of the most state-sensitive
effects in the Gen 1 move system.

---

### CutHalfHpEffect.cs

#### Purpose
`CutHalfHpEffect` reduces the defender’s current HP
by half.

It represents moves such as:
- Super Fang

Instead of using the damage formula,
the effect removes 50% of the target’s
current HP at the time of execution.

---

#### Core Mechanic

When `Apply(BattleContext context)` is called:

1) Calculate type effectiveness:

   `TypeEffectivenessChart.GetMultiplier(...)`

2) If an effectiveness message exists
   (e.g. "It's super effective!" or immunity message),
   log it.

3) If effectiveness equals 0:
   - Exit immediately.
   - No damage is applied.

4) Otherwise:

   - Retrieve defender’s current HP:

     `hp = CurrentHp`

   - Calculate half:

     `damage = hp / 2`

   - Apply damage:

     `TakeDamage(damage)`

   - Log:

     `<Defender> received <X> damage.`

---

#### Important Notes

- Damage is based on current HP,
  not max HP.
- The effect ignores:
  - Attack stat
  - Defense stat
  - critical hits
- Type immunity prevents the effect entirely.
- Damage scales dynamically
  as the defender’s HP decreases.
- `LastDamage` is not modified explicitly
  (unless handled elsewhere in engine flow).

---

#### Architectural Role

`CutHalfHpEffect` is a deterministic
HP-scaling damage effect.

It:

- bypasses `DamageCalculator`
- still respects type effectiveness
- modifies defender HP directly

This effect demonstrates that:

- not all damage must rely on battle stats
- special-case damage logic can remain isolated
- the effect system supports formula-independent mechanics

It fills the niche between:

- `FixedDamageEffect` (constant value)
- `DamageEffect` (stat-based formula)
- percentage-based HP manipulation effects

---

### DamageEffect.cs

#### Purpose
`DamageEffect` is the core offensive effect responsible for dealing standard damage
based on Gen 1 / Stadium-style battle mechanics.

It integrates:
- type effectiveness
- critical hit logic
- full damage formula calculation
- HP reduction
- battle logging
- updating battle history (`LastDamage`)

This is the primary effect used by most attacking moves.

---

#### Constructor

`DamageEffect(CriticalRatio criticalRatio)`

The effect accepts a `CriticalRatio` parameter which determines
the critical hit chance category for the move.

This allows support for:
- normal critical rate moves
- high critical rate moves (e.g. Slash)
- potentially custom ratios

---

#### Execution Flow

When `Apply(BattleContext context)` is called:

1) Type Effectiveness  
The effect calculates type effectiveness using `TypeEffectivenessChart.GetMultiplier(...)`.

If a log message is available (e.g. "It's super effective!"),
it is printed before damage is applied.

2) Critical Hit Check  
Critical hit determination is delegated to `CriticalHitCalculator.IsCriticalHit(...)`.

If the hit is critical, the effect logs:
"Critical hit!"

The result (true/false) is passed to the damage calculator.

3) Damage Calculation  
Final damage is calculated using:

`DamageCalculator.Calculate(context, isCritical, effectiveness)`

The calculator is responsible for:
- applying Attack/Defense or Special/Special
- applying stat stage multipliers
- applying critical hit logic
- applying type multiplier
- applying random damage variation
- applying Gen 1 rounding rules

`DamageEffect` itself does not implement the formula —
it only coordinates the process.

4) Applying Damage  
Damage is applied using:

`context.Defender.ActivePokemon.TakeDamage(damage)`

If damage is greater than 0, the effect logs:

> <Defender> received <damage>[^8] damage!

If the defender faints, it logs:

> <Defender> fainted!

5) Updating Battle History  
After damage is applied:

`context.LastDamage = damageTaken;`

This allows reactive effects such as:
- CounterDamageEffect
- other reflection mechanics

to reference the most recent damage value.

---

#### Responsibilities Summary

`DamageEffect` is responsible for:
- triggering type effectiveness logic
- triggering critical hit logic
- delegating full damage calculation
- applying HP reduction
- logging battle messages
- updating `BattleContext.LastDamage`

It does NOT:
- implement the damage formula directly
- modify stat stages
- manage multi-turn behavior

---

#### Architectural Role

`DamageEffect` acts as a coordinator between:
- `TypeEffectivenessChart`
- `CriticalHitCalculator`
- `DamageCalculator`
- `BattlePokemon.TakeDamage`

It is the central bridge between move execution and the battle calculation system.

Most offensive moves in the engine rely on this effect.

---

### DisableEffect.cs

#### Purpose
`DisableEffect` implements the Gen 1-style Disable mechanic used for "Disable"[^10]

It prevents a specific move from being used for a limited number of turns,
based on the last move used in battle.

---

#### Core Mechanic

When `Apply(BattleContext context)` is called:

1) The effect checks `context.LastMove`.

If there is no previously used move:
- the move fails
- the battle log prints:
> But it failed!

2) If a last move exists:

- If the move is not already disabled:
  - a random duration is generated using:
    `context.Range.Next(0, 7)`
  - the move is disabled using:
    `lastMove.Disable(duration)`
  - the battle log prints:
    > <Attacker>'s <MoveName> was disabled!

- If the move is already disabled:
  - the battle log prints:
    > <Attacker>'s <MoveName> is already disabled!

---

#### Important Notes

- Disable targets the last move used in battle.
- The disabled state is stored inside the move object itself
  (e.g., `IsDisabled` flag and internal duration counter).
- The effect does not directly manage turn countdown —
  that logic is handled in `BattleContext`
- Duration is randomly determined (from 0 to 6)

---

#### Architectural Role

`DisableEffect`:
- depends on `BattleContext.LastMove`
- modifies the internal state of a move
- introduces temporary move-level restrictions

This effect demonstrates how the engine supports temporary move state changes
without modifying Pokémon core stats.

---

### DrainEffect.cs

#### Purpose
`DrainEffect` implements HP-draining mechanics for moves that heal the attacker
based on damage dealt.[^11]

This represents moves such as:
- Absorb
- Mega Drain
- Giga Drain
- Leech Life

The amount healed depends on a percentage of the damage inflicted.

---

#### Constructor

`DrainEffect(int percent)`

The `_percent` parameter determines how much of the dealt damage
is converted into HP restoration.

Example:
- 50 → heals 50% of damage dealt

---

#### Core Mechanic

When `Apply(BattleContext context)` is called:

1) If the attacker is already at full HP:
- the effect logs:
> <Attacker> hp is full!
- the effect ends immediately

2) If `context.LastDamage` is null:
- the effect ends (no damage was recorded)

3) If the recorded damage is less than or equal to 0:
- the effect ends (nothing to drain)

4) Healing amount is calculated as:

`heal = damageDealt * percent / 100`

5) If the calculated heal value is greater than 0:
- HP is restored using:
  `ActivePokemon.RestoreHp(heal)`
- the battle log prints:
> <Attacker> drained <restoredHp>[^12] energy!

---

#### Important Notes

- The effect depends on `context.LastDamage`,
  meaning it must be executed after a successful `DamageEffect`.
- Healing cannot exceed max HP (handled internally by `RestoreHp`).
- If the attacker is already at full HP, no healing occurs.
- If no damage was dealt (e.g. immunity = 0 damage),
  no healing occurs.

---

#### Architectural Role

`DrainEffect`:
- reads battle history (`LastDamage`)
- modifies attacker HP
- depends on correct execution order (must follow damage effect)
- does not calculate damage itself

This effect demonstrates how multiple effects can be chained:
first `DamageEffect`, then `DrainEffect`,
creating composite move behavior.

---

### FixedDamageEffect.cs

#### Purpose
`FixedDamageEffect` implements fixed-value damage moves.[^13]

Unlike standard attacks, this effect ignores:
- Attack and Defense stats
- stat stages
- critical hits
- type effectiveness multipliers

Damage is determined either by:
- a predefined constant value
- the attacker’s level

This represents moves such as:
- Seismic Toss (damage = attacker level)
- Night Shade (damage = attacker level)
- Dragon Rage (fixed 40 damage)
- Sonic Boom (fixed 20 damage)

---

#### Constructor

`FixedDamageEffect(int? fixedPower)`

- If `_fixedPower` is null:
  - damage equals attacker’s level
- If `_fixedPower` has a value:
  - that value is used as the damage amount

This allows support for both level-based and constant-damage moves.

---

#### Core Mechanic

When `Apply(BattleContext context)` is called:

1) Determine damage value:

- If `_fixedPower` is null:
  `damage = attacker.Level`
- Otherwise:
  `damage = fixedPower`

2) Apply damage directly using:

`context.Defender.ActivePokemon.TakeDamage(damage)`

3) If damage is greater than 0, log:

> <Defender> received <damage>[^8] damage!

4) If the defender faints, log:

> <Defender> fainted.

5) Store the damage value in:

`context.LastDamage`

This allows reactive effects (e.g. Counter) to access the damage dealt.

---

#### Important Notes

- This effect bypasses the standard damage formula entirely.
- It does not check type effectiveness.
- It does not apply critical hit logic.
- Damage is applied directly and deterministically.

---

#### Architectural Role

`FixedDamageEffect` is a simplified alternative to `DamageEffect`.

It:
- directly determines damage amount
- applies HP reduction
- updates `BattleContext.LastDamage`

It does NOT:
- use `DamageCalculator`
- depend on stat stages
- use effectiveness or crit logic

This effect demonstrates how the move system supports
non-formula-based damage mechanics in a clean and isolated way.

---

### MultistrikeEffect.cs

#### Purpose
`MultistrikeEffect` implements multi-hit attack mechanics.[^14]

It represents moves that hit multiple times in a single turn, such as:
- Fury Attack
- Comet Punch
- Pin Missile
- Bonemerang

The number of hits may be:
- fixed range (e.g. 2–5 hits)
- custom range (defined via constructor)

Damage, crit and type effectiveness are calculated once
and reused for each hit.

---

#### Constructor

`MultistrikeEffect(CriticalRatio criticalRatio, int minHits, int maxHits)`

Parameters:

- `criticalRatio` → controls critical hit chance
- `minHits` → minimum number of hits
- `maxHits` → maximum number of hits

If the range is 2–5, Gen 1 weighted probabilities are used.

---

#### Hit Count Logic

If `_minHits == 2` and `_maxHits == 5`,
Gen 1 distribution is applied:

- 2 hits → 37.5%
- 3 hits → 37.5%
- 4 hits → 12.5%
- 5 hits → 12.5%

Implemented via:

`RollHits(Random range)`

Otherwise:
a uniform random value between `minHits` and `maxHits`
is selected; note that if they are the same, the number will be equal to them.

---

#### Core Mechanic

When `Apply(BattleContext context)` is called:

1) Type effectiveness is calculated once:
   `TypeEffectivenessChart.GetMultiplier(...)`

2) Critical hit is rolled once:
   `CriticalHitCalculator.IsCriticalHit(...)`

3) Number of hits is determined.

4) For each hit:
   - Damage is calculated using:
     `DamageCalculator.Calculate(...)`

   - Damage is applied via:
     `TakeDamage(...)`

   - If damage > 0:
     log - 
     > <Defender> received <damage>[^8] damage!

   - `context.LastDamage` is updated

   - If defender faints:
     - log faint message
     - stop further hits

5) After the loop (if at least one hit occurred):

   - Type effectiveness message is logged
   - Critical hit message is logged (if applicable)
   - Final message:
     > It hit <h> time(s)!

---

#### Important Notes

- Crit and effectiveness are determined once per move,
  not per individual hit (Gen 1 behavior).
- Damage formula is executed separately for each hit.
- If the defender faints mid-sequence,
  remaining hits are skipped.
- `LastDamage` reflects the damage of the most recent hit.

---

#### Architectural Role

`MultistrikeEffect` is a composite offensive effect that:

- uses `DamageCalculator`
- uses `CriticalHitCalculator`
- uses `TypeEffectivenessChart`
- performs multiple sequential HP reductions
- updates `BattleContext.LastDamage`

It demonstrates that:
- complex mechanics can still be isolated inside a single effect
- RNG logic can be encapsulated cleanly
- Gen 1 probability quirks can be preserved without polluting other systems

This effect is one of the most mechanically complex
standard damage variants in the engine.

---

### OneHitKoEffect.cs

#### Purpose
`OneHitKoEffect` implements one-hit knockout mechanics.[^15]

It represents OHKO moves such as:
- Fissure
- Guillotine
- Horn Drill

If the move succeeds and the target is not immune,
the defender faints immediately regardless of HP.

---

#### Core Mechanic

When `Apply(BattleContext context)` is called:

1) Type effectiveness is calculated:

   `TypeEffectivenessChart.GetMultiplier(...)`

2) If an effectiveness log message exists
   (e.g. "It's super effective!" or "It has no effect!"),
   it is printed.

3) If effectiveness equals 0:
   - the effect ends immediately
   - the target is immune
   - no KO occurs

4) Otherwise:

   - `ActivePokemon.Knockout()` is called
   - log:
     `It's One-Hit KO!`
   - log:
     > <Defender> fainted!

---

#### Important Notes

- This effect ignores:
  - current HP
  - stats
  - damage formula
  - critical hits
- If the move’s type has no effect on the target,
  the KO does not occur.
- No damage value is calculated.
- `context.LastDamage` is not modified.

---

#### Architectural Role

`OneHitKoEffect` is a terminal effect:

- it does not calculate damage
- it directly forces fainting
- it bypasses standard combat math

It demonstrates that:

- the effect system supports non-numeric resolution
- fatal outcomes can be implemented cleanly
- special-case mechanics do not pollute `DamageCalculator`

This keeps the battle engine modular and predictable.

---

### RandomMoveEffect.cs

#### Purpose
`RandomMoveEffect` executes a randomly selected move
from the global move pool.

It exclusively represents a move:
- Metronome[^16]

Instead of performing its own battle logic,
this effect selects another move and executes it.

---

#### Core Mechanic

When `Apply(BattleContext context)` is called:

1) Retrieve all available moves from:

   `context.AllMoves`

2) Filter out moves that should not be selectable:

   - Currently, the move "Metronome" itself is excluded
     to prevent infinite recursion.

3) Select a random move:

   `index = context.Range.Next(moves.Count)`

4) Create a new `BattleMove` instance
   from the selected move definition.

5) Execute it immediately:

   `randomMove.Use(context)`

---

#### Important Notes

- The effect does not calculate damage directly.
- It does not modify `context.LastDamage` itself.
- The executed move fully controls its own effects.
- Infinite recursion is prevented
  by excluding "Metronome" from selection.

---

#### Architectural Role

`RandomMoveEffect` is a delegation-based effect.

It:

- selects another move definition dynamically
- instantiates a new `BattleMove`
- transfers control to that move’s execution logic

This demonstrates that:

- effects can trigger full move execution chains
- the battle system supports nested move resolution
- complex behaviors can be implemented without special-case engine code

It is one of the most powerful examples
of the modular effect architecture.

---

### RecoilEffect.cs

#### Purpose
`RecoilEffect` implements recoil damage mechanics.[^17]

It represents moves that damage the attacker
based on the damage dealt to the defender.

Examples:
- Take Down
- Double-Edge
- Submission

The recoil amount is calculated
as a percentage of the damage inflicted.

---

#### Constructor

`RecoilEffect(int percent)`

The `_percent` parameter determines
what percentage of the dealt damage
is returned to the attacker as recoil.

Example:
- 25 → attacker takes 25% of damage dealt
- 33 → attacker takes 33% of damage dealt

---

#### Core Mechanic

When `Apply(BattleContext context)` is called:

1) If `context.LastDamage` is null:
   - the effect ends (no recorded damage)

2) If recorded damage ≤ 0:
   - the effect ends (nothing to recoil from)

3) Recoil amount is calculated:

   `recoil = damageDealt * percent / 100`

4) If recoil > 0:

   - Apply damage to attacker via:
     `TakeDamage(recoil)`

   - Log:
     > <Attacker>'s hit with <recoil>[^12] recoil!

5) If the attacker faints:
   - Log:
     > <Attacker> fainted.

---

#### Important Notes

- The effect depends on `context.LastDamage`,
  meaning it must execute after `DamageEffect`.
- Recoil is based on actual damage dealt,
  not on calculated raw damage.
- If the move dealt no damage
  (miss, immunity, 0 damage),
  recoil does not occur.
- Recoil can cause the attacker to faint.

---

#### Architectural Role

`RecoilEffect`:
- reads battle state (`LastDamage`)
- modifies attacker HP
- can cause self-KO
- does not calculate damage itself

It is conceptually the inverse of `DrainEffect`,
demonstrating how symmetric mechanics
can be implemented using small, modular effects.

This reinforces the composable design
of the move-effect system.

---

### RecoverHpEffect.cs

#### Purpose
`RecoverHpEffect` restores a percentage
of the attacker’s maximum HP.[^18]

It represents moves such as:
- Recover
- Soft-Boiled
- (partially) Rest[^19]

Unlike `DrainEffect`, this healing
is not based on damage dealt.

---

#### Constructor

`RecoverHpEffect(int percent)`

The `_percent` parameter determines
what percentage of max HP will be restored.

Example:
- 50 → restore 50% of max HP
- 100 → restore full HP

---

#### Core Mechanic

When `Apply(BattleContext context)` is called:

1) If the attacker is already at full HP:

   - Log:
     > <Attacker> hp is full.
   - End effect.

2) Otherwise:

   - Calculate healing amount:

     `heal = MaxHp * percent / 100`

   - Restore HP via:
     `RestoreHp(heal)`

   - Log:
     > <Attacker> recovered <restoredHp> hp.

---

#### Important Notes

- Healing is based on max HP,
  not current HP.
- Actual restored amount may be lower
  than calculated heal (due to HP cap).
- The effect does not depend on `LastDamage`.
- It does not interact with stat stages.
- It does not check for battle conditions
  (e.g. status removal — handled elsewhere if needed).

---

#### Architectural Role

`RecoverHpEffect` is a direct state-modifying effect.

It:

- reads attacker’s max HP
- restores HP deterministically
- logs the result
- does not depend on other effects

It contrasts with:
- `DrainEffect` (damage-dependent healing)
- `RecoilEffect` (damage-based self-damage)

Together they show three distinct HP modification patterns:
- fixed percentage of max HP
- percentage of dealt damage
- percentage of received damage

---

### SelfdestructEffect.cs

#### Purpose
`SelfdestructEffect` forces the attacker to faint
immediately after using the move.

It represents moves such as:
- Selfdestruct
- Explosion

The attacker sacrifices itself as part of the move resolution.

---

#### Core Mechanic

When `Apply(BattleContext context)` is called:

1) The attacker’s active Pokémon is knocked out:

   `ActivePokemon.Knockout()`

2) Log:
   > <Attacker> fainted!

---

#### Important Notes

- This effect does not calculate damage.
- It does not interact with `LastDamage`.
- It does not check HP thresholds.
- It directly forces fainting regardless of current HP.
- It always occurs even if the move misses.

---

#### Architectural Role

`SelfdestructEffect` is a terminal self-targeting effect.

It:

- modifies only the attacker
- bypasses HP calculation
- produces guaranteed self-KO

This effect demonstrates that:

- move effects can apply to either attacker or defender
- fatal outcomes can be implemented independently
  of the damage system
- the engine supports sacrifice-style mechanics cleanly

It is conceptually similar to `OneHitKoEffect`,
but targets the attacker instead of the defender.

---

### StatChangeEffect.cs

#### Purpose
`StatChangeEffect` modifies stat stages
of either the attacker or the defender.

It represents moves such as:
- Swords Dance (+2 Attack, Self)
- Growl (-1 Attack, Enemy)
- Amnesia (+2 Special, Self)
- Tail Whip (-1 Defense, Enemy)
- Agility (+2 Speed, Self)
AND as a secondary effect:
- Aurora Beam (-1 Attack, Enemy)
- Acid (-1 Defense, Enemy)
- BubbleBeam (-1 Speed, Enemy)
- Psychic (-1 SpDefense, Enemy)

This is one of the core mechanics
of the battle engine.

---

#### Constructor

`StatChangeEffect(Targets target, int? chance, int stages, Stat affectedStat)`

Parameters:

- `target` → who is affected:
  - `Enemy`
  - `Self`
- `chance` → optional percentage chance
  - null → always occurs
  - value → additional effect chance
- `stages` → how many stages to change
  - positive → increase
  - negative → decrease
- `affectedStat` → which stat is modified

---

#### Core Mechanic

When `Apply(BattleContext context)` is called:

1) Determine whether the effect occurs:

   `CheckWhetherAdditionalEffectOccured(_chance, context)`

   - If `_chance` is null → always occurs
   - Otherwise → RNG roll

2) If effect does not occur:
   - Exit silently.

3) Determine target Pokémon:

   - `Targets.Enemy` → Defender
   - `Targets.Self` → Attacker
   - `Targets.All` → not implemented

4) Retrieve previous stat stage:

   `prevStage = target.StatStages.GetStage(stat)`

5) Apply stage change:

   `ChangeStat(stat, stages)`

6) Retrieve new stage:

   `curStage = target.StatStages.GetStage(stat)`

7) Generate log message via:

   `GetLogMessage(stat, prevStage, curStage)`

8) Log:

   > <Pokemon>'s <message>
    **Examples:**
   > Pikachu's Attack rose!
   > Pikachu's Accuracy fell!
   > Pikachu's Speed harshly fell!
   > Pikachu's Special Attack rose sharply!

---

#### Stage System Behavior

Stat stages typically:

- Range from -6 to +6
- Modify battle calculations
- Affect:
  - Attack
  - Defense
  - Special Attack
  - Special Defense
  - Speed
  - Accuracy
  - Evasion

If the stat is already at its limit:

- Stage remains unchanged
- `GetLogMessage` should reflect:
  - "won't go higher!"
  - "won't go lower!"

---

#### Important Notes

- This effect does not directly modify raw stats.
- It modifies stat stages,
  which are interpreted later
  by `DamageCalculator`.
- It supports both guaranteed
  and chance-based stat changes.
- It cleanly separates:
  - target selection
  - RNG logic
  - stat mutation
  - logging

---

#### Architectural Role

`StatChangeEffect` is one of the most critical
non-damage effects in the engine.

It:

- interacts with `BattlePokemon`
- modifies `StatStages`
- influences future damage calculations
- supports both primary and secondary effects

Together with `DamageEffect`,
it forms the foundation
of competitive battle mechanics.

Without it,
most strategic depth would not exist.

---

### StatChangeResetEffect.cs

#### Purpose
`StatChangeResetEffect` resets all stat stages
for both battling Pokémon.

It represents moves such as:
- Haze[^20]

The effect removes all accumulated stat boosts
and drops from the battle.

---

#### Core Mechanic

When `Apply(BattleContext context)` is called:

1) Reset attacker's stat stages:

   `context.Attacker.ActivePokemon.StatStages.Reset()`

2) Reset defender's stat stages:

   `context.Defender.ActivePokemon.StatStages.Reset()`

3) Log:
   > All stat changes have been reset!

---

#### What Gets Reset

- Attack stage
- Defense stage
- Special stage
- Speed stage
- Accuracy stage
- Evasion stage

All stages return to neutral (0).

---

#### Important Notes

- This effect does not:
  - modify HP
  - remove status conditions
  - affect field conditions
- It resets both sides simultaneously.
- It bypasses any chance logic
  (always occurs when executed).
- It does not depend on `LastDamage`.

---

#### Architectural Role

`StatChangeResetEffect` is a global-stage modifier.

It:

- interacts with both battle participants
- restores stage neutrality
- provides a hard counter to setup strategies

Together with `StatChangeEffect`,
it completes the stat-stage manipulation system:

- `StatChangeEffect` → incremental modification
- `StatChangeResetEffect` → full reset

This ensures the battle engine
supports both setup and anti-setup mechanics cleanly.

---

### TypeChangeEffect.cs

#### Purpose
`TypeChangeEffect` changes the attacker’s type
to match the defender’s primary type.

It represents moves such as:
- Conversion (Gen 1 behavior)

After execution, the attacker becomes the same type
as the defender’s first listed type.

---

#### Core Mechanic

When `Apply(BattleContext context)` is called:

1) Clear attacker’s current types:

   `ActivePokemon.Types.Clear()`

2) Copy defender’s primary type:

   `ActivePokemon.Types.Add(defender.Types[0])`

3) Log:

   > <Attacker> changed type to <Type>!

---

#### Behavior Details

- Only the defender’s first (primary) type is used.
- The attacker becomes a single-type Pokémon.
- Previous types are completely removed.
- The change persists until:
  - switching out
  - battle end
  - another type-changing effect

---

#### Important Notes

- This effect does not:
  - reset stat stages
  - modify HP
  - affect status conditions
- It directly mutates the Pokémon’s type list.
- Pokémon oryginal types are still available via `BattlePokemon.Species.Types`. Thanks to that, they can be restored after switch.
- Type change immediately affects:
  - type effectiveness calculations
  - STAB logic

---

#### Architectural Role

`TypeChangeEffect` modifies a core identity property
of a Pokémon during battle.

It:

- interacts directly with the `Types` collection
- influences `TypeEffectivenessChart`
- indirectly impacts damage calculation and resistances

This demonstrates that the effect system
can safely mutate even structural battle attributes,
not just HP or stat stages.

---

## 5. Not implemented effects
- **DamageReductionEffect**
- **FixatedEffect**
- **LeechSeedEffect**
- **StatusEffect**
- **TransformEffect**
After implementing these effects, all moves from `moves_gen1.json` will be fully usable.
If these effects are triggered, an appropriate message will be displayed indicating the functionality is not available. This will not affect gameplay.

---

## 6. Summary

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
[^6]: https://m.bulbapedia.bulbagarden.net/wiki/Counter_(move)
[^7]: Move not implemented since it was introduced in Generation 2 - reference: https://m.bulbapedia.bulbagarden.net/wiki/Mirror_Coat_(move)
[^8]: Note that in the original games the number of damage was not displayed.
[^9]: https://m.bulbapedia.bulbagarden.net/wiki/Super_Fang_(move)
[^10]: https://m.bulbapedia.bulbagarden.net/wiki/Disable_(move)
[^11]: https://m.bulbapedia.bulbagarden.net/wiki/Category:HP-draining_moves
[^12]: The same as in case of damage, that number was not originally displayed.
[^13]: https://m.bulbapedia.bulbagarden.net/wiki/Category:Moves_that_deal_direct_damage
[^14]: https://m.bulbapedia.bulbagarden.net/wiki/Multistrike_move
[^15]: https://m.bulbapedia.bulbagarden.net/wiki/One-hit_knockout_move
[^16]: https://m.bulbapedia.bulbagarden.net/wiki/Metronome_(move)
[^17]: https://m.bulbapedia.bulbagarden.net/wiki/Recoil
[^18]: https://m.bulbapedia.bulbagarden.net/wiki/Category:Status_moves_that_heal_the_user_immediately
[^19]: This move should consist of `RecoverHpEffect` + `StatusEffect`. However, in the current version, the second effect hasn't been implemented, so the move functions like the moves listed above.
[^20]: https://m.bulbapedia.bulbagarden.net/wiki/Haze_(move)