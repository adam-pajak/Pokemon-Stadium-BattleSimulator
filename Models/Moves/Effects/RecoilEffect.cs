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
        int damageTaken = context.Attacker.ActivePokemon.TakeDamage((byte)recoil);
        context.Log($"{context.Attacker.ActivePokemon.Species.Name}'s hit with {damageTaken} recoil!");
        if (context.Attacker.ActivePokemon.IsFainted) context.Log($"{context.Attacker.ActivePokemon.Species.Name} fainted.");
    }
}