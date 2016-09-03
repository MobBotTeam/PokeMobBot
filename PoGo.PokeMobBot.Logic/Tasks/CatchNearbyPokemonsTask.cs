#region using directives

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Logging;
using PoGo.PokeMobBot.Logic.State;
using PoGo.PokeMobBot.Logic.Utils;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public static class CatchNearbyPokemonsTask
    {
        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            
            

            // Refresh inventory so that the player stats are fresh
            await session.Inventory.RefreshCachedInventory();

            session.EventDispatcher.Send(new DebugEvent()
            {
                Message = session.Translation.GetTranslation(TranslationString.LookingForPokemon)
            });

            var pokemons = await GetNearbyPokemons(session);
            foreach (var pokemon in pokemons)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var pokeBallsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemPokeBall);
                var greatBallsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemGreatBall);
                var ultraBallsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemUltraBall);
                var masterBallsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemMasterBall);

                if (pokeBallsCount + greatBallsCount + ultraBallsCount + masterBallsCount == 0)
                {
                    session.EventDispatcher.Send(new NoticeEvent()
                    {
                        Message = session.Translation.GetTranslation(TranslationString.ZeroPokeballInv)
                    });
                    return;
                }

                if (session.LogicSettings.UsePokemonToNotCatchFilter &&
                    session.LogicSettings.PokemonsNotToCatch.Contains(pokemon.PokemonId))
                {
                    session.EventDispatcher.Send(new NoticeEvent()
                    {
                        Message = session.Translation.GetTranslation(TranslationString.PokemonSkipped, session.Translation.GetPokemonName(pokemon.PokemonId))
                    });
                    continue;
                }

                var distance = LocationUtils.CalculateDistanceInMeters(session.Client.CurrentLatitude,
                    session.Client.CurrentLongitude, pokemon.Latitude, pokemon.Longitude);
                await Task.Delay(distance > 100 ? 3000 : 500, cancellationToken);

                var encounter =
                    await session.Client.Encounter.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnPointId);

                if (encounter.Status == EncounterResponse.Types.Status.EncounterSuccess)
                {
                    await CatchPokemonTask.Execute(session, encounter, pokemon);
                }
                else if (encounter.Status == EncounterResponse.Types.Status.PokemonInventoryFull)
                {
                    if (session.LogicSettings.TransferDuplicatePokemon)
                    {
                        session.EventDispatcher.Send(new WarnEvent
                        {
                            Message = session.Translation.GetTranslation(TranslationString.InvFullTransferring)
                        });
                        await TransferDuplicatePokemonTask.Execute(session, cancellationToken);
                    }
                    else
                        session.EventDispatcher.Send(new WarnEvent
                        {
                            Message = session.Translation.GetTranslation(TranslationString.InvFullTransferManually)
                        });
                }
                else if (encounter.Status == EncounterResponse.Types.Status.EncounterPokemonFled)
                {
                    session.EventDispatcher.Send(new WarnEvent
                    {
                        Message = session.Translation.GetTranslation(TranslationString.EncounterProblemPokemonFlee, session.Translation.GetPokemonName(encounter.WildPokemon.PokemonData.PokemonId))
                    });
                }
                else
                {
                    session.EventDispatcher.Send(new WarnEvent
                    {
                        Message =
                            session.Translation.GetTranslation(TranslationString.EncounterProblem, encounter.Status)
                    });
                }

                // always wait the delay amount between catches, ideally to prevent you from making another call too early after a catch event
                await Task.Delay(session.LogicSettings.DelayBetweenPokemonCatch);
            }
        }

        private static async Task<List<PokemonCacheItem>> GetNearbyPokemons(ISession session)
        {
            //var mapObjects = await session.Client.Map.GetMapObjects();

            var pokemons = await session.MapCache.MapPokemons(session);

            return pokemons;
        }
    }
}