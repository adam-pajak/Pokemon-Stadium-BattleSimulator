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
        if (context.LastMove is null)
        {
            context.Log("But it failed!");
            return;
        }
        if (_replace)
        {
            var index = context.Attacker.Moves.FindIndex(move => move == context.LastMove);
            context.Attacker.Moves.Insert(index, context.LastMove);
            context.Log($"{context.Attacker.Species.Name} learned {context.LastMove.Property.Name}!");
        }
        else
        {
            context.Log($"{context.Attacker.Species.Name} copied {context.LastMove.Property.Name}!");
            if (context.LastMove.Property.Name == "MirrorMove")
            {
                context.Log("But it failed!");
            }
            context.LastMove.Use(context);
        }
    }
}