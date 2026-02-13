using System.Diagnostics;
using PokemonStadium.Models.Enums;

namespace PokemonStadium.Battle.Calculators;

public static class TypeEffectivenessChart
{
    private static readonly Dictionary<PokemonType, Dictionary<PokemonType, double>> Chart
        = new()
        {
            [PokemonType.Normal] = new()
            {
                [PokemonType.Rock] = 0.5,
                [PokemonType.Ghost] = 0.0
            },
            [PokemonType.Fire] = new()
            {
                [PokemonType.Grass] = 2.0,
                [PokemonType.Ice] = 2.0,
                [PokemonType.Bug] = 2.0,

                [PokemonType.Fire] = 0.5,
                [PokemonType.Water] = 0.5,
                [PokemonType.Rock] = 0.5,
                [PokemonType.Dragon] = 0.5
            },
            [PokemonType.Water] = new()
            {
                [PokemonType.Fire] = 2.0,
                [PokemonType.Ground] = 2.0,
                [PokemonType.Rock] = 2.0,

                [PokemonType.Water] = 0.5,
                [PokemonType.Grass] = 0.5,
                [PokemonType.Dragon] = 0.5
            },
            [PokemonType.Electric] = new()
            {
                [PokemonType.Water] = 2.0,
                [PokemonType.Flying] = 2.0,

                [PokemonType.Electric] = 0.5,
                [PokemonType.Grass] = 0.5,
                [PokemonType.Dragon] = 0.5,

                [PokemonType.Ground] = 0.0
            },
            [PokemonType.Grass] = new()
            {
                [PokemonType.Water] = 2.0,
                [PokemonType.Ground] = 2.0,
                [PokemonType.Rock] = 2.0,

                [PokemonType.Fire] = 0.5,
                [PokemonType.Grass] = 0.5,
                [PokemonType.Poison] = 0.5,
                [PokemonType.Flying] = 0.5,
                [PokemonType.Bug] = 0.5,
                [PokemonType.Dragon] = 0.5
            },
            [PokemonType.Ice] = new()
            {
                [PokemonType.Grass] = 2.0,
                [PokemonType.Ground] = 2.0,
                [PokemonType.Flying] = 2.0,
                [PokemonType.Dragon] = 2.0,

                [PokemonType.Fire] = 0.5,
                [PokemonType.Water] = 0.5,
                [PokemonType.Ice] = 0.5
            },
            [PokemonType.Fighting] = new()
            {
                [PokemonType.Normal] = 2.0,
                [PokemonType.Ice] = 2.0,
                [PokemonType.Rock] = 2.0,

                [PokemonType.Poison] = 0.5,
                [PokemonType.Flying] = 0.5,
                [PokemonType.Psychic] = 0.5,
                [PokemonType.Bug] = 0.5,

                [PokemonType.Ghost] = 0.0
            },
            [PokemonType.Poison] = new()
            {
                [PokemonType.Grass] = 2.0,

                [PokemonType.Poison] = 0.5,
                [PokemonType.Ground] = 0.5,
                [PokemonType.Rock] = 0.5,
                [PokemonType.Ghost] = 0.5
            },
            [PokemonType.Ground] = new()
            {
                [PokemonType.Fire] = 2.0,
                [PokemonType.Electric] = 2.0,
                [PokemonType.Poison] = 2.0,
                [PokemonType.Rock] = 2.0,

                [PokemonType.Grass] = 0.5,
                [PokemonType.Bug] = 0.5,

                [PokemonType.Flying] = 0.0
            },
            [PokemonType.Flying] = new()
            {
                [PokemonType.Grass] = 2.0,
                [PokemonType.Fighting] = 2.0,
                [PokemonType.Bug] = 2.0,

                [PokemonType.Electric] = 0.5,
                [PokemonType.Rock] = 0.5
            },
            [PokemonType.Psychic] = new()
            {
                [PokemonType.Fighting] = 2.0,
                [PokemonType.Poison] = 2.0,

                [PokemonType.Psychic] = 0.5
            },
            [PokemonType.Bug] = new()
            {
                [PokemonType.Grass] = 2.0,
                [PokemonType.Psychic] = 2.0,

                [PokemonType.Fire] = 0.5,
                [PokemonType.Fighting] = 0.5,
                [PokemonType.Poison] = 0.5,
                [PokemonType.Flying] = 0.5,
                [PokemonType.Ghost] = 0.5
            },
            [PokemonType.Rock] = new()
            {
                [PokemonType.Fire] = 2.0,
                [PokemonType.Ice] = 2.0,
                [PokemonType.Flying] = 2.0,
                [PokemonType.Bug] = 2.0,

                [PokemonType.Fighting] = 0.5,
                [PokemonType.Ground] = 0.5
            },
            [PokemonType.Ghost] = new()
            {
                [PokemonType.Psychic] = 2.0,
                [PokemonType.Ghost] = 2.0,

                [PokemonType.Normal] = 0.0
            },
            [PokemonType.Dragon] = new()
            {
                [PokemonType.Dragon] = 2.0
            },

        };

    public static double GetMultiplier(PokemonType attackType, IEnumerable<PokemonType> defenderTypes)
    {
        double multiplier = 1.0;

        foreach (var defenderType in defenderTypes)
        {
            multiplier *= GetSingleMultiplier(attackType, defenderType);
        }
        
        return multiplier;
    }

    private static double GetSingleMultiplier(PokemonType attack, PokemonType defense)
    {
        if (Chart.TryGetValue(attack, out var row) && row.TryGetValue(defense, out var value))
        {
            return value;
        }

        return 1.0;
    }
    public static string? GetLogMessage(double multiplier)
    {
        return multiplier switch
        {
            0.0 => "It has no effect!",
            > 1.0 => "It's super effective!",
            < 1.0 => "It's not very effective!",
            1.0 => null,
            _ =>  throw new InvalidDataException("Unknown multiplier")
        };
    }
}
