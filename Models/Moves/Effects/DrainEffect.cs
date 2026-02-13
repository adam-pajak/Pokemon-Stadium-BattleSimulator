using PokemonStadium.Battle.Core;

namespace PokemonStadium.Models.Moves.Effects;

public class DrainEffect : IMoveEffect
{
    private readonly int _percent;

    public DrainEffect(int percent)
    {
        _percent = percent;
    }
    public void Apply(BattleContext context)
    {
        if (context.Attacker.ActivePokemon.Stats.MaxHp == context.Attacker.ActivePokemon.CurrentHp)
        {
            context.Log($"{context.Attacker.ActivePokemon.Species.Name} hp is full!");
            return;
        }
        if (context.LastDamage is null) return;
        int damageDealt = context.LastDamage.Value;
        if (damageDealt <= 0) return; 
        int heal = damageDealt * _percent / 100;
        if (heal <= 0) return;
        int restoredHp = context.Attacker.ActivePokemon.RestoreHp(heal);
        context.Log($"{context.Attacker.ActivePokemon.Species.Name} drained {restoredHp} energy!");
    }
}