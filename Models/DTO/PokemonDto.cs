using PokemonStadium.Models.Enums;

namespace PokemonStadium.Models.DTO;

public class StatsDto
{
    public int Hp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int SpecialAttack { get; set; }
    public int SpecialDefense { get; set; }
    public int Speed { get; set; }
}

public class PokemonDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    
    public List<PokemonType> Types { get; set; } = new();

    public StatsDto BaseStats { get; set; } = null!;

    public List<string> Moves { get; set; } = new();
}