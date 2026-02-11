using PokemonStadium.Models.Enums;

namespace PokemonStadium.Models.DTO;

public class MoveDto
{
    public required string Name { get; set; }
    public PokemonType Type { get; set; }
    public MoveCategory Category { get; set; }
    public int? Power { get; set; }
    public int? Accuracy { get; set; }
    public int Pp { get; set; }
    public int Priority { get; set; }
    public Targets Target { get; set; }
    public List<MoveEffectDto> Effects { get; set; } = new();
}
