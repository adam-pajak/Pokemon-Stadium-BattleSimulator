# Game Flow

## Overview

This document describes the full execution flow  
of the battle simulator — from program start  
to battle resolution.

It explains how the system transitions between:

- Initialization
- Team setup
- Turn resolution
- Battle end

---

## Entry Point

The program starts in: **Program.cs**

### Responsibilities

- Load data using `DataLoader`
- Collect input from the user
- Create trainers
- Initialize battle context
- Start the game loop
- Review the finished battle

---

## Initialization Phase

### 1. Load Data

The following data is loaded:

- [Moves](data-loader.md#move-loading) (JSON → Move objects)
- [Pokémon species](data-loader.md#pokémon-loading) (JSON → Pokemon objects)

This step ensures:

- All moves exist
- All Pokémon reference valid moves
- All effects are correctly initialized

---

### 2. Player Input

The program asks the user for:

- Player name
- Opponent name
- Team size
- Pokémon selection for both players

At this stage:

- Teams are created
- Pokémon instances are assigned to trainers

---

### 3. Trainer Creation

Two trainers are created:

- [PlayerTrainer](trainers.md#3-playertrainercs)
- [ComputerTrainer](trainers.md#4-computertrainercs)

Each trainer contains:

- Name
- Team of Pokémon
- Active Pokémon (initially first in team)


---

### 4. Battle Context Initialization

A [`BattleContext`](battle-core.md#2-battlecontextcs) object is created.

It contains:

- Attacker and Defender (dynamic per turn)
- Current move
- Last move used
- Last damage dealt
- Random number generator
- Logging system

This object is shared across all systems.

---

## Main Game Loop

The game is executed using:

`Game.PlayRound(...)`

This method represents one full battle loop.

---

## Turn Flow

Each turn follows these steps:


### 0. Pokémon State Check

Before decision phase if any active pokémon is fainted, its trainer is forced to switch to a new one. 

---

### 1. Player Decision Phase

Each trainer chooses an action:

- Use move
- Switch Pokémon

Player input or AI determines the choice.

---

### 2. Turn Order Resolution

The order of actions is determined based on:

- Move priority
- Pokémon speed

The faster Pokémon acts first.

---

### 3. Action Execution

Each action is executed in order.

If the action is a move:

1. Accuracy check (`AccuracyCalculator`)
2. If hit:
    - Execute move effects in order
3. If miss:
    - Execute always-occuring effects if exists[^1]
    - Log miss message if necessary

If the action is a switch:

- Active Pokémon is replaced
- No damage calculation occurs

---

### 4. Effect Execution

Each move contains a list of effects.

Effects are executed sequentially:

- DamageEffect
- StatChangeEffect
- etc.

Each effect can:

- Read from `BattleContext`
- Modify battle state
- Log output

For details see: [move effects overview](move-effects.md#1-overview).

---


### 5. End Turn Check

Check if battle should end:

If:

- One trainer has no remaining Pokémon

Then:

- Battle ends
- Winner is declared

Otherwise:

- Next turn begins

---

## Battle End

When a trainer loses all Pokémon:

- The game logs the result
- The game loop is terminated

---

## After battle review
Player can choose whether to see battle review which contains all battle logs preceded by turn they has appeared. 

## Game Flow Diagram

The diagram below illustrates the high-level execution flow  
of the battle system.
```
                    ┌──────────────────────┐
                    │      Program.cs      │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │      DataLoader      │
                    │  (moves, pokemons)   │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │     Player Input     │
                    │    (names, teams)    │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │    Create Trainers   │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │  BattleContext Init  │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────┐                                        
                    │   Game.PlayRound()   ├───────────┐
                    └──────────┬───────────┘           │
                               │                       │
                               ▼                       │
                    ┌──────────────────────┐           │
                    │     Turn start       │           │
                    │ Pokémon state check  │           │
                    └──────────┬───────────┘           │
                               │                       │
                               ▼                       │
                    ┌──────────────────────┐           │
                    │       Players        │           │
                    │    choose actions    │           │
                    └──────────┬───────────┘           │
                               │                       │
                               ▼                       │
                    ┌──────────────────────┐           │
                    │ Determine turn order │           │
                    │ (priority + speed)   │           │
                    └──────────┬───────────┘           │
                               │                       │
                               ▼                       │
                    ┌──────────────────────┐           │
       ┌────────────┤    Execute actions   │           │
       │            │       in order       │           │
       │            └──────────┬───────────┘           │
       │                       │                       │
       │                       ▼                       │
       │            ┌──────────────────────┐           │                            
       │            │    Accuracy check    │           │
       │            └────┬───────────┬─────┘           │
       │                 │           │                 │
       │                 │ hit       │ miss            │
       │                 ▼           ▼                 │
       │    ┌─────────────────┐  ┌─────────────────┐   │
       │    │ Execute effects │  │     Execute     │   │
       │    └────────┬────────┘  │  always effects │   │
       │             │           └────────┬────────┘   │
       │             │                    ▼            │
       │             │           ┌─────────────────┐   │
       │             │           │  Log miss msg   │   │
       │             │           └────────┬────────┘   │
       │             ▼                    ▼            │
       │          ┌──────────────────────────┐         │
       │          │       Faint Check        │         │
       │          └────────────┬─────────────┘         │
       │                       │                       │
       │                       ▼                       │
       │          ┌──────────────────────────┐         │
       │      NO  │     If both actions      │         │
       └──────────┤    has been executed     │         │
                  └────────────┬─────────────┘         │
                               │ YES                   │
                               ▼                       │
                    ┌──────────────────────┐ continue  │
                    │    End turn check    ├───────────┘
                    └──────────┬───────────┘
                               │
                               │ end
                               ▼
                    ┌──────────────────────┐
                    │    Declare winner    │
                    └──────────┬───────────┘
                               ▼
```
---

## Design Principles

The game flow is designed to be:

- Deterministic (with seeded RNG)
- Modular (separate systems for accuracy, damage, effects)
- Data-driven (moves and effects defined externally)
- Extensible (new mechanics require minimal engine changes)

---

## Key Concepts

- `BattleContext` is the central state container
- Effects handle all battle logic
- Calculators handle pure computations
- Game controls flow, not logic

---

## Summary

The battle system operates as a loop of:

1. Decision-making
2. Action resolution
3. State updates

Until one trainer runs out of Pokémon.

The engine separates:

- Flow control (Game)
- State (BattleContext)
- Logic (Effects & Calculators)

This ensures clarity, scalability,  
and ease of extension.

## Notes
[^1]: Currently [ChargingTurnEffect](move-effects.md#chargingturneffectcs) (partially) and [SelfDestructEffect](move-effects.md#selfdestructeffectcs)