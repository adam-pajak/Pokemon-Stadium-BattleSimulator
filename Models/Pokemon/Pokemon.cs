using PokemonStadium.Models.Enums;
using PokemonStadium.Models.Moves;

namespace PokemonStadium.Models.Pokemon;
public class Pokemon
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required List<PokemonType> Types { get; init; }
    public required BaseStats BaseStats { get; init; }
    public required List<Move> Moves { get; init; }

    public void Show()
    {
        Console.WriteLine($"#{Id}: {Name}");
    }
}