using PokemonStadium.Battle.Core;
using PokemonStadium.Models.Enums;

namespace PokemonStadium.Models.Moves.Effects;

public class StatChangeEffect : IMoveEffect
{
    private readonly Targets _target;
    private readonly int? _chance;
    private readonly int _stages;
    private readonly Stat _affectedStat;

    public StatChangeEffect(Targets target, int? chance, int stages, Stat affectedStat)
    {
        _target = target;
        _chance = chance;
        _stages = stages;
        _affectedStat = affectedStat;
    }
    public void Apply(BattleContext context)
    {
        bool occured = IMoveEffect.CheckWhetherAdditionalEffectOccured(_chance, context);
        if (occured)
        {
            BattlePokemon target;
            switch (_target)
            {
                case Targets.Enemy:
                    target = context.Defender.ActivePokemon;
                    break;
                case Targets.Self:
                    target = context.Attacker.ActivePokemon;
                    break;
                case Targets.All:
                    throw new NotImplementedException("Stat change effect currently works only on one target.");
                default:
                    throw new InvalidDataException($"Invalid target type: {_target}");
            }
            int prevStage, curStage;
            prevStage = target.StatStages.GetStage(_affectedStat);
            target.StatStages.ChangeStat(_affectedStat, _stages);
            curStage = target.StatStages.GetStage(_affectedStat);
            string msg = target.StatStages.GetLogMessage(_affectedStat, prevStage, curStage);
            context.Log($"{target.Species.Name}'s {msg}");
        }
    }
}