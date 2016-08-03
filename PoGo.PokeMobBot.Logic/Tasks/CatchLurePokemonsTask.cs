#region using directives

using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.State;
using PokemonGo.RocketAPI;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Responses;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class CatchLurePokemonsTask
    {
        private readonly TransferDuplicatePokemonTask _transferDuplicatePokemonTask;
        private readonly CatchPokemonTask _catchPokemonTask;
        private readonly Inventory _inventory;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ITranslation _translation;
        private readonly ILogicSettings _logicSettings;
        private readonly Client _client;
        private readonly TransferLowStatPokemonTask _lowStatPokemonTask;

        public CatchLurePokemonsTask(TransferDuplicatePokemonTask transferDuplicatePokemonTask, CatchPokemonTask catchPokemonTask, Inventory inventory, IEventDispatcher eventDispatcher, ITranslation translation, ILogicSettings logicSettings, Client client, TransferLowStatPokemonTask lowStatPokemonTask)
        {
            _transferDuplicatePokemonTask = transferDuplicatePokemonTask;
            _catchPokemonTask = catchPokemonTask;
            _inventory = inventory;
            _eventDispatcher = eventDispatcher;
            _translation = translation;
            _logicSettings = logicSettings;
            _client = client;
            _lowStatPokemonTask = lowStatPokemonTask;
        }

        public async Task Execute(FortData currentFortData, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Refresh inventory so that the player stats are fresh
            await _inventory.RefreshCachedInventory();

            _eventDispatcher.Send(new DebugEvent()
            {
                Message = _translation.GetTranslation(TranslationString.LookingForLurePokemon)
            });

            var fortId = currentFortData.Id;

            var pokemonId = currentFortData.LureInfo.ActivePokemonId;

            if (_logicSettings.UsePokemonToNotCatchFilter && _logicSettings.PokemonsNotToCatch.Contains(pokemonId))
            {
                _eventDispatcher.Send(new NoticeEvent
                {
                    Message = _translation.GetTranslation(TranslationString.PokemonSkipped, _translation.GetPokemonName(pokemonId))
                });
            }
            else
            {
                var encounterId = currentFortData.LureInfo.EncounterId;
                var encounter = await _client.Encounter.EncounterLurePokemon(encounterId, fortId);

                if (encounter.Result == DiskEncounterResponse.Types.Result.Success)
                {
                    await _catchPokemonTask.Execute(encounter, null, currentFortData, encounterId);
                }
                else if (encounter.Result == DiskEncounterResponse.Types.Result.PokemonInventoryFull)
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
                        await _lowStatPokemonTask.Execute(cancellationToken);
                    }
                    else
                        _eventDispatcher.Send(new WarnEvent
                        {
                            Message = _translation.GetTranslation(TranslationString.InvFullTransferManually)
                        });
                }
                else
                {
                    if (encounter.Result.ToString().Contains("NotAvailable")) return;
                    _eventDispatcher.Send(new WarnEvent
                    {
                        Message = _translation.GetTranslation(TranslationString.EncounterProblemLurePokemon, encounter.Result)
                    });
                }
            }
        }
    }
}