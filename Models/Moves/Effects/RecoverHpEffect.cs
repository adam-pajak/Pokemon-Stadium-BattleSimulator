using PokemonStadium.Battle.Core;

namespace PokemonStadium.Models.Moves.Effects;

public class RecoverHpEffect : IMoveEffect
{
    private readonly int _percent;

    public RecoverHpEffect(int percent)
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
        int heal = context.Attacker.Stats.MaxHp * _percent / 100;
        int restoredHp = context.Attacker.RestoreHp(heal);
        context.Log($"{context.Attacker.Species.Name} recovered {restoredHp} hp.");
    }
}