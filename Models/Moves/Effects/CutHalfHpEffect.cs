using PokemonStadium.Battle.Calculators;
using PokemonStadium.Battle.Core;

namespace PokemonStadium.Models.Moves.Effects;

public class CutHalfHpEffect : IMoveEffect
{
    public CutHalfHpEffect() { }
    public void Apply(BattleContext context)
    {
        if (AccuracyCalculator.DoesMoveHit(context))
        {
            double effectiveness = TypeEffectivenessChart.GetMultiplier(context.Move.Property.Type, context.Defender.Types);
            var msg = TypeEffectivenessChart.GetLogMessage(effectiveness);
            if (msg is not null) context.Log(msg);
            if (effectiveness == 0) return;
            int hp = context.Defender.CurrentHp;
            int takenDamage = context.Defender.TakeDamage((byte)(hp / 2));
            context.Log($"{context.Defender.Species.Name} received {takenDamage} damage.");
        }
        else
        {
            context.Log($"{context.Defender.Species.Name} avoided the attack!");
        }
    }
}