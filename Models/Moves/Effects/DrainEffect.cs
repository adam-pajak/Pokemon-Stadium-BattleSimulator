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
        if (context.Attacker.Stats.MaxHp == context.Attacker.CurrentHp)
        {
            context.Log($"{context.Attacker.Species.Name} hp is full.");
            return;
        }
        if (context.LastDamage is null) return;
        int damageDealt = context.LastDamage.Value;
        if (damageDealt <= 0) return; 
        int heal = damageDealt * _percent / 100;
        if (heal <= 0) return;
        int restoredHp = context.Attacker.RestoreHp(heal);
        context.Log($"{context.Attacker.Species.Name} drained {restoredHp} energy.");
    }
}