using System.Linq;
using PoGo.PokeMobBot.Logic.Event;
using System.Threading.Tasks;
using PokemonGo.RocketAPI;

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class EvolveSpecificPokemonTask
    {
        private readonly Inventory _inventory;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ILogicSettings _logicSettings;
        private readonly Client _client;

        public EvolveSpecificPokemonTask(Inventory inventory, IEventDispatcher eventDispatcher, ILogicSettings logicSettings, Client client)
        {
            _inventory = inventory;
            _eventDispatcher = eventDispatcher;
            _logicSettings = logicSettings;
            _client = client;
        }

        public async Task Execute(string pokemonId)
        {
            var id = ulong.Parse(pokemonId);

            var all = await _inventory.GetPokemons();
            var pokemons = all.OrderByDescending(x => x.Cp).ThenBy(n => n.StaminaMax);
            var pokemon = pokemons.FirstOrDefault(p => p.Id == id);

            if (pokemon == null) return;

            var evolveResponse = await _client.Inventory.EvolvePokemon(pokemon.Id);

            _eventDispatcher.Send(new PokemonEvolveEvent
            {
                Id = pokemon.PokemonId,
                Exp = evolveResponse.ExperienceAwarded,
                Result = evolveResponse.Result
            });

            await Task.Delay(_logicSettings.DelayEvolvePokemon);
        }
    }
}
