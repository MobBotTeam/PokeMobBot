#region using directives

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Utils;
using PokemonGo.RocketAPI;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class CatchNearbyPokemonsTask
    {
        private readonly TransferDuplicatePokemonTask _transferDuplicatePokemonTask;
        private readonly CatchPokemonTask _catchPokemonTask;
        private readonly LocationUtils _locationUtils;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ITranslation _translation;
        private readonly Inventory _inventory;
        private readonly ILogicSettings _logicSettings;
        private readonly Client _client;

        public CatchNearbyPokemonsTask(TransferDuplicatePokemonTask transferDuplicatePokemonTask, CatchPokemonTask catchPokemonTask, LocationUtils locationUtils, IEventDispatcher eventDispatcher, ITranslation translation, Inventory inventory, ILogicSettings logicSettings, Client client)
        {
            _transferDuplicatePokemonTask = transferDuplicatePokemonTask;
            _catchPokemonTask = catchPokemonTask;
            _locationUtils = locationUtils;
            _eventDispatcher = eventDispatcher;
            _translation = translation;
            _inventory = inventory;
            _logicSettings = logicSettings;
            _client = client;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Refresh inventory so that the player stats are fresh
            await _inventory.RefreshCachedInventory();

            _eventDispatcher.Send(new DebugEvent()
            {
                Message = _translation.GetTranslation(TranslationString.LookingForPokemon)
            });

            var pokemons = await GetNearbyPokemons();
            foreach (var pokemon in pokemons)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var pokeBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemPokeBall);
                var greatBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemGreatBall);
                var ultraBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemUltraBall);
                var masterBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemMasterBall);

                if (pokeBallsCount + greatBallsCount + ultraBallsCount + masterBallsCount == 0)
                {
                    _eventDispatcher.Send(new NoticeEvent()
                    {
                        Message = _translation.GetTranslation(TranslationString.ZeroPokeballInv)
                    });
                    return;
                }

                if (_logicSettings.UsePokemonToNotCatchFilter && _logicSettings.PokemonsNotToCatch.Contains(pokemon.PokemonId))
                {
                    _eventDispatcher.Send(new NoticeEvent()
                    {
                        Message = _translation.GetTranslation(TranslationString.PokemonSkipped, _translation.GetPokemonName(pokemon.PokemonId))
                    });
                    continue;
                }

                var distance = _locationUtils.CalculateDistanceInMeters(_client.CurrentLatitude, _client.CurrentLongitude, pokemon.Latitude, pokemon.Longitude);
                await Task.Delay(distance > 100 ? 3000 : 500, cancellationToken);

                var encounter = await _client.Encounter.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnPointId);

                if (encounter.Status == EncounterResponse.Types.Status.EncounterSuccess)
                {
                    await _catchPokemonTask.Execute(encounter, pokemon);
                }
                else if (encounter.Status == EncounterResponse.Types.Status.PokemonInventoryFull)
                {
                    if (_logicSettings.TransferDuplicatePokemon)
                    {
                        _eventDispatcher.Send(new WarnEvent
                        {
                            Message = _translation.GetTranslation(TranslationString.InvFullTransferring)
                        });
                        await _transferDuplicatePokemonTask.Execute(cancellationToken);
                    }
                    else
                        _eventDispatcher.Send(new WarnEvent
                        {
                            Message = _translation.GetTranslation(TranslationString.InvFullTransferManually)
                        });
                }
                else
                {
                    _eventDispatcher.Send(new WarnEvent
                    {
                        Message =
                            _translation.GetTranslation(TranslationString.EncounterProblem, encounter.Status)
                    });
                }

                // If pokemon is not last pokemon in list, create delay between catches, else keep moving.
                if (!Equals(pokemons.ElementAtOrDefault(pokemons.Count() - 1), pokemon))
                {
                    if (_logicSettings.Teleport)
                        await Task.Delay(_logicSettings.DelayBetweenPokemonCatch, cancellationToken);
                    else
                        await Task.Delay(_logicSettings.DelayBetweenPokemonCatch, cancellationToken);
                }
            }
        }

        private async Task<IOrderedEnumerable<MapPokemon>> GetNearbyPokemons()
        {
            var mapObjects = await _client.Map.GetMapObjects();

            var pokemons = mapObjects.Item1.MapCells.SelectMany(i => i.CatchablePokemons)
                .OrderBy(
                    i =>
                        _locationUtils.CalculateDistanceInMeters(_client.CurrentLatitude,
                            _client.CurrentLongitude,
                            i.Latitude, i.Longitude));

            return pokemons;
        }
    }
}