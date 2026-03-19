# Data Format

This document defines the structure of all external data  
used by the battle engine.

It describes how moves, Pokémon species, and related data  
should be represented in configuration files (e.g. JSON).

---

## Overview

The engine is data-driven.

All battle behavior is defined through:

- Move definitions
- Pokémon species data
- Effect configuration

The engine loads this data and maps it to runtime objects.

---

## Design Principles

- Data is declarative (no logic in data files)
- Behavior is defined by effects
- Species data is immutable
- All identifiers must be consistent and unique
- Effects are configured, not hardcoded

---

## Move Data

A move definition contains:

- Name
- Type
- Category
- Power
- Accuracy
- PP (Power Points)
- Priority
- Target
- Effects

Example:

```json
{
  "Name": "Slash",
  "Type": "Normal",
  "Category": "Physical",
  "Power": 70,
  "Accuracy": 100,
  "Pp": 20,
  "Priority": 0,
  "Target": "Enemy",
  "Effects": [
    {
      "EffectType": "Damage",
      "CriticalRatio": "High"
    }
  ]
}
```

### Move Fields

#### 1. Name

- Unique identifier
- Used for lookup and logging

#### 2. Type

- Enum: `PokemonType`

#### 3. Category

- Physical / Special / Status

#### 4. Power

- Integer or null (null means non-damaging move)

#### 5. Accuracy

- Integer (0–100) or null (null means moves that cannot miss)

#### 6. Target

- Self / Enemy / (future: All)

#### 7. Effects

- Ordered list of effects
- Executed sequentially

---

### Effect Configuration

Each effect entry must contain:

`effectType` → name of the effect class

Additional fields depend on effect type.

Example:
```json
{
  "EffectType": "Status",
  "Target": "Enemy",
  "Chance": 30,
  "Status": "Poison"
}
```


### Effect Mapping
Each effect type must be mapped
to a corresponding class in the engine.

Example:

- `"DamageEffect"` → `DamageEffect`
- `"RecoilEffect"` → `RecoilEffect`
- `"StatChangeEffect"` → `StatChangeEffect`

This mapping is handled by the data loader (factory).

---

## Pokémon Species Data
Defines immutable base data.

Example:
```json
{
    "Id": 25,
    "Name": "Pikachu",
    "Types": [
      "Electric"
    ],
    "BaseStats": {
      "Hp": 35,
      "Attack": 55,
      "Defense": 40,
      "SpAttack": 50,
      "SpDefense": 50,
      "Speed": 90
    },
    "Moves": ["Thunderbolt", "SeismicToss", "Slam", "ThunderWave"]
}
```


### Species fields

#### 1. Id
- Pokédex number (unique identifier)
#### 2. Name
- Used for logging
#### 3. Types
- List of: Enum: PokemonType
- One or two types
- Immutable during battle (base reference)
#### 4. BaseStats
- Used to initialize BattlePokemon
- Neither modified nor used during battle
#### 5. Moves
- Name reference to `Move` field - `Name`
- Used to create list of moves

---

## Runtime vs data
Important distinction:

### Data (this file describes)
- Move definitions
- Species definitions
- Effect configuration

### Runtime (engine handles)

- Current HP
- Battle stats and Stat stages
- ~~Status conditions~~[^1]
- Temporary type changes
- Battle flags

---

## Validation Rules

Data should be validated before use:

- Names must be unique
- Types must exist in enum
- Effects must be valid
- Required fields must be present
- Numeric values must be in valid ranges

If [DataLoader](data-loader.md) finds invalid or unknown data, an exception will be thrown.

---

## Extensibility

To add a new effect:
- Create a new effect class
- Add a corresponding data format
- Extend the data loader factory
- Use the effect in move definitions

No engine modification should be required if created effect affects only during move's hit.

## Summary
The data format defines:
- What exists (moves, species)
- How they behave (effects)
- How they are configured (JSON)
The game engine is responsible only for execution.

## Notes
[^1]: Not implemented - reference: https://bulbapedia.bulbagarden.net/wiki/Status_condition