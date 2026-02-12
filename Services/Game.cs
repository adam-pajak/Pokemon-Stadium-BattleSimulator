using PokemonStadium.Battle.Core;
using PokemonStadium.Battle.Trainers;

namespace PokemonStadium.Services;

public static class Game
{
    public static BattleContext Context { get; set; } = null!;

    public static bool PlayRound(Trainer playerA, Trainer playerB)
    {
        
        Context.NextTurn();
        SwitchPokemonIfRequired(playerA);
        SwitchPokemonIfRequired(playerB);
        Console.Clear();
        Console.Write($"{playerA.Name}: ");
        playerA.ActivePokemon.DisplayInBattleInformation();
        Console.Write($"{playerB.Name}: ");
        playerB.ActivePokemon.DisplayInBattleInformation();
        BattleMove? movePlayerA = ChooseBattleAction(playerA);
        BattleMove? movePlayerB = ChooseBattleAction(playerB);
        AssignAndExecuteOrder(playerA, playerB, movePlayerA, movePlayerB);
        if (HasPlayerLost(playerA))
        {
            Console.WriteLine($"You lost with {playerB.Name}");
            return false;
        }
        if (HasPlayerLost(playerB))
        {
            Console.WriteLine($"You won with {playerB.Name}");
            return false;
        }
        return true;
    }
    private static BattleMove? ChooseBattleAction(Trainer player)
    {
        BattleMove? move = player.ChooseAction();
        return move;
    }
    private static void AssignAndExecuteOrder(Trainer playerA, Trainer playerB, BattleMove? movePlayerA, BattleMove? movePlayerB)
    {
        int priorityA, priorityB;
        Trainer first, second;
        BattleMove? firstMove, secondMove;
        priorityA = movePlayerA is null ? 10 : movePlayerA.Property.Priority;
        priorityB = movePlayerB is null ? 10 : movePlayerB.Property.Priority;
        if (priorityA > priorityB)
        {
            first = playerA;
            firstMove = movePlayerA;
            second = playerB;
            secondMove = movePlayerB;
        }
        else if (priorityA < priorityB)
        {
            first = playerB;
            firstMove = movePlayerB;
            second = playerA;
            secondMove = movePlayerA;
        }
        else // priorityA = priorityB
        {
            int speedA = playerA.ActivePokemon.GetSpeed();
            int speedB = playerB.ActivePokemon.GetSpeed();
            if (speedA > speedB)
            {
                first = playerA;
                firstMove = movePlayerA;
                second = playerB;
                secondMove = movePlayerB;
            }
            else if (speedA < speedB)
            {
                first = playerB;
                firstMove = movePlayerB;
                second = playerA;
                secondMove = movePlayerA;
            }
            else
            {
                Random t = new Random();
                if (t.Next(1, 2) == 1)
                {
                    first = playerA;
                    firstMove = movePlayerA;
                    second = playerB;
                    secondMove = movePlayerB;
                }
                else
                {
                    first = playerB;
                    firstMove = movePlayerB;
                    second = playerA;
                    secondMove = movePlayerA;
                }
            }
        }

        if (firstMove is not null && secondMove is not null)
        {
            AttackAction(firstMove, first, second);
            AttackAction(secondMove, second, first);

        }
        else if (firstMove is null && secondMove is not null)
        {
            SwitchAction(first);
            AttackAction(secondMove, second, first);
        }
        else // first and second moves are null (switch)
        {
            SwitchAction(first);
            SwitchAction(second);
        }
        
    }
    private static bool HasPlayerLost(Trainer player)
    {
        return player.Pokemons.FindAll(p => !p.IsFainted).Count == 0;
    }

    private static void AttackAction(BattleMove move, Trainer attacker, Trainer defender)
    {
        Context.Refresh(attacker, defender, move);
        if (!SwitchPokemonIfRequired(attacker))
        {
            Context.Move.Use(Context);
            attacker.LastAction = Context.LastMove;
        }
    }

    private static void SwitchAction(Trainer player)
    {
        var newActivePokemon = player.SwitchPokemon();
        if (!player.ActivePokemon.IsFainted) Context.Log($"{player.ActivePokemon.Species.Name} return!");
        player.SetActivePokemon(newActivePokemon);
        Context.Log($"Go {player.ActivePokemon.Species.Name}!");
        Context.LastMove = null;
        Context.LastDamage = null;
    }
    private static bool SwitchPokemonIfRequired(Trainer player)
    {
        if (player.ActivePokemon.IsFainted && player.CanSwitchPokemon())
        {
            SwitchAction(player);
            return true;
        }
        return player.ActivePokemon.IsFainted;
    }
    
}