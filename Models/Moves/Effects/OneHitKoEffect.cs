using PokemonStadium.Battle.Calculators;
using PokemonStadium.Battle.Core;

namespace PokemonStadium.Models.Moves.Effects;

public class OneHitKoEffect : IMoveEffect
{
    public OneHitKoEffect() { }
    public void Apply(BattleContext context)
    {
        double effectiveness = TypeEffectivenessChart.GetMultiplier(context.Move.Property.Type, context.Defender.Types);
        var msg = TypeEffectivenessChart.GetLogMessage(effectiveness);
        if (msg is not null) context.Log(msg);
        if (effectiveness == 0) return;
        context.Defender.Knockout();
        context.Log("It's One-Hit KO!");
        context.Log($"{context.Defender.Species.Name} fainted!");
    }
}