namespace PokemonStadium.Models.Pokemon;

public class BaseStats
{
    public required int Hp { get; init; }
    public required int Attack { get; init; }
    public required int Defense { get; init; }
    public required int SpAttack { get; init; }
    public required int SpDefense { get; init; }
    public required int Speed { get; init; }
}