using PokemonStadium.Battle.Core;

namespace PokemonStadium.Models.Moves.Effects;

public class SelfdestructEffect : IMoveEffect
{
    public SelfdestructEffect() { }
    public void Apply(BattleContext context)
    {
        context.Attacker.Knockout();
        context.Log($"{context.Attacker.Species.Name} fainted!");
    }
}