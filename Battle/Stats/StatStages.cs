
using PokemonStadium.Models.Enums;

namespace PokemonStadium.Battle.Stats;

public class StatStages
{
    public readonly Dictionary<Stat, int> Stages = new()
    {
        [Stat.Attack] = 0,
        [Stat.Defense] = 0,
        [Stat.SpAttack] = 0,
        [Stat.SpDefense] = 0,
        [Stat.Speed] = 0,
        [Stat.Accuracy] = 0,
        [Stat.Evasion] = 0
    };
    

    public int GetStage(Stat stat)
    {
        if (!Stages.TryGetValue(stat, out var value))
            throw new InvalidDataException($"Stat [{stat}] not found in Stages dictionary.");
        return value;
    }
    public void ChangeStat(Stat stat, int stages)
    {
        int value = GetStage(stat);
        value = Math.Clamp(value + stages, -6, 6);
        Stages[stat] = value;
    }

    public void Reset()
    {
        foreach (var key in Stages.Keys.ToList())
        {
            Stages[key] = 0;
        }

    }
    
    public string GetLogMessage(Stat stat, int prevStage, int curStage)
    {
        return (curStage - prevStage) switch
        {
            >= 3 => $"{stat} rose drastically!",
            2 => $"{stat} rose sharply!",
            1 => $"{stat} rose!",
            0 => $"{stat} cannot go any " + (curStage == 6 ? "higher!" : "lover!"),
            -1 => $"{stat} fell!",
            <= -2 => $"{stat} harshly fell!"
        };
    }
}
