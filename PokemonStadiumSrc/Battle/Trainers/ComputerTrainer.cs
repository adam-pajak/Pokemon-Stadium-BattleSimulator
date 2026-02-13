using PokemonStadium.Battle.Core;

namespace PokemonStadium.Battle.Trainers;

public class ComputerTrainer : Trainer
{
    public ComputerTrainer(string name, List<BattlePokemon> pokemons) : base(name, pokemons) { }

    public override BattleMove? ChooseAction()
    {
        if (LockChoice) return LastAction;
        return ChooseMove();
    }

    public override BattleMove ChooseMove()
    {
        int randomMove;
        do
        {
            randomMove = new Random().Next(0, ActivePokemon.Moves.Count);
        } while (!ActivePokemon.Moves[randomMove].CanBeUsed());
        return ActivePokemon.Moves[randomMove];
    }

    public override BattlePokemon SwitchPokemon()
    {
        int randomPokemon = new Random().Next(0, GetPokemonToSwitch().Count);
        return GetPokemonToSwitch()[randomPokemon];
    }
}