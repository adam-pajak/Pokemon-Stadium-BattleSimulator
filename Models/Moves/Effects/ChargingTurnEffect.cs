using PokemonStadium.Battle.Core;

namespace PokemonStadium.Models.Moves.Effects;

public class ChargingTurnEffect : IMoveEffect
{
    private readonly bool _semiInvulnerableTurn;
    public ChargingTurnEffect(bool semiInvulnerableTurn)
    {
        _semiInvulnerableTurn = semiInvulnerableTurn;
    }
    public void Apply(BattleContext context)
    {
        if (_semiInvulnerableTurn) context.Attacker.IsInvulnerable = !context.Attacker.IsInvulnerable;
        string moveName = context.Move.Property.Name;
        if (moveName == "HyperBeam") // przeładowanie: HyperBeam, GigaImpact, etc.
        {
            if (context.Attacker.IsRecharging) context.Log($"{context.Attacker.Species.Name} {GetLogMessage(moveName)}");
            context.Attacker.IsRecharging = !context.Attacker.IsRecharging;
        }
        else
        {
            context.Attacker.IsCharging = !context.Attacker.IsCharging;
            if (context.Attacker.IsCharging) context.Log($"{context.Attacker.Species.Name} {GetLogMessage(moveName)}");
        }
    }
    private string GetLogMessage(string moveName)
    {
        return moveName switch
        {
            "Dig" => "burrowed its way under the ground!",
            "Fly" => "flew up high!",
            "HyperBeam" => "must recharge!",
            "RazorWind" => "made a whirlwind!",
            "SkullBash" => "lowered its head!",
            "SkyAttack" => "is glowing!",
            "SolarBeam" => "absorbed light!",
            _ => ""
        };
    }
}