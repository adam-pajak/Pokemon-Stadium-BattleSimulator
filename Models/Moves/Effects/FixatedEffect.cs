using PokemonStadium.Battle.Core;

namespace PokemonStadium.Models.Moves.Effects;

public class FixatedEffect : IMoveEffect
{
    private int _minHits;
    private int _maxHits;
    public FixatedEffect(int minHits, int maxHits)
    {
        _minHits = minHits;
        _maxHits = maxHits;
    }
    public void Apply(BattleContext context)
    {
        Console.WriteLine("Fixated effect is currently not implemented and can't be applied.");
        Thread.Sleep(2000);
    }
}