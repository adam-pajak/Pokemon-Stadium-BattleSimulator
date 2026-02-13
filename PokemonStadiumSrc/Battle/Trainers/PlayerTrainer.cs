using PokemonStadium.Battle.Core;

namespace PokemonStadium.Battle.Trainers;

public class PlayerTrainer : Trainer
{
    public PlayerTrainer(string name, List<BattlePokemon> pokemons) : base(name, pokemons) { }

    
    public override BattleMove? ChooseAction()
    {
        if (LockChoice)
        {
            Console.CursorVisible = false;
            Thread.Sleep(2000);
            Console.CursorVisible = true;
            return LastAction;
        }
        if (CanSwitchPokemon())
        {
            Console.WriteLine("1. Battle | 2. Switch Pokemon");
            string? choice;
            int choiceNumber;
            do
            {
                choice = Console.ReadLine();
            } while (!int.TryParse(choice, out choiceNumber) || choiceNumber < 1 || choiceNumber > 2);

            if (choiceNumber == 2)
            {
                return null;
            }
        }
        return ChooseMove();
    }

    public override BattleMove ChooseMove()
    {
        Console.WriteLine($"What {ActivePokemon.Species.Name} will do?");
        int i = 1;
        foreach (var move in ActivePokemon.Moves)
        {
            Console.Write($"{i++}. ");
            move.Show();
        }
        string? choice;
        int choiceNumber;
        do
        {
            do
            {
                choice = Console.ReadLine();
            } while (!int.TryParse(choice, out choiceNumber) || choiceNumber < 1 || choiceNumber >= i);
        } while (!ActivePokemon.Moves[choiceNumber - 1].CanBeUsed());

        return ActivePokemon.Moves[choiceNumber - 1];
    }

    public override BattlePokemon SwitchPokemon()
    {
        Console.WriteLine("Choose Pokemon to switch: ");
        Console.WriteLine($"Active Pokemon: {ActivePokemon.Species.Name}");
        int i = 1;
        GetPokemonToSwitch().ForEach(p => Console.WriteLine($"{i++} {p.Species.Name}"));
        string? choice;
        int choiceNumber;
        do
        {
            choice = Console.ReadLine();
        } while (!int.TryParse(choice, out choiceNumber) || choiceNumber < 1 || choiceNumber >= i);
        
        return GetPokemonToSwitch()[choiceNumber - 1];
    }
}