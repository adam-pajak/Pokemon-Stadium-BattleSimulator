using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using PokemonStadium.Models;          
using PokemonStadium.Models.Enums;
using PokemonStadium.Models.DTO;
using PokemonStadium.Models.Moves;
using PokemonStadium.Models.Moves.Effects;
using PokemonStadium.Models.Pokemon;

namespace PokemonStadium.Services
{
    public static class DataLoader
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        // ===== RUCHY =====

        public static Dictionary<string, Move> LoadMoves(string path)
        {
            var json = File.ReadAllText(path);
            var moveDtos = JsonSerializer.Deserialize<List<MoveDto>>(json, JsonOptions)
                           ?? throw new Exception("Cannot deserialize moves");

            return moveDtos.ToDictionary(
                dto => dto.Name,
                dto => MapMove(dto),
                StringComparer.OrdinalIgnoreCase
            );
        }

        private static Move MapMove(MoveDto dto)
        {
            var move = new Move
            {
                Name = dto.Name,
                Type = dto.Type,
                Category = dto.Category,
                Power = dto.Power,
                Accuracy = dto.Accuracy,
                Pp = dto.Pp,
                Priority = dto.Priority,
                Target = dto.Target,
                Effects = new List<IMoveEffect>()
            };

            move.Effects.AddRange(
                dto.Effects.Select(e => CreateEffect(e, move))
            );

            return move;
        }


        // ===== POKÉMONY =====

        public static List<Pokemon> LoadPokemons(string path, Dictionary<string, Move> movesByName)
        {
            var json = File.ReadAllText(path);
            var pokemonDtos = JsonSerializer.Deserialize<List<PokemonDto>>(json, JsonOptions)
                               ?? throw new Exception("Cannot deserialize pokemons");

            var result = new List<Pokemon>();

            foreach (var dto in pokemonDtos)
            {
                var pokemonMoves = dto.Moves.Select(moveName =>
                {
                    if (!movesByName.TryGetValue(moveName, out var move))
                        throw new Exception(
                            $"Pokemon '{dto.Name}' references unknown move '{moveName}'");

                    return move;
                }).ToList();
                
                var stats = new BaseStats
                {
                    Hp = dto.BaseStats.Hp,
                    Attack = dto.BaseStats.Attack,
                    Defense = dto.BaseStats.Defense,
                    SpAttack = dto.BaseStats.SpecialAttack,
                    SpDefense = dto.BaseStats.SpecialDefense,
                    Speed = dto.BaseStats.Speed
                };

                var pokemon = new Pokemon
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Types = dto.Types,
                    BaseStats = stats,
                    Moves = pokemonMoves
                };

                result.Add(pokemon);
            }

            return result;
        }

        // ===== EFEKTY RUCHÓW =====

        private static IMoveEffect CreateEffect(MoveEffectDto dto, Move move)
        {
            switch (dto.EffectType)
            {
                case EffectType.ChargingTurn:
                    if (dto.SemiInvulnerableTurn is null)
                    {
                        throw new InvalidDataException("SemiInvulnerableTurn cannot be null while EffectType is ChargingTurn");
                    }
                    return new ChargingTurnEffect(dto.SemiInvulnerableTurn.Value);
                case EffectType.CopyMove:
                    if (dto.Replace is null)
                    {
                        throw new InvalidDataException("Replace cannot be null while EffectType is CopyMove");
                    }
                    return new CopyMoveEffect(dto.Replace.Value);
                case EffectType.CounterDamage: return new CounterDamageEffect(move.Category);
                case EffectType.CutHalfHp: return new CutHalfHpEffect();
                case EffectType.Damage:
                    if (move.Power is null || dto.CriticalRatio is null)
                    {
                        throw new InvalidDataException("Power or CriticalRatio cannot be null while EffectType is Damage");
                    }
                    return new DamageEffect(dto.CriticalRatio.Value);
                case EffectType.DamageReduction:
                    if (dto.AffectedStat is null || dto.Duration is null)
                    {
                        throw new InvalidDataException("AffectedStat or Duration cannot be null while EffectType is DamageReduction");
                    }
                    return new DamageReductionEffect(dto.AffectedStat.Value, dto.Duration.Value);
                case EffectType.Disable: return new DisableEffect();
                case EffectType.Drain: return new DrainEffect(dto.Percent);
                case EffectType.Fixated:
                    if (dto.MinHits is null || dto.MaxHits is null)
                    {
                        throw new InvalidDataException("MinHits or MaxHits cannot be null while EffectType is Fixated");
                    }
                    return new FixatedEffect(dto.MinHits.Value, dto.MaxHits.Value);
                case EffectType.FixedDamage: return new FixedDamageEffect(dto.FixedPower);
                case EffectType.LeechSeed: return new LeechSeedEffect();
                case EffectType.Multistrike:
                    if (dto.MinHits is null || dto.MaxHits is null || dto.CriticalRatio is null || move.Power is null)
                    {
                        throw new InvalidDataException("MinHits, MaxHits, CriticalRatio or Power cannot be null while EffectType is Multistrike");
                    }

                    return new MultistrikeEffect(dto.CriticalRatio.Value, dto.MinHits.Value,
                        dto.MaxHits.Value);
                case EffectType.OneHitKo: return new OneHitKoEffect();
                case EffectType.RandomMove: return new RandomMoveEffect();
                case EffectType.Recoil: return new RecoilEffect(dto.Percent);
                case EffectType.RecoverHp: return new RecoverHpEffect(dto.Percent);
                case EffectType.Selfdestruct: return new SelfdestructEffect();
                case EffectType.StatChange:
                    if (dto.Target is null || dto.Stages is null || dto.AffectedStat is null)
                    {
                        throw new InvalidDataException("Target (of effect), Stages or AffectedStat cannot be null while EffectType is StatChange");
                    }
                    return new StatChangeEffect(dto.Target.Value, dto.Chance, dto.Stages.Value, dto.AffectedStat.Value);
                case EffectType.StatChangeReset: return new StatChangeResetEffect();
                case EffectType.Status:
                    if (dto.Target is null || dto.Status is null)
                    {
                        throw new InvalidDataException("Target (of effect) or Status cannot be null while EffectType is Status");
                    }
                    return new StatusEffect(dto.Target.Value, dto.Chance, dto.Status.Value);
                case EffectType.Transform: return new TransformEffect();
                case EffectType.TypeChange: return new TypeChangeEffect();
                default: throw new NotSupportedException("Unknown EffectType");
            }
        }
    }
}
