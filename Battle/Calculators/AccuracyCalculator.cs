using PokemonStadium.Battle.Core;
using PokemonStadium.Models.Enums;

namespace PokemonStadium.Battle.Calculators;
// https://bulbapedia.bulbagarden.net/wiki/Accuracy#Accuracy_check
public static class AccuracyCalculator
{
    public static bool DoesMoveHit(BattleContext context)
    {
        if (context.Move.Property.Target == Targets.Self) return true;
        if (context.Defender.IsInvulnerable || context.Defender.IsFainted) return false;
        if (context.Move.Property.Accuracy is null) return true;
        
        int accuracy = context.Move.Property.Accuracy.Value * 255 / 100;
        accuracy = context.Attacker.GetAccuracy(accuracy);
        accuracy = context.Defender.GetEvasion(accuracy);
        accuracy = Math.Clamp(accuracy, 1, 255);
        int roll = context.Range.Next(256);
        bool hit = roll <= accuracy;
        return hit;
    }
}