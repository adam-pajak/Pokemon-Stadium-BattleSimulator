using PokemonStadium.Battle.Core;

namespace PokemonStadium.Models.Moves.Effects;

public class TypeChangeEffect : IMoveEffect
{
    public TypeChangeEffect() { }
    public void Apply(BattleContext context)
    {
        context.Attacker.Types.Clear();
        context.Attacker.Types.Add(context.Defender.Types[0]);
        context.Log($"{context.Attacker.Species.Name} changed type to {context.Defender.Types[0]}!");
    }
}