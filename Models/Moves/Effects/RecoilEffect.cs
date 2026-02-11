using PokemonStadium.Battle.Core;

namespace PokemonStadium.Models.Moves.Effects;

public class RecoilEffect : IMoveEffect
{
    private readonly int _percent;

    public RecoilEffect(int percent)
    {
        _percent = percent;
    }
    public void Apply(BattleContext context)
    {
        if (context.LastDamage is null) return;
        int damageDealt = context.LastDamage.Value;
        if (damageDealt <= 0) return;
        int recoil = damageDealt * _percent / 100;
        if (recoil <= 0) return;
        int damageTaken = context.Attacker.TakeDamage((byte)recoil);
        context.Log($"{context.Attacker.Species.Name}'s hit with {damageTaken} recoil!");
        if (context.Attacker.IsFainted) context.Log($"{context.Attacker.Species.Name} fainted.");
    }
}