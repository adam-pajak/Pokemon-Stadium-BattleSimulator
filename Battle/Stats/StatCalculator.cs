using PokemonStadium.Models.Pokemon;

namespace PokemonStadium.Battle.Stats;

public static class StatCalculator
{
    public static BattleStats Calculate(Pokemon pokemon, int level)
    {
        return new BattleStats
        {
            MaxHp = CalculateHp(pokemon.BaseStats.Hp, level),
            Attack = CalculateOther(pokemon.BaseStats.Attack, level),
            Defense = CalculateOther(pokemon.BaseStats.Defense, level),
            SpAttack = CalculateOther(pokemon.BaseStats.SpAttack, level),
            SpDefense = CalculateOther(pokemon.BaseStats.SpDefense, level),
            Speed = CalculateOther(pokemon.BaseStats.Speed, level),
        };
    }

    private static int CalculateHp(int baseStat, int level)
        => ((2 * baseStat * level) / 100) + level + 10;

    private static int CalculateOther(int baseStat, int level)
        => ((2 * baseStat * level) / 100) + 5;
}
