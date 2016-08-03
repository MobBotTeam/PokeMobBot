#region using directives

using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Utils;
using PokemonGo.RocketAPI;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class CatchIncensePokemonsTask
    {
        private readonly TransferDuplicatePokemonTask _transferDuplicatePokemonTask;
        private readonly CatchPokemonTask _catchPokemonTask;
        private readonly LocationUtils _locationUtils;
        private readonly Inventory _inventory;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ITranslation _translation;
        private readonly Client _client;
        private readonly ILogicSettings _logicSettings;
        private readonly TransferLowStatPokemonTask _transferLowStatPokemonTask;

        public CatchIncensePokemonsTask(TransferDuplicatePokemonTask transferDuplicatePokemonTask, CatchPokemonTask catchPokemonTask, LocationUtils locationUtils, Inventory inventory, IEventDispatcher eventDispatcher, ITranslation translation, Client client, ILogicSettings logicSettings, TransferLowStatPokemonTask transferLowStatPokemonTask)
        {
            _transferDuplicatePokemonTask = transferDuplicatePokemonTask;
            _catchPokemonTask = catchPokemonTask;
            _locationUtils = locationUtils;
            _inventory = inventory;
            _eventDispatcher = eventDispatcher;
            _translation = translation;
            _client = client;
            _logicSettings = logicSettings;
            _transferLowStatPokemonTask = transferLowStatPokemonTask;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Refresh inventory so that the player stats are fresh
            await _inventory.RefreshCachedInventory();

            _eventDispatcher.Send(new DebugEvent()
            {
                Message = _translation.GetTranslation(TranslationString.LookingForIncensePokemon)
            });

            var incensePokemon = await _client.Map.GetIncensePokemons();
            if (incensePokemon.Result == GetIncensePokemonResponse.Types.Result.IncenseEncounterAvailable)
            {
                var pokemon = new MapPokemon
                {
                    EncounterId = incensePokemon.EncounterId,
                    ExpirationTimestampMs = incensePokemon.DisappearTimestampMs,
                    Latitude = incensePokemon.Latitude,
                    Longitude = incensePokemon.Longitude,
                    PokemonId = incensePokemon.PokemonId,
                    SpawnPointId = incensePokemon.EncounterLocation
                };

                if (_logicSettings.UsePokemonToNotCatchFilter && _logicSettings.PokemonsNotToCatch.Contains(pokemon.PokemonId))
                {
                    _eventDispatcher.Send(new NoticeEvent()
                    {
                        Message = _translation.GetTranslation(TranslationString.PokemonIgnoreFilter, _translation.GetPokemonName(pokemon.PokemonId))
                    });
                }
                else
                {
                    var distance = _locationUtils.CalculateDistanceInMeters(_client.CurrentLatitude, _client.CurrentLongitude, pokemon.Latitude, pokemon.Longitude);
                    if (_logicSettings.Teleport)
                        await Task.Delay(_logicSettings.DelayCatchIncensePokemon, cancellationToken);
                    else
                        await Task.Delay(distance > 100 ? 3000 : 500, cancellationToken);

                    var encounter = await _client.Encounter.EncounterIncensePokemon((long)pokemon.EncounterId, pokemon.SpawnPointId);

                    if (encounter.Result == IncenseEncounterResponse.Types.Result.IncenseEncounterSuccess)
                    {
                        await _catchPokemonTask.Execute(encounter, pokemon);
                    }
                    else if (encounter.Result == IncenseEncounterResponse.Types.Result.PokemonInventoryFull)
                    {
                        if (_logicSettings.TransferDuplicatePokemon)
                        {
                            _eventDispatcher.Send(new WarnEvent
                            {
                                Message = _translation.GetTranslation(TranslationString.InvFullTransferring)
                            });
                            await _transferDuplicatePokemonTask.Execute(cancellationToken);
                        }
                        if (_logicSettings.TransferLowStatPokemon)
                        {
                            _eventDispatcher.Send(new WarnEvent
                            {
                                Message = _translation.GetTranslation(TranslationString.InvFullTransferring)
                            });
                            await _transferLowStatPokemonTask.Execute(cancellationToken);
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
                            Message = _translation.GetTranslation(TranslationString.EncounterProblem, encounter.Result)
                        });
                    }
                }
            }
        }
    }
}