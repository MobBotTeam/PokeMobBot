#region using directives

using System;
using System.Linq;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.PoGoUtils;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class PokemonListTask
    {
        private readonly PokemonInfo _pokemonInfo;
        private readonly ILogicSettings _logicSettings;
        private readonly Inventory _inventory;

        public PokemonListTask(PokemonInfo pokemonInfo, ILogicSettings logicSettings, Inventory inventory)
        {
            _pokemonInfo = pokemonInfo;
            _logicSettings = logicSettings;
            _inventory = inventory;
        }

        public async Task Execute(Action<IEvent> action)
        {
            // Refresh inventory so that the player stats are fresh
            await _inventory.RefreshCachedInventory();

            var myPokemonSettings = await _inventory.GetPokemonSettings();
            var pokemonSettings = myPokemonSettings.ToList();

            var myPokemonFamilies = await _inventory.GetPokemonFamilies();
            var pokemonFamilies = myPokemonFamilies.ToArray();

            var allPokemonInBag = await _inventory.GetHighestsCp(1000);

            var pkmWithIv = allPokemonInBag.Select(p => {
                var settings = pokemonSettings.Single(x => x.PokemonId == p.PokemonId);
                return Tuple.Create(
                    p,
                    _pokemonInfo.CalculatePokemonPerfection(p),
                    pokemonFamilies.Single(x => settings.FamilyId == x.FamilyId).Candy_
                );
            });

            action(new PokemonListEvent
            {
                PokemonList = pkmWithIv.ToList()
            });

            await Task.Delay(_logicSettings.DelayBetweenPlayerActions);
        }
    }
}