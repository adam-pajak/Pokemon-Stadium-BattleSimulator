using PokemonStadium.Models.Enums;

namespace PokemonStadium.Models.DTO;

public class MoveEffectDto
{
    public EffectType? EffectType { get; init; }
    public bool? SemiInvulnerableTurn { get; init; }
    public bool? Replace { get; init; }
    public CriticalRatio? CriticalRatio { get; init; }
    public Stat? AffectedStat { get; init; }
    public int Percent { get; init; }
    public int? MinHits { get; init; }
    public int? MaxHits { get; init; }
    public int? FixedPower { get; init; }
    public Targets? Target { get; init; }
    public int? Chance { get; init; }
    public int? Stages { get; init; }
    public Status? Status { get; init; }
    public int? Duration { get; init; }
}