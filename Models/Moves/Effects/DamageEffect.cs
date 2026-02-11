using PokemonStadium.Battle.Calculators;
using PokemonStadium.Battle.Core;
using PokemonStadium.Models.Enums;

namespace PokemonStadium.Models.Moves.Effects;

public class DamageEffect : IMoveEffect
{
    private readonly CriticalRatio _criticalRatio;
    public DamageEffect(CriticalRatio criticalRatio)
    {
        _criticalRatio = criticalRatio;
    }
    public void Apply(BattleContext context)
    {
        double effectiveness = TypeEffectivenessChart.GetMultiplier(context.Move.Property.Type, context.Defender.Types);
        var msg = TypeEffectivenessChart.GetLogMessage(effectiveness);
        if (msg is not null) context.Log(msg);
        bool isCritical = CriticalHitCalculator.IsCriticalHit(context, _criticalRatio);
        if (isCritical) context.Log("Critical hit!");
        byte damage = DamageCalculator.Calculate(context, isCritical, effectiveness);
        int damageTaken = context.Defender.TakeDamage(damage);
        if (damageTaken > 0)
        {
            context.Log($"{context.Defender.Species.Name} received {damageTaken} damage!");
        }
        if (context.Defender.IsFainted) context.Log($"{context.Defender.Species.Name} fainted!");
        context.LastDamage = damageTaken;
    }
}
