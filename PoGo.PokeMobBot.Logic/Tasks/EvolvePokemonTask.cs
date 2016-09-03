#region using directives

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.State;
using PoGo.PokeMobBot.Logic.Utils;
using PokemonGo.RocketAPI;
using POGOProtos.Inventory.Item;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class EvolvePokemonTask
    {
        private readonly DelayingUtils _delayingUtils;
        private readonly DelayingEvolveUtils _delayingEvolveUtils;
        private readonly Inventory _inventory;
        private readonly ILogicSettings _logicSettings;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly Client _client;

        private DateTime _lastLuckyEggTime;

        public EvolvePokemonTask(DelayingUtils delayingUtils, DelayingEvolveUtils delayingEvolveUtils, Inventory inventory, ILogicSettings logicSettings, IEventDispatcher eventDispatcher, Client client)
        {
            _delayingUtils = delayingUtils;
            _delayingEvolveUtils = delayingEvolveUtils;
            _inventory = inventory;
            _logicSettings = logicSettings;
            _eventDispatcher = eventDispatcher;
            _client = client;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Refresh inventory so that the player stats are fresh
            await _inventory.RefreshCachedInventory();

            var pokemonToEvolveTask = await _inventory.GetPokemonToEvolve(_logicSettings.PokemonsToEvolve);
            var pokemonToEvolve = pokemonToEvolveTask.ToList();

            if (pokemonToEvolve.Any())
            {
                var inventoryContent = await _inventory.GetItems();

                var luckyEggs = inventoryContent.Where(p => p.ItemId == ItemId.ItemLuckyEgg);
                var luckyEgg = luckyEggs.FirstOrDefault();

                //maybe there can be a warning message as an else condition of luckyEgg checks, like; 
                //"There is no Lucky Egg, so, your UseLuckyEggsMinPokemonAmount setting bypassed."
                if (_logicSettings.UseLuckyEggsWhileEvolving && luckyEgg != null && luckyEgg.Count > 0)
                {
                    if (pokemonToEvolve.Count >= _logicSettings.UseLuckyEggsMinPokemonAmount)
                    {
                        await UseLuckyEgg();
                    }
                    else
                    {
                        // Wait until we have enough pokemon
                        _eventDispatcher.Send(new UseLuckyEggMinPokemonEvent
                        {
                            Diff = _logicSettings.UseLuckyEggsMinPokemonAmount - pokemonToEvolve.Count,
                            CurrCount = pokemonToEvolve.Count,
                            MinPokemon = _logicSettings.UseLuckyEggsMinPokemonAmount
                        });
                        return;
                    }
                }

                foreach (var pokemon in pokemonToEvolve)
                {
                    // no cancellationToken.ThrowIfCancellationRequested here, otherwise the lucky egg would be wasted.
                    var evolveResponse = await _client.Inventory.EvolvePokemon(pokemon.Id);

                    _eventDispatcher.Send(new PokemonEvolveEvent
                    {
                        Id = pokemon.PokemonId,
                        Exp = evolveResponse.ExperienceAwarded,
                        Result = evolveResponse.Result
                    });

                    await _delayingEvolveUtils.Delay(_logicSettings.DelayEvolvePokemon, 0, _logicSettings.DelayEvolveVariation);
                }
            }
        }

        public async Task UseLuckyEgg()
        {
            var inventoryContent = await _inventory.GetItems();

            var luckyEggs = inventoryContent.Where(p => p.ItemId == ItemId.ItemLuckyEgg);
            var luckyEgg = luckyEggs.FirstOrDefault();

            if (_lastLuckyEggTime.AddMinutes(30).Ticks > DateTime.Now.Ticks)
                return;

            _lastLuckyEggTime = DateTime.Now;
            await _client.Inventory.UseItemXpBoost();
            await _inventory.RefreshCachedInventory();
            if (luckyEgg != null) _eventDispatcher.Send(new UseLuckyEggEvent { Count = luckyEgg.Count });
            if (_logicSettings.Teleport)
                await Task.Delay(_logicSettings.DelayDisplayPokemon);
            else
                await _delayingUtils.Delay(_logicSettings.DelayBetweenPokemonCatch, 2000);
        }
    }
}
