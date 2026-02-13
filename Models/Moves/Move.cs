using PokemonStadium.Models.Enums;
using PokemonStadium.Models.Moves.Effects;

namespace PokemonStadium.Models.Moves;

public class Move
{
    public required string Name { get; init; }
    public required PokemonType Type { get; init; }
    public required MoveCategory Category { get; init; }
    public required int? Power { get; init; }
    public required int? Accuracy { get; init; }
    public required int Pp { get; init; }
    public required int Priority { get; init; }
    public required Targets Target { get; init; }
    public required List<IMoveEffect> Effects { get; init; }
}

