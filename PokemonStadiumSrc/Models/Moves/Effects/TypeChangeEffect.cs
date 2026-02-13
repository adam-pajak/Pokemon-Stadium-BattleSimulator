using PokemonStadium.Battle.Core;

namespace PokemonStadium.Models.Moves.Effects;

public class TypeChangeEffect : IMoveEffect
{
    public TypeChangeEffect() { }
    public void Apply(BattleContext context)
    {
        context.Attacker.ActivePokemon.Types.Clear();
        context.Attacker.ActivePokemon.Types.Add(context.Defender.ActivePokemon.Types[0]);
        context.Log($"{context.Attacker.ActivePokemon.Species.Name} changed type to {context.Defender.ActivePokemon.Types[0]}!");
    }
}