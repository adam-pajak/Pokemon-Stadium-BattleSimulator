using PokemonStadium.Battle.Core;
using PokemonStadium.Models.Enums;

namespace PokemonStadium.Models.Moves.Effects;

public class StatusEffect : IMoveEffect
{
    private Targets _target;
    private double? _chance;
    private Status _status;

    public StatusEffect(Targets target, double? chance, Status status)
    {
        _target = target;
        _chance = chance;
        _status = status;
    }
    public void Apply(BattleContext context)
    {
        Console.WriteLine("Status effect is currently not implemented and can't be applied.");
        Thread.Sleep(2000);
    }
}