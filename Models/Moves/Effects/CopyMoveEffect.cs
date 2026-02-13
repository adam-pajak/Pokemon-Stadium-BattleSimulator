using PokemonStadium.Battle.Core;

namespace PokemonStadium.Models.Moves.Effects;

public class CopyMoveEffect : IMoveEffect
{
    private readonly bool _replace;

    public CopyMoveEffect(bool replace)
    {
        _replace = replace;
    }
    public void Apply(BattleContext context)
    {
        // Mimic
        if (_replace)
        {
            var newMove = context.Defender.ActivePokemon.Moves[context.Range.Next(0, context.Defender.ActivePokemon.Moves.Count)];
            int index = context.Attacker.ActivePokemon.Moves.FindIndex(m => m.Property.Name == "Mimic");
            context.Attacker.ActivePokemon.Moves.Insert(index, newMove);
            context.Log($"{context.Attacker.ActivePokemon.Species.Name} learned {newMove}!");
        }
        // MirrorMove
        else
        {
            if (context.LastMove is null)
            {
                context.Log("But it failed!");
                return;
            }
            context.Log($"{context.Attacker.ActivePokemon.Species.Name} copied {context.LastMove.Property.Name}!");
            if (context.LastMove.Property.Name == "MirrorMove")
            {
                context.Log("But it failed!");
                return;
            }
            context.LastMove.Use(context);
        }
    }
}