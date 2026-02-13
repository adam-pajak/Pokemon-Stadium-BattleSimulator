using System.Reflection.Metadata;
using PokemonStadium.Battle.Calculators;
using PokemonStadium.Models.Enums;
using PokemonStadium.Models.Moves;
using PokemonStadium.Models.Moves.Effects;

namespace PokemonStadium.Battle.Core;


public class BattleMove
{
    public readonly Move Property;
    private int _currentPp; 
    private int _maxPp;
    public bool IsDisabled = false;
    private int _disableDuration = 0;
    
    public BattleMove(Move move)
    {
        Property = move;
        _currentPp = Property.Pp;
        _maxPp = Property.Pp;
    }
    
    public void Show()
    {
        Console.Write($"{Property.Name}");
        if (IsDisabled)
        {
            Console.WriteLine(" | DISABLED");
        }
        else
        {
            Console.WriteLine($" | {_currentPp}/{_maxPp} PP");
        }
    }
    public void Use(BattleContext context)
    {
        context.Move = this; // Nadpisujemy, aby upewnić się, że context zawiera prawidłowe informacje do przekazania do efektów
        if (!context.Attacker.ActivePokemon.IsRecharging)
        {
            context.Log($"{context.Attacker.ActivePokemon.Species.Name} used {Property.Name}!");
            if (CanBeUsed())
            {
                bool hit = AccuracyCalculator.DoesMoveHit(context);
                if (hit)
                {
                    foreach (var effect in Property.Effects)
                    {
                        effect.Apply(context);
                        if (context.Attacker.ActivePokemon.IsCharging) break;
                    }
                    if (context.Move.Property.Category == MoveCategory.Status) context.LastDamage = null;
                }
                else // ChargingTurnEffect oraz SelfDestructEffect wykonują się zawsze nawet jak ruch nie trafi
                {
                    List<IMoveEffect> alwaysEffects = Property.Effects.Where(e => e is ChargingTurnEffect or SelfdestructEffect).ToList();
                    foreach (var alwaysEffect in alwaysEffects)
                    {
                        alwaysEffect.Apply(context);
                        if (context.Attacker.ActivePokemon.IsCharging) break;
                    }
                    // Pomijamy wiadomość i nie wykorzystujemy PP tylko wtedy, gdy jest to pierwsza tura ładowanego ataku
                    if (alwaysEffects.Any(e => e is ChargingTurnEffect) && context.Attacker.ActivePokemon.IsCharging)
                    {
                        _currentPp++;
                    }
                    else context.Log($"{context.Defender.ActivePokemon.Species.Name} avoided the attack!");

                    context.LastDamage = null;
                }
                context.LastMove = this;
                _currentPp--;
            }
        }
        else
        {
            // gdy pokemon jest w stanie przeładowania, to pomijamy i wykonujemy tylko ostatni efekt z listy (którym w tym przypadku zawsze jest ChargingTurnEffect)
            Property.Effects.Last().Apply(context);
        }
    }

    public void Disable(int duration)
    {
        _disableDuration = duration;
        IsDisabled = true;
    }

    public bool UnDisable()
    {
        _disableDuration--;
        if (_disableDuration <= 0) IsDisabled = false;
        return IsDisabled;
    }
    public bool CanBeUsed()
    {
        if (IsDisabled)
        {
            Console.WriteLine($"{Property.Name} is disabled!");
            return false;
        }
        if (_currentPp <= 0)
        {
            Console.WriteLine($"You have no PP left on {Property.Name}");
            return false;
        }
        return true;
    }
}