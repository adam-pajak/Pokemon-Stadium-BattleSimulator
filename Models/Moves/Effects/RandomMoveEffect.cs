using PokemonStadium.Battle.Core;

namespace PokemonStadium.Models.Moves.Effects;

public class RandomMoveEffect : IMoveEffect
{
    public RandomMoveEffect() { }
    public void Apply(BattleContext context)
    {
        var moves = context.AllMoves.Values.Where(m => m.Name != "Metronome").ToList();
        int index = context.Range.Next(moves.Count);
        var randomMove = new BattleMove(moves[index]);
        randomMove.Use(context);
    }
}