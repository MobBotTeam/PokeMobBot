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
        private readonly IEventDispatcher _eventDispatcher;

        public PokemonListTask(PokemonInfo pokemonInfo, ILogicSettings logicSettings, Inventory inventory, IEventDispatcher eventDispatcher)
        {
            _pokemonInfo = pokemonInfo;
            _logicSettings = logicSettings;
            _inventory = inventory;
            _eventDispatcher = eventDispatcher;
        }

        public async Task Execute()
        {
            // Refresh inventory so that the player stats are fresh
            await _inventory.RefreshCachedInventory();

            var allPokemonInBag = await _inventory.GetHighestsCp(1000);
            var pkmWithIv = allPokemonInBag.Select(p => Tuple.Create(p, _pokemonInfo.CalculatePokemonPerfection(p)));
            _eventDispatcher.Send(
                new PokemonListEvent
                {
                    PokemonList = pkmWithIv.ToList()
                });
            if(_logicSettings.Teleport)
                await Task.Delay(_logicSettings.DelayDisplayPokemon);
            else
                await Task.Delay(500);
        }
    }
}