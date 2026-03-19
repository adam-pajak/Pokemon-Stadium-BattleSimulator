# Data Loading System

## Data Loader

The `DataLoader` is responsible for loading external data  
(JSON files) and transforming it into runtime objects  
used by the battle engine.

It acts as a bridge between:

- Declarative data (DTOs, JSON)
- Runtime domain models (Move, Pokemon, Effects)

---

## Purpose

The loader handles three main tasks:

1. Loading moves from JSON
2. Loading Pokémon species from JSON
3. Creating move effects using a factory method

It ensures that:

- Data is valid
- Objects are properly constructed
- All references are resolved

---

## JSON Deserialization

The loader uses `System.Text.Json` with:

- Case-insensitive property matching
- Enum string conversion

Configuration:

- Property names are not case-sensitive
- Enums are read from strings (e.g. `"Fire"` → `PokemonType.Fire`)

This makes JSON files easier to write and maintain.

---

## Move Loading

**Method:** `LoadMoves(string path)`

### Flow

1. Read JSON file
2. Deserialize into `List<MoveDto>`
3. Convert each DTO into a `Move`
4. Store moves in a dictionary:

Dictionary<string, Move>

Key:

- Move name (case-insensitive)

---

## Move Mapping

**Method:** `MapMove(MoveDto dto)`

### Responsibilities

Creates a runtime `Move` object:

- Copies basic properties:
    - Name
    - Type
    - Category
    - Power
    - Accuracy
    - PP
    - Priority
    - Target

- Initializes empty effect list

- Converts DTO effects into runtime effects

Effects are created using: `CreateEffect(MoveEffectDto dto, Move move)`

---

## Pokémon Loading

**Method:** `LoadPokemons(string path, Dictionary<string, Move> movesByName)`

### Flow

1. Read JSON file
2. Deserialize into `List<PokemonDto>`
3. For each Pokémon:
    - Resolve move names → Move objects
    - Build `BaseStats`
    - Create `Pokemon` instance
4. Return list of Pokémon

---

## Move Resolution

Each Pokémon references moves by name.

During loading:

- Each move name is looked up in the move dictionary
- If a move is missing → exception is thrown

This ensures referential integrity.

---

## Base Stats Mapping

DTO stats are converted into:

BaseStats object:

- Hp
- Attack
- Defense
- Special Attack
- Special Defense
- Speed

These values are immutable during battle.

---

## Effect Factory

Method: `CreateEffect(MoveEffectDto dto, Move move)`

This is the core of the data-driven system.

---

## Factory Responsibilities

- Converts DTO → concrete `IMoveEffect`
- Validates required parameters
- Injects dependencies (e.g. move category)

Each effect type maps to a class.

Example:

`EffectType.Damage` → `DamageEffect`  
`EffectType.Recoil` → `RecoilEffect`  
`EffectType.StatChange` → `StatChangeEffect`

---

## Parameter Validation

Each effect validates required fields:

Example:

- `DamageEffect` requires:
    - Power
    - CriticalRatio

- `StatChangeEffect` requires:
    - Target
    - Stages
    - AffectedStat

If required data is missing:

- An exception is thrown
- Loading fails immediately

This prevents invalid runtime behavior.

---

## Context-Aware Effects

Some effects depend on move data:

Example:

`CounterDamageEffect(move.Category)`

This allows effects to:

- Adapt to move configuration
- Avoid duplicating data in JSON

---

## Error Handling

The loader throws exceptions when:

- JSON cannot be deserialized
- Move references are invalid
- Required effect parameters are missing
- Unknown effect types are encountered

This ensures:

- Early failure
- Easier debugging
- Data consistency

---

## Design Principles

The data loader is:

- Deterministic
- Strict (fails fast on invalid data)
- Centralized (single loading entry point)
- Extensible (new effects require only factory extension)

---

## Extending the System

To add a new effect:

1. Create a new class implementing `IMoveEffect`
2. Add new `EffectType` enum value
3. Extend `CreateEffect` with a new case
4. Define required fields in `MoveEffectDto`
5. Use the effect in JSON

No changes are required in the battle engine.

---

## Runtime vs DTO

Important distinction:

DTO (Data Transfer Object):

- Represents JSON structure
- Used only during loading

Runtime objects:

- Used during battle
- Contain behavior and logic

The loader is responsible for mapping between them.

---

## Summary

The `DataLoader`:

- Loads and validates external data
- Maps DTOs to runtime objects
- Resolves references
- Creates effects using a factory pattern

It enables a fully data-driven battle system  
where behavior is defined in JSON,  
not hardcoded in the engine.