using PokemonStadium.Battle.Core;

namespace PokemonStadium.Models.Moves.Effects;

public class StatChangeResetEffect : IMoveEffect
{
    public StatChangeResetEffect() { }
    public void Apply(BattleContext context)
    {
        context.Attacker.StatStages.Reset();
        context.Defender.StatStages.Reset();
        context.Log("All stat changes have been reset!");
    }
}