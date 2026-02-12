using PokemonStadium.Battle.Calculators;
using PokemonStadium.Battle.Core;

namespace PokemonStadium.Models.Moves.Effects;

public class CutHalfHpEffect : IMoveEffect
{
    public CutHalfHpEffect() { }
    public void Apply(BattleContext context)
    {
    
        double effectiveness = TypeEffectivenessChart.GetMultiplier(context.Move.Property.Type, context.Defender.ActivePokemon.Types);
        var msg = TypeEffectivenessChart.GetLogMessage(effectiveness);
        if (msg is not null) context.Log(msg);
        if (effectiveness == 0) return;
        int hp = context.Defender.ActivePokemon.CurrentHp;
        int takenDamage = context.Defender.ActivePokemon.TakeDamage((byte)(hp / 2));
        context.Log($"{context.Defender.ActivePokemon.Species.Name} received {takenDamage} damage.");
    }
}