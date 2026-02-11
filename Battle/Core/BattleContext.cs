using PokemonStadium.Battle.Calculators;
using PokemonStadium.Models;
using PokemonStadium.Models.Moves;

namespace PokemonStadium.Battle.Core;

public class BattleContext
{
    public BattlePokemon Attacker = null!;
    public BattlePokemon Defender = null!;
    public BattleMove Move = null!;
    public Random Range = new();
    
    public int? LastDamage { get; set; } // counter (CounterDamageEffect)
    public BattleMove? LastMove { get; set; } // mimic, mirrorMove etc. (CopyMoveEffect)
    public required Dictionary<string, Move> AllMoves { get; init; } // metronome (RandomMoveEffect)
    private int _turn;
    public List<string> LogMessages { get; set; } = new();
    

    public void Refresh(BattlePokemon attacker, BattlePokemon defender, BattleMove move)
    {
        Attacker = attacker;
        Defender = defender;
        Move = move;
        Range = new Random();
    }
    public void Log(string message)
    {
        LogMessages.Add($"Turn {_turn}: {message}");
        Console.Clear();
        Program.SlowWrite(message);
    }

    private void CheckDisabledMoves(BattlePokemon pokemon)
    {
        var disabledMoves = pokemon.GetDisabledMoves();
        foreach (var move in disabledMoves)
        {
            if (move.UnDisable())
            {
                Log($"{pokemon.Species.Name}'s {Move.Property.Name} is no longer disabled!");
            }
        }
    }

    
    public void NextTurn()
    {
        if (_turn > 0)
        {
            CheckDisabledMoves(Attacker);
            CheckDisabledMoves(Defender);
        }
        _turn++;
    }
}
