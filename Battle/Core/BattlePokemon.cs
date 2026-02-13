using PokemonStadium.Battle.Stats;
using PokemonStadium.Models.Enums;
using PokemonStadium.Models.Pokemon;

namespace PokemonStadium.Battle.Core;

public class BattlePokemon
{
    public readonly Pokemon Species;
    public readonly int Level;
    public readonly BattleStats Stats;
    public readonly StatStages StatStages;
    public int CurrentHp;
    public bool IsFainted;
    public List<PokemonType> Types;
    public List<BattleMove> Moves;
    public bool IsInvulnerable;
    public bool IsCharging;
    public bool IsRecharging;
    
    public BattlePokemon(Pokemon pokemon, int level)
    {
        Species = pokemon;
        Types = pokemon.Types;
        Level = level;
        Stats = StatCalculator.Calculate(Species, Level);
        StatStages = new StatStages();
        IsFainted = false;
        Moves = pokemon.Moves.Select(m => new BattleMove(m)).ToList();
        CurrentHp = Stats.MaxHp;
        IsInvulnerable = false;
        IsCharging = false;
        IsRecharging = false;
    }

    public void DisplayInBattleInformation()
    {
        Console.Write($"{Species.Name} | LVL: {Level} | HP: ");
        Console.ForegroundColor = ConsoleColor.Green;
        if (CurrentHp <= Stats.MaxHp / 2) Console.ForegroundColor = ConsoleColor.Yellow;
        if (CurrentHp < Stats.MaxHp / 5) Console.ForegroundColor = ConsoleColor.Red;
        
        Console.WriteLine($"{CurrentHp}/{Stats.MaxHp}");
        Console.ResetColor();
    }

    public void SwitchedOut()
    {
        StatStages.Reset();
        Types = Species.Types;
        Moves = Species.Moves.Select(m => new BattleMove(m)).ToList();
        IsInvulnerable = false;
        IsCharging = false;
        IsRecharging = false;
    }

    public List<BattleMove> GetDisabledMoves() => Moves.FindAll(move => move.IsDisabled);
    
    
    public int TakeDamage(byte damage)
    {
        int maxDamage = CurrentHp;
        int damageTaken = Math.Min(maxDamage, damage);
        CurrentHp -= damageTaken;
        IsFainted = CurrentHp <= 0;
        return damageTaken;
    }
    public void Knockout()
    {
        CurrentHp = 0;
        IsFainted = true;
    }
    public int RestoreHp(int hp)
    {
        int maxRecovery = Stats.MaxHp - CurrentHp;
        int recovery = Math.Min(hp, maxRecovery);
        CurrentHp += recovery;
        return recovery;
    }
    
    public int GetAttack(bool criticalHit)
    {
        if (criticalHit) return Stats.Attack;
        Fraction multiplier = StatStageMultipliers.GetMultiplier(StatStages.Stages[Stat.Attack]);
        return Stats.Attack * multiplier.Numerator / multiplier.Denominator;

    }

    public int GetDefense(bool criticalHit)
    {
        if (criticalHit) return Stats.Defense;
        Fraction multiplier = StatStageMultipliers.GetMultiplier(StatStages.Stages[Stat.Defense]);
        return Stats.Defense * multiplier.Numerator / multiplier.Denominator;

    }
    public int GetSpAttack(bool criticalHit)
    {
        if (criticalHit) return Stats.SpAttack;
        Fraction multiplier = StatStageMultipliers.GetMultiplier(StatStages.Stages[Stat.SpAttack]);
        return Stats.SpAttack * multiplier.Numerator / multiplier.Denominator;

    }

    public int GetSpDefense(bool criticalHit)
    {
        if (criticalHit) return Stats.SpDefense;
        Fraction multiplier = StatStageMultipliers.GetMultiplier(StatStages.Stages[Stat.SpDefense]);
        return Stats.SpDefense * multiplier.Numerator / multiplier.Denominator;

    }

    public int GetSpeed()
    {
        Fraction multiplier = StatStageMultipliers.GetMultiplier(StatStages.Stages[Stat.Speed]);
        return Stats.Speed * multiplier.Numerator / multiplier.Denominator;
    }

    public int GetAccuracy(int moveAccuracy)
    {
        Fraction multiplier = StatStageMultipliers.GetAccuracyEvasionMultiplier(StatStages.Stages[Stat.Accuracy], Stat.Accuracy);
        return moveAccuracy * multiplier.Numerator / multiplier.Denominator;
    }

    public int GetEvasion(int moveAccuracy)
    {
        Fraction multiplier = StatStageMultipliers.GetAccuracyEvasionMultiplier(StatStages.Stages[Stat.Evasion], Stat.Evasion);
        return moveAccuracy * multiplier.Numerator / multiplier.Denominator;
    }
    
}