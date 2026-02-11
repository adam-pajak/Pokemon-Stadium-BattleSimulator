using PokemonStadium.Battle.Calculators;
using PokemonStadium.Battle.Core;

namespace PokemonStadium.Models.Moves.Effects;

public class FixedDamageEffect : IMoveEffect
{
    private readonly int? _fixedPower;

    public FixedDamageEffect(int? fixedPower)
    {
        _fixedPower = fixedPower;
    }
    public void Apply(BattleContext context)
    {
        int fixedDamage;
        if (_fixedPower is null) fixedDamage = context.Attacker.Level;
        else fixedDamage = _fixedPower.Value;
        int damageTaken = context.Defender.TakeDamage((byte)fixedDamage);
        if (damageTaken > 0)
        {
            context.Log($"{context.Defender.Species.Name} received {damageTaken} damage!");
        }
        if (context.Defender.IsFainted) context.Log($"{context.Defender.Species.Name} fainted.");
        context.LastDamage = damageTaken;
    }
}