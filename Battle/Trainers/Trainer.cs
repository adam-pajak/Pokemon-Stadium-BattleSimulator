using PokemonStadium.Battle.Core;

namespace PokemonStadium.Battle.Trainers;

public abstract class Trainer
{
    public string Name;
    public List<BattlePokemon> Pokemons;
    public BattlePokemon ActivePokemon;
    public BattleMove? LastAction { get; set; }
    public bool LockChoice { get; set; }

    protected Trainer(string name, List<BattlePokemon> pokemons)
    {
        Name = name;
        Pokemons = pokemons;
        ActivePokemon = pokemons[0];
    }
    public abstract BattleMove? ChooseAction();
    public abstract BattleMove ChooseMove();
    public abstract BattlePokemon SwitchPokemon();
    public bool CanSwitchPokemon()
    {
        return GetPokemonToSwitch().Count > 0;
    }
    public List<BattlePokemon> GetPokemonToSwitch()
    {
        return Pokemons.FindAll(p => !p.IsFainted && p != ActivePokemon);
    }

    public void SetActivePokemon(BattlePokemon pokemon)
    {
        pokemon.SwitchedOut();
        ActivePokemon = pokemon;
    }

    
}