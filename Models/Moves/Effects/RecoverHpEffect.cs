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
        if (context.Attacker.ActivePokemon.Stats.MaxHp == context.Attacker.ActivePokemon.CurrentHp)
        {
            context.Log($"{context.Attacker.ActivePokemon.Species.Name} hp is full.");
            return;
        }
        int heal = context.Attacker.ActivePokemon.Stats.MaxHp * _percent / 100;
        int restoredHp = context.Attacker.ActivePokemon.RestoreHp(heal);
        context.Log($"{context.Attacker.ActivePokemon.Species.Name} recovered {restoredHp} hp.");
    }
}