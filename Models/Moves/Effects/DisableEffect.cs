using PokemonStadium.Battle.Core;

namespace PokemonStadium.Models.Moves.Effects;

public class DisableEffect : IMoveEffect
{
    public DisableEffect() { }
    public void Apply(BattleContext context)
    {
        var lastMove = context.LastMove;
        if (lastMove is null)
        {
            context.Log("But it failed!");
            return;
        }
        if (!lastMove.IsDisabled)
        {
            int duration = context.Range.Next(0, 7);
            lastMove.Disable(duration);
            context.Log($"{context.Attacker.ActivePokemon.Species.Name}'s {lastMove.Property.Name} was disabled!");
        }
        else
        {
            context.Log($"{context.Attacker.ActivePokemon.Species.Name}'s {lastMove.Property.Name} is already disabled!");
        }
    }
}