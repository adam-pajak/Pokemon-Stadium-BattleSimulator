using PokemonStadium.Battle.Core;
using PokemonStadium.Models.Enums;

namespace PokemonStadium.Battle.Calculators;
// https://bulbapedia.bulbagarden.net/wiki/Damage#Damage_calculation
public static class DamageCalculator
{
    public static byte Calculate(BattleContext context, bool isCritical, double typeEffectiveness)
    {
        int power = context.Move.Property.Power ?? 0;
        
        int level = context.Attacker.Level;
        
        int critical = isCritical ? 2 : 1;
        
        int attack = context.Move.Property.Category == MoveCategory.Physical
            ? context.Attacker.GetAttack(isCritical)
            : context.Attacker.GetSpAttack(isCritical);
        
        int defense = context.Move.Property.Category == MoveCategory.Physical
            ? context.Defender.GetDefense(isCritical)
            : context.Defender.GetSpDefense(isCritical);
        
        if (attack > 255 || defense > 255)
        {
            attack /= 4;
            defense /= 4;
        }
        int damage = (2 * level * critical / 5 + 2) * power * attack / defense / 50 + 2;
        
        if (context.Attacker.Types.Contains(context.Move.Property.Type)) damage = (damage * 3) / 2;
        
        if (typeEffectiveness == 0) return 0;
        
        damage = (int)(damage * typeEffectiveness);
        
        
        int randomModifier = context.Range.Next(217, 256);
        damage = (damage * randomModifier) / 255;
        if (damage < 1) damage = 1;
        
        return (byte)damage;
    }
}