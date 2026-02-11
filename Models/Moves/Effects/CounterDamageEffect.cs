using PokemonStadium.Battle.Core;
using PokemonStadium.Models.Enums;

namespace PokemonStadium.Models.Moves.Effects;

public class CounterDamageEffect : IMoveEffect
{
    private readonly MoveCategory _category;
    
    public CounterDamageEffect(MoveCategory category)
    {
        _category = category;
    }
    public void Apply(BattleContext context)
    {
        if (context.LastDamage is null || context.LastMove is null || context.LastMove.Property.Category != _category)
        {
            context.Log("But it failed!");
            return;
        }
        context.Log($"{context.Attacker.Species.Name} countered {context.LastMove.Property.Name}!");
        int takenDamage = context.Defender.TakeDamage((byte)(context.LastDamage * 2));
        context.Log($"{context.Defender.Species.Name} received {takenDamage} damage!");
        if (context.Defender.IsFainted) context.Log($"{context.Defender.Species.Name} fainted!");
    
    }
}