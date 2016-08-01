#region using directives

using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Logging;
using PoGo.PokeMobBot.Logic.State;
using PoGo.PokeMobBot.Logic.Utils;
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
        private readonly ILogger _logger;

        public CatchIncensePokemonsTask(TransferDuplicatePokemonTask transferDuplicatePokemonTask, CatchPokemonTask catchPokemonTask, LocationUtils locationUtils, ILogger logger)
        {
            _transferDuplicatePokemonTask = transferDuplicatePokemonTask;
            _catchPokemonTask = catchPokemonTask;
            _locationUtils = locationUtils;
            _logger = logger;
        }

        public async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.Write(session.Translation.GetTranslation(TranslationString.LookingForIncensePokemon), LogLevel.Debug);

            var incensePokemon = await session.Client.Map.GetIncensePokemons();
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

                if (session.LogicSettings.UsePokemonToNotCatchFilter &&
                    session.LogicSettings.PokemonsNotToCatch.Contains(pokemon.PokemonId))
                {
                    _logger.Write(session.Translation.GetTranslation(TranslationString.PokemonIgnoreFilter,
                        pokemon.PokemonId));
                }
                else
                {
                    var distance = _locationUtils.CalculateDistanceInMeters(session.Client.CurrentLatitude,
                        session.Client.CurrentLongitude, pokemon.Latitude, pokemon.Longitude);
                    if(session.LogicSettings.Teleport)
                        await Task.Delay(session.LogicSettings.DelayCatchIncensePokemon, cancellationToken);
                    else
                        await Task.Delay(distance > 100 ? 3000 : 500, cancellationToken);

                    var encounter =
                        await
                            session.Client.Encounter.EncounterIncensePokemon((long) pokemon.EncounterId,
                                pokemon.SpawnPointId);

                    if (encounter.Result == IncenseEncounterResponse.Types.Result.IncenseEncounterSuccess)
                    {
                        await _catchPokemonTask.Execute(session, encounter, pokemon);
                    }
                    else if (encounter.Result == IncenseEncounterResponse.Types.Result.PokemonInventoryFull)
                    {
                        if (session.LogicSettings.TransferDuplicatePokemon)
                        {
                            session.EventDispatcher.Send(new WarnEvent
                            {
                                Message = session.Translation.GetTranslation(TranslationString.InvFullTransferring)
                            });
                            await _transferDuplicatePokemonTask.Execute(session, cancellationToken);
                        }
                        else
                            session.EventDispatcher.Send(new WarnEvent
                            {
                                Message = session.Translation.GetTranslation(TranslationString.InvFullTransferManually)
                            });
                    }
                    else
                    {
                        session.EventDispatcher.Send(new WarnEvent
                        {
                            Message =
                                session.Translation.GetTranslation(TranslationString.EncounterProblem, encounter.Result)
                        });
                    }
                }
            }
        }
    }
}