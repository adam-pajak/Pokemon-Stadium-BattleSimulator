using PokemonStadium.Battle.Core;

namespace PokemonStadium.Models.Moves.Effects;

public class TransformEffect : IMoveEffect
{
    public TransformEffect() { }
    public void Apply(BattleContext context)
    {
        Console.WriteLine("Transform effect is currently not implemented and can't be applied.");
        Thread.Sleep(2000);
    }
}