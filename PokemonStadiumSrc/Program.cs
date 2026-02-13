using PokemonStadium.Battle.Core;
using PokemonStadium.Battle.Trainers;
using PokemonStadium.Models.Moves;
using PokemonStadium.Models.Pokemon;
using PokemonStadium.Services;

namespace PokemonStadium;

internal static class Program
{
    public static void SlowWrite(string text)
    {
        // wypisywanie litera po literze z opóźnieniem
        foreach (char c in text)
        {
            Console.Write(c);
            Thread.Sleep(100);
        }
        Thread.Sleep(500);
        Console.WriteLine();
    }
    static List<BattlePokemon> ReturnChosenPokemon(string name, int number, int level, List<Pokemon> pokemons)
    {
        Console.WriteLine("Choose Pokémon from the list above (by number): ");
        var result = new HashSet<Pokemon>();
        do
        {
            int id;
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out id))
            {
                Console.WriteLine("Invalid input. Try again.");
            }
            Pokemon? pokemon = pokemons.FirstOrDefault(p => p.Id == id);
            if (pokemon == null)
            {
                Console.WriteLine("Pokemon not found. Try again.");
                continue;
            }
            result.Add(pokemon);
            Console.WriteLine($"{pokemon.Name} has been added to {name}'s team.");
        } while (result.Count < number);
        return result.Select(p => new BattlePokemon(p, level)).ToList();
    }
    
    
    static void Main(string[] args)
    {
        var moves = DataLoader.LoadMoves("Data/moves_gen1.json");
        var pokemons = DataLoader.LoadPokemons("Data/pokemon_gen1.json", moves);

        Console.Clear();
        Console.WriteLine($"# Loaded {pokemons.Count} Pokémon and {moves.Count} moves. #");
        
        Console.WriteLine("Welcome to Pokémon Stadium!");
        string? input;
        Console.WriteLine("What's your name?");
        do
        {
            input = Console.ReadLine();
        } while (input is null);
        string name = input;
        Console.WriteLine("Enter your opponent's name: ");
        do
        {
            input = Console.ReadLine();
        } while (input is null);
        string opponentName = input;
        
        Console.WriteLine("Choose battle rules: ");
        Console.WriteLine("1. How many Pokemon would you like to get (from 1 to 6)?");
        int number;
        do
        {
            input = Console.ReadLine();
        } while (!int.TryParse(input, out number)  || number < 1 || number > 6);

        int level;
        Console.WriteLine("2. At what level will they be (1-100)?");
        do
        {
            input = Console.ReadLine();
        } while (!int.TryParse(input, out level)  || level < 1 || level > 100);
        
        foreach (var pokemon in pokemons)
        {
            pokemon.Show();
        }
        // wybieranie drużyny
        Console.WriteLine("3. Choose your team.");
        var playerPokemon = ReturnChosenPokemon(name, number, level, pokemons);
        Console.WriteLine("4. Choose opponent's team.");
        var computerPokemon = ReturnChosenPokemon(opponentName, number, level, pokemons);
        
        
        Trainer player = new PlayerTrainer (name, playerPokemon);
        Trainer computer = new ComputerTrainer (opponentName, computerPokemon);
        Console.WriteLine("Battle between: ");
        Console.WriteLine($"Team {player.Name}");
        player.Pokemons.ForEach( p => Console.WriteLine($"- {p.Species.Name}"));
        Console.WriteLine($"Team {computer.Name}");
        computer.Pokemons.ForEach( p => Console.WriteLine($"- {p.Species.Name}"));
        Console.CursorVisible = false;
        Thread.Sleep(5000);
        Console.Clear();
        Console.CursorVisible = true;
        SlowWrite($"{computer.Name} wants to fight!");
        Game.Context = new BattleContext {AllMoves = moves};
        while (Game.PlayRound(player, computer)) ;
        Console.WriteLine("Would you like to review the battle?");
        Console.WriteLine("1. Yes | 2. No");
        do
        {
            input = Console.ReadLine();
        } while (!int.TryParse(input, out number) || number <  1 || number > 2);
        Console.Clear();
        if (number == 1)
        {
            foreach (var msg in Game.Context.LogMessages)
            {
                SlowWrite(msg);
            }
        }
    }
}