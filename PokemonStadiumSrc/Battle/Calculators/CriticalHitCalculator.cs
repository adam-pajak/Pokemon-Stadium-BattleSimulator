using PokemonStadium.Battle.Core;
using PokemonStadium.Models.Enums;

namespace PokemonStadium.Battle.Calculators;

// https://bulbapedia.bulbagarden.net/wiki/Critical_hit#Pok%C3%A9mon_Stadium

public static class CriticalHitCalculator
{

    public static bool IsCriticalHit(BattleContext context, CriticalRatio ratio)
    {
        int speed = context.Attacker.ActivePokemon.Species.BaseStats.Speed;

        int threshold = (speed + 76) / 4;

        if (ratio != CriticalRatio.High)
            threshold = Math.Min(threshold, 255);

        int roll = context.Range.Next(256);
        bool isCritical = roll < threshold;
        
        return isCritical;
    }
}
