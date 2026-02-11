using PokemonStadium.Models.Enums;

namespace PokemonStadium.Battle.Stats;

// https://bulbapedia.bulbagarden.net/wiki/Stat_modifier#Stage_multipliers
public static class StatStageMultipliers
{
    public static Fraction GetMultiplier(int stage)
    {
        return stage switch
        {
            -6 => new(25, 100), // -6
            -5 => new(28, 100), // -5
            -4 => new(33, 100), // -4
            -3 => new(40, 100), // -3
            -2 => new(50, 100), // -2
            -1 => new(66, 100), // -1
            0 => new(100, 100), //  0
            1 => new(150, 100), // +1
            2 => new(200, 100), // +2
            3 => new(250, 100), // +3
            4 => new(300, 100), // +4
            5 => new(350, 100), // +5
            6 => new(400, 100),  // +6
            _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, null)
        };
    }

    public static Fraction GetAccuracyEvasionMultiplier(int stage, Stat stat)
    {
        stage = stat == Stat.Accuracy ? stage : -stage;
        {
            return stage switch
            {
                -6 => new(1, 3),
                -5 => new(36, 100),
                -4 => new(43, 100),
                -3 => new(50, 100),
                -2 => new(66, 100),
                -1 => new(75, 100),
                0 => new(1, 1),
                1 => new(133, 100),
                2 => new(166, 100),
                3 => new(200, 100),
                4 => new(233, 100),
                5 => new(266, 100),
                6 => new(3, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, null)
            };
        }
        
    }
    
    
}
