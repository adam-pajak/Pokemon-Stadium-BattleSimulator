using PokemonStadium.Battle.Core;

namespace PokemonStadium.Models.Moves.Effects;

public interface IMoveEffect
{
    void Apply(BattleContext context);
    

    public static bool CheckWhetherAdditionalEffectOccured(int? chance, BattleContext context)
    {
        if (chance is null) return true;
        chance = chance * 255 / 100;
        int roll = context.Range.Next(256);
        return roll <= chance;
    }
}