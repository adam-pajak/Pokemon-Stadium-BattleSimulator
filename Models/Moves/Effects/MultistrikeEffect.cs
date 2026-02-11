using PokemonStadium.Battle.Calculators;
using PokemonStadium.Battle.Core;
using PokemonStadium.Models.Enums;

namespace PokemonStadium.Models.Moves.Effects;

public class MultistrikeEffect : IMoveEffect
{
    private readonly CriticalRatio _criticalRatio;
    private readonly int _minHits;
    private readonly int _maxHits;

    public MultistrikeEffect(CriticalRatio criticalRatio, int minHits, int maxHits)
    {
        _criticalRatio = criticalRatio;
        _minHits = minHits;
        _maxHits = maxHits;
    }
    public void Apply(BattleContext context)
    {
        double effectiveness = TypeEffectivenessChart.GetMultiplier(context.Move.Property.Type, context.Defender.Types);
        bool isCritical = CriticalHitCalculator.IsCriticalHit(context, _criticalRatio);
        int hits;
        if (_minHits == 2 && _maxHits == 5)
        {
            hits = RollHits(context.Range);
        }
        else
        {
            hits = 2;
        }
        int h;
        for (h = 0; h < hits; h++)
        {
            byte damage = DamageCalculator.Calculate(context, isCritical, effectiveness);
            int damageTaken = context.Defender.TakeDamage(damage);
            if (damage > 0)
            {
                context.Log($"{context.Defender.Species.Name} received {damageTaken} damage!");
            }
            if (context.Defender.IsFainted)
            {
                context.Log($"{context.Defender.Species.Name} fainted.");
                break;
            }
            context.LastDamage = damageTaken;
        }

        if (h > 0)
        {
            var msg = TypeEffectivenessChart.GetLogMessage(effectiveness);
            if (msg is not null) context.Log(msg);
            if (isCritical) context.Log("Critical hit!");
            context.Log($"It hit {h} time(s)!");
        }
    }
    private static int RollHits(Random range)
    {
        int roll = range.Next(256);
        if (roll < 96)
            return 2;
        if (roll < 192)
            return 3;
        if (roll < 224)
            return 4;
        return 5;
    }
}