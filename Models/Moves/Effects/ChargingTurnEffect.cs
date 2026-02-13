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
        if (_semiInvulnerableTurn) context.Attacker.ActivePokemon.IsInvulnerable = !context.Attacker.ActivePokemon.IsInvulnerable;
        string moveName = context.Move.Property.Name;
        if (moveName == "HyperBeam") // przeładowanie: HyperBeam, GigaImpact, etc.
        {
            if (context.Attacker.ActivePokemon.IsRecharging)
            {
                context.Log($"{context.Attacker.ActivePokemon.Species.Name} {GetLogMessage(moveName)}");
                context.Attacker.LockChoice = false;
            }
            else context.Attacker.LockChoice = true;
            context.Attacker.ActivePokemon.IsRecharging = !context.Attacker.ActivePokemon.IsRecharging;
        }
        else
        {
            context.Attacker.ActivePokemon.IsCharging = !context.Attacker.ActivePokemon.IsCharging;
            if (context.Attacker.ActivePokemon.IsCharging)
            {
                context.Log($"{context.Attacker.ActivePokemon.Species.Name} {GetLogMessage(moveName)}");
                context.Attacker.LockChoice = true;
            }
            else context.Attacker.LockChoice = false;
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