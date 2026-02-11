using PokemonStadium.Battle.Core;

namespace PokemonStadium.Models.Moves.Effects;

public class LeechSeedEffect : IMoveEffect
{
    public LeechSeedEffect() { }
    public void Apply(BattleContext context)
    {
        Console.WriteLine("Leech Seed effect is currently not implemented and can't be applied.");
        Thread.Sleep(2000);
    }
}