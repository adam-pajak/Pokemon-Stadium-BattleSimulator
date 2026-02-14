# Pokemon-Stadium-BattleSimulator
Pokémon Stadium-style battle simulator inspired by Gen 1 mechanics, written in **C#**.

## About
This project is a battle engine simulator inspired by **Pokémon Stadium**.
The goal is to recreate classic Pokémon battle mechanics with clean code and expandable architecture.

## Features
- All 151 gen 1 Pokémon available with their respective movesets[^1]
- 17 unique pre-programmed effects (learn more in technical documentation in /docs folder)
- Almost all **Pokémon Stadium** battle mechanics implemented
- Simple command line interface

## Planned Features
- Implement 5 planned effects that already exist in source code
- Status conditions
- HP (Hit point) bars
- Adding your own Pokémon
- Next gen Pokemon, moves, mechanics etc.

## Project Structure
```
Pokemon-Stadium-BattleSimulator/
├── Battle/
│ ├── Calculators/
│ │ ├── AccuracyCalculator.cs
│ │ ├── CriticalHitCalculator.cs
│ │ ├── DamageCalculator.cs
│ │ └── TypeEffectivenessChart.cs
│ ├── Core/
│ │ ├── BattleContext.cs
│ │ ├── BattleMove.cs
│ │ └── BattlePokemon.cs
│ ├── Stats/
│ │ ├── BattleStats.cs
│ │ ├── Fraction.cs
│ │ ├── StatCalculator.cs
│ │ ├── StatStageMultipliers.cs
│ │ └── StatStages.cs
│ ├── Statuses/ (empty)
│ └── Trainers/
│   ├── ComputerTrainer.cs
│   ├── PlayerTrainer.cs
│   └── Trainer.cs
├── Data/
│ ├── moves_gen1.json
│ └── pokemon_gen1.json
├── Models/
│ ├── DTO/
│ │ ├── MoveDto.cs
│ │ ├── MoveEffectDto.cs
│ │ └── PokemonDto.cs
│ └── Enums/
│   ├── CriticalRatio.cs
│   ├── EffectType.cs
│   ├── MoveCategory.cs
│   ├── PokemonType.cs
│   ├── Stat.cs
│   ├── Status.cs
│   └── Targets.cs
├── Moves/
│ ├── Effects/
│ │ ├── IMoveEffect.cs
│ │ ├── DamageEffect.cs
│ │ ├── MultistrikeEffect.cs
│ │ ├── StatChangeEffect.cs
│ │ └── ...
│ └── Move.cs
├── Pokemon/
│ ├── BaseStats.cs
│ └── Pokemon.cs
├── Services/
│ ├── DataLoader.cs
│ └── Game.cs
├── docs/
│ ├── README.md
│ └── techcical documentation files...
├── README.md
└── Program.cs
```
## Requirements
- .NET SDK (version 9.0 or higher)[^2]

Check your .NET version: 
```bash
dotnet --version
```
## Build and run
1. Clone repository:
```bash
git clone https://github.com/adam-pajak/Pokemon-Stadium-BattleSimulator.git
cd Pokemon-Stadium-BattleSimulator
```
2. Build:
```bash
dotnet build
```
3. Run:
```bash
dotnet run
```
## How to play
- Choose your Pokémon team
- Select moves or switch each turn
- Defeat all opponent Pokémon to win

## Game flow
1. Player enters their name, opponent's name and chooses Pokémon for both players.
2. Battle begins
3. Player chooses action => move or switch
4. Opponent chooses action => move or switch
5. Turn order is calculated
6. Moves (effects) are executed one by one
7. If one of the players (or both) lost all their Pokémon => battle ends, else => go to step 3.
8. One of the players wins and battle can be reviewed.

## Documentation
More documentation will be added inside the /docs folder.
## License
No license yet.
## Contributing
This project is currently developed as a personal learning project.
Suggestions and pull requests are welcome.

## Notes
[^1]: From: https://bulbapedia.bulbagarden.net/wiki/List_of_Pok%C3%A9_Cup_Rental_Pok%C3%A9mon_in_Pok%C3%A9mon_Stadium (note that some moves were not used due to their effect and were replaced by other moves)
[^2]: Choose version and download it from this website: https://dotnet.microsoft.com/en-us/download/dotnet
