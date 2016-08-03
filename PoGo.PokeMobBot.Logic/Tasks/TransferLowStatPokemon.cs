#region using directives

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.PoGoUtils;
using PoGo.PokeMobBot.Logic.Utils;
using PokemonGo.RocketAPI;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class TransferLowStatPokemonTask
    {
        private readonly Inventory _inventory;
        private readonly ILogicSettings _logicSettings;
        private readonly PokemonInfo _pokemonInfo;
        private readonly Client _client;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly DelayingUtils _delayingUtils;

        public TransferLowStatPokemonTask(Inventory inventory, ILogicSettings logicSettings, PokemonInfo pokemonInfo, Client client, IEventDispatcher eventDispatcher, DelayingUtils delayingUtils)
        {
            _inventory = inventory;
            _logicSettings = logicSettings;
            _pokemonInfo = pokemonInfo;
            _client = client;
            _eventDispatcher = eventDispatcher;
            _delayingUtils = delayingUtils;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Refresh inventory so that the player stats are fresh
            await _inventory.RefreshCachedInventory();

            var pokemons = await _inventory.GetPokemons();

            var pokemonList = pokemons.Where(p => !_logicSettings.PokemonsNotToTransfer.Contains(p.PokemonId)).ToList(); //filter out the do not transfers

            if (_logicSettings.KeepPokemonsThatCanEvolve)
            {
                pokemonList = pokemonList.Where(p => !_logicSettings.PokemonsToEvolve.Contains(p.PokemonId)).ToList(); //filter out the evolve list if evolve is true
            }

            var pokemonSettings = await _inventory.GetPokemonSettings();
            var pokemonFamilies = await _inventory.GetPokemonFamilies();

            foreach (var pokemon in pokemonList)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (pokemon.Favorite == 1)
                    continue;

                if (_logicSettings.PrioritizeIvAndCp)    //combined mode - pokemon has to match minimum CP and IV requirements to be kept
                {
                    if (pokemon.Cp >= _logicSettings.KeepMinCp && (_pokemonInfo.CalculatePokemonPerfection(pokemon) >= _logicSettings.KeepMinIvPercentage))
                    {
                        continue;
                    }
                }
                else    //normal filtering
                {
                    if (pokemon.Cp >= _logicSettings.KeepMinCp)  //dont toss if above minimum CP
                    {
                        continue;
                    }
                    if (_logicSettings.PrioritizeIvOverCp)
                    {
                        if (_pokemonInfo.CalculatePokemonPerfection(pokemon) >= _logicSettings.KeepMinIvPercentage) //dont toss if its over min IV
                        {
                            continue;
                        }
                    }
                }

                await _client.Inventory.TransferPokemon(pokemon.Id);
                await _inventory.DeletePokemonFromInvById(pokemon.Id);

                var bestPokemonOfType = (_logicSettings.PrioritizeIvOverCp
                    ? await _inventory.GetHighestPokemonOfTypeByIv(pokemon)
                    : await _inventory.GetHighestPokemonOfTypeByCp(pokemon)) ?? pokemon;

                var setting = pokemonSettings.Single(q => q.PokemonId == pokemon.PokemonId);
                var family = pokemonFamilies.First(q => q.FamilyId == setting.FamilyId);

                family.Candy_++;

                _eventDispatcher.Send(new TransferPokemonEvent
                {
                    Id = pokemon.PokemonId,
                    Perfection = _pokemonInfo.CalculatePokemonPerfection(pokemon),
                    Cp = pokemon.Cp,
                    BestCp = bestPokemonOfType.Cp,
                    BestPerfection = _pokemonInfo.CalculatePokemonPerfection(bestPokemonOfType),
                    FamilyCandies = family.Candy_
                });
                if (_logicSettings.Teleport)
                    await Task.Delay(_logicSettings.DelayTransferPokemon, cancellationToken);
                else
                    await _delayingUtils.Delay(_logicSettings.DelayBetweenPlayerActions, 0);
            }
        }
    }
}