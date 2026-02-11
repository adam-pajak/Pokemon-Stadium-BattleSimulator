using PokemonStadium.Battle.Core;
using PokemonStadium.Models.Enums;

namespace PokemonStadium.Models.Moves.Effects;

public class DamageReductionEffect : IMoveEffect
{
    private Stat _affectedStat;
    private int _duration;
    public DamageReductionEffect(Stat affectedStat, int duration)
    {
        _affectedStat = affectedStat;
        _duration = duration;
    }
    public void Apply(BattleContext context)
    {
        Console.WriteLine("Damage reduction effect is currently not implemented and can't be applied.");
        Thread.Sleep(2000);
    }
}