using System.Linq;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.PoGoUtils;
using PokemonGo.RocketAPI;

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class TransferPokemonTask
    {
        private readonly Inventory _inventory;
        private readonly Client _client;
        private readonly ILogicSettings _logicSettings;
        private readonly PokemonInfo _pokemonInfo;
        private readonly IEventDispatcher _eventDispatcher;

        public TransferPokemonTask(Inventory inventory, Client client, ILogicSettings logicSettings, PokemonInfo pokemonInfo, IEventDispatcher eventDispatcher)
        {
            _inventory = inventory;
            _client = client;
            _logicSettings = logicSettings;
            _pokemonInfo = pokemonInfo;
            _eventDispatcher = eventDispatcher;
        }

        public async Task Execute(string pokemonId)
        {
            var id = ulong.Parse(pokemonId);

            var all = await _inventory.GetPokemons();
            var pokemons = all.OrderByDescending(x => x.Cp).ThenBy(n => n.StaminaMax);
            var pokemon = pokemons.FirstOrDefault(p => p.Id == id);

            if (pokemon == null) return;

            var pokemonSettings = await _inventory.GetPokemonSettings();
            var pokemonFamilies = await _inventory.GetPokemonFamilies();

            await _client.Inventory.TransferPokemon(id);
            await _inventory.DeletePokemonFromInvById(id);

            var bestPokemonOfType = (_logicSettings.PrioritizeIvOverCp
                ? await _inventory.GetHighestPokemonOfTypeByIv(pokemon)
                : await _inventory.GetHighestPokemonOfTypeByCp(pokemon)) ?? pokemon;

            var setting = pokemonSettings.Single(q => q.PokemonId == pokemon.PokemonId);
            var family = pokemonFamilies.First(q => q.FamilyId == setting.FamilyId);

            family.Candy_++;

            // Broadcast event as everyone would benefit
            _eventDispatcher.Send(new TransferPokemonEvent
            {
                Id = pokemon.PokemonId,
                Perfection = _pokemonInfo.CalculatePokemonPerfection(pokemon),
                Cp = pokemon.Cp,
                BestCp = bestPokemonOfType.Cp,
                BestPerfection = _pokemonInfo.CalculatePokemonPerfection(bestPokemonOfType),
                FamilyCandies = family.Candy_
            });

            await Task.Delay(_logicSettings.DelayTransferPokemon);
        }
    }
}
