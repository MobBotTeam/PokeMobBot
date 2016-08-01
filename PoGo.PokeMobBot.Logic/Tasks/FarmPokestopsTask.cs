#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GeoCoordinatePortable;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Logging;
using PoGo.PokeMobBot.Logic.State;
using PoGo.PokeMobBot.Logic.Utils;
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Responses;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class FarmPokestopsTask
    {
        private readonly RecycleItemsTask _recycleItemsTask;
        private readonly EvolvePokemonTask _evolvePokemonTask;
        private readonly LevelUpPokemonTask _levelUpPokemonTask;
        private readonly TransferDuplicatePokemonTask _transferDuplicatePokemonTask;
        private readonly RenamePokemonTask _renamePokemonTask;
        private readonly SnipePokemonTask _snipePokemonTask;
        private readonly EggWalker _eggWalker;
        private readonly CatchNearbyPokemonsTask _catchNearbyPokemonsTask;
        private readonly CatchIncensePokemonsTask _catchIncensePokemonsTask;
        private readonly CatchLurePokemonsTask _catchLurePokemonsTask;
        private readonly DelayingUtils _delayingUtils;
        private readonly LocationUtils _locationUtils;
        private readonly StringUtils _stringUtils;
        private readonly ILogger _logger;
        private readonly DisplayPokemonStatsTask _displayPokemonStatsTask;

        public int TimesZeroXPawarded;

        public FarmPokestopsTask(RecycleItemsTask recycleItemsTask, EvolvePokemonTask evolvePokemonTask, LevelUpPokemonTask levelUpPokemonTask, TransferDuplicatePokemonTask transferDuplicatePokemonTask, RenamePokemonTask renamePokemonTask, SnipePokemonTask snipePokemonTask, EggWalker eggWalker, CatchNearbyPokemonsTask catchNearbyPokemonsTask, CatchIncensePokemonsTask catchIncensePokemonsTask, CatchLurePokemonsTask catchLurePokemonsTask, DelayingUtils delayingUtils, LocationUtils locationUtils, StringUtils stringUtils, ILogger logger, DisplayPokemonStatsTask displayPokemonStatsTask)
        {
            _logger = logger;
            _displayPokemonStatsTask = displayPokemonStatsTask;
            _recycleItemsTask = recycleItemsTask;
            _evolvePokemonTask = evolvePokemonTask;
            _levelUpPokemonTask = levelUpPokemonTask;
            _transferDuplicatePokemonTask = transferDuplicatePokemonTask;
            _renamePokemonTask = renamePokemonTask;
            _snipePokemonTask = snipePokemonTask;
            _eggWalker = eggWalker;
            _catchNearbyPokemonsTask = catchNearbyPokemonsTask;
            _catchIncensePokemonsTask = catchIncensePokemonsTask;
            _catchLurePokemonsTask = catchLurePokemonsTask;
            _delayingUtils = delayingUtils;
            _locationUtils = locationUtils;
            _stringUtils = stringUtils;
        }

        public async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var distanceFromStart = _locationUtils.CalculateDistanceInMeters(
                session.Settings.DefaultLatitude, session.Settings.DefaultLongitude,
                session.Client.CurrentLatitude, session.Client.CurrentLongitude);

            // Edge case for when the client somehow ends up outside the defined radius
            if (session.LogicSettings.MaxTravelDistanceInMeters != 0 &&
                distanceFromStart > session.LogicSettings.MaxTravelDistanceInMeters)
            {
                _logger.Write(
                    session.Translation.GetTranslation(TranslationString.FarmPokestopsOutsideRadius, distanceFromStart),
                    LogLevel.Warning);

                await Task.Delay(1000, cancellationToken);

                await session.Navigation.HumanLikeWalking(
                    new GeoCoordinate(session.Settings.DefaultLatitude, session.Settings.DefaultLongitude),
                    session.LogicSettings.WalkingSpeedInKilometerPerHour, null, cancellationToken);
            }

            var pokestopList = await GetPokeStops(session);
            var stopsHit = 0;
            var displayStatsHit = 0;

            if (pokestopList.Count <= 0)
            {
                session.EventDispatcher.Send(new WarnEvent
                {
                    Message = session.Translation.GetTranslation(TranslationString.FarmPokestopsNoUsableFound)
                });
            }

            session.EventDispatcher.Send(new PokeStopListEvent {Forts = pokestopList});

            while (pokestopList.Any())
            {
                cancellationToken.ThrowIfCancellationRequested();

                //resort
                pokestopList =
                    pokestopList.OrderBy(
                        i =>
                            _locationUtils.CalculateDistanceInMeters(session.Client.CurrentLatitude,
                                session.Client.CurrentLongitude, i.Latitude, i.Longitude)).ToList();
                var pokeStop = pokestopList[0];
                pokestopList.RemoveAt(0);

                var distance = _locationUtils.CalculateDistanceInMeters(session.Client.CurrentLatitude,
                    session.Client.CurrentLongitude, pokeStop.Latitude, pokeStop.Longitude);
                var fortInfo = await session.Client.Fort.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);

                session.EventDispatcher.Send(new FortTargetEvent { Id = fortInfo.FortId, Name = fortInfo.Name, Distance = distance,Latitude = fortInfo.Latitude, Longitude = fortInfo.Longitude, Description = fortInfo.Description, url = fortInfo.ImageUrls[0] });
                if (session.LogicSettings.Teleport)
                    await session.Client.Player.UpdatePlayerLocation(fortInfo.Latitude, fortInfo.Longitude,
                        session.Client.Settings.DefaultAltitude);

                else
                {
                    await session.Navigation.HumanLikeWalking(new GeoCoordinate(pokeStop.Latitude, pokeStop.Longitude),
                    session.LogicSettings.WalkingSpeedInKilometerPerHour,
                    async () =>
                    {
                        // Catch normal map Pokemon
                        await _catchNearbyPokemonsTask.Execute(session, cancellationToken);
                        //Catch Incense Pokemon
                        await _catchIncensePokemonsTask.Execute(session, cancellationToken);
                        return true;
                    }, cancellationToken);
                }
                
                FortSearchResponse fortSearch;
                var timesZeroXPawarded = 0;
                var fortTry = 0; //Current check
                const int retryNumber = 50; //How many times it needs to check to clear softban
                const int zeroCheck = 5; //How many times it checks fort before it thinks it's softban
                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    fortSearch =
                        await session.Client.Fort.SearchFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                    if (fortSearch.ExperienceAwarded > 0 && timesZeroXPawarded > 0) timesZeroXPawarded = 0;
                    if (fortSearch.ExperienceAwarded == 0)
                    {
                        timesZeroXPawarded++;

                        if (timesZeroXPawarded > zeroCheck)
                        {
                            if ((int) fortSearch.CooldownCompleteTimestampMs != 0)
                            {
                                break;
                                    // Check if successfully looted, if so program can continue as this was "false alarm".
                            }

                            fortTry += 1;

                            session.EventDispatcher.Send(new FortFailedEvent
                            {
                                Name = fortInfo.Name,
                                Try = fortTry,
                                Max = retryNumber - zeroCheck
                            });
                            if(session.LogicSettings.Teleport)
                                await Task.Delay(session.LogicSettings.DelaySoftbanRetry, cancellationToken);
                            else
                                await _delayingUtils.Delay(session.LogicSettings.DelayBetweenPlayerActions, 400);
                        }
                    }
                    else
                    {
                        session.EventDispatcher.Send(new FortUsedEvent
                        {
                            Id = pokeStop.Id,
                            Name = fortInfo.Name,
                            Exp = fortSearch.ExperienceAwarded,
                            Gems = fortSearch.GemsAwarded,
                            Items = _stringUtils.GetSummedFriendlyNameOfItemAwardList(fortSearch.ItemsAwarded),
                            Latitude = pokeStop.Latitude,
                            Longitude = pokeStop.Longitude,
                            InventoryFull = fortSearch.Result == FortSearchResponse.Types.Result.InventoryFull,
                            Description = fortInfo.Description,
                            url = fortInfo.ImageUrls[0]                            
                        });

                        break; //Continue with program as loot was succesfull.
                    }
                } while (fortTry < retryNumber - zeroCheck);
                    //Stop trying if softban is cleaned earlier or if 40 times fort looting failed.


                if(session.LogicSettings.Teleport)
                    await Task.Delay(session.LogicSettings.DelayPokestop);
                else
                    await Task.Delay(1000, cancellationToken);


                //Catch Lure Pokemon


                if (pokeStop.LureInfo != null)
                {
                    await _catchLurePokemonsTask.Execute(session, pokeStop, cancellationToken);
                }
                if(session.LogicSettings.Teleport)
                    await _catchNearbyPokemonsTask.Execute(session, cancellationToken);

                await _eggWalker.ApplyDistance(distance, cancellationToken);

                if (++stopsHit%5 == 0) //TODO: OR item/pokemon bag is full
                {
                    stopsHit = 0;
                    // need updated stardust information for upgrading, so refresh your profile now
                    await DownloadProfile(session);

                    if (fortSearch.ItemsAwarded.Count > 0)
                    {
                        await session.Inventory.RefreshCachedInventory();
                    }
                    await _recycleItemsTask.Execute(session, cancellationToken);
                    if (session.LogicSettings.EvolveAllPokemonWithEnoughCandy ||
                        session.LogicSettings.EvolveAllPokemonAboveIv)
                    {
                        await _evolvePokemonTask.Execute(session, cancellationToken);
                    }
                    if (session.LogicSettings.AutomaticallyLevelUpPokemon)
                    {
                        await _levelUpPokemonTask.Execute(session, cancellationToken);
                    }
                    if (session.LogicSettings.TransferDuplicatePokemon)
                    {
                        await _transferDuplicatePokemonTask.Execute(session, cancellationToken);
                    }
                    if (session.LogicSettings.RenamePokemon)
                    {
                        await _renamePokemonTask.Execute(session, cancellationToken);
                    }
                    if (++displayStatsHit >= 4)
                    {
                        await _displayPokemonStatsTask.Execute(session);
                        displayStatsHit = 0;
                    }
                }

                if (session.LogicSettings.SnipeAtPokestops || session.LogicSettings.UseSnipeLocationServer)
                {
                    await _snipePokemonTask.Execute(session, cancellationToken);
                }
            }
        }

        private async Task<List<FortData>> GetPokeStops(ISession session)
        {
            var mapObjects = await session.Client.Map.GetMapObjects();

            // Wasn't sure how to make this pretty. Edit as needed.
            var pokeStops = mapObjects.MapCells.SelectMany(i => i.Forts)
                .Where(
                    i =>
                        i.Type == FortType.Checkpoint &&
                        i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime() &&
                        ( // Make sure PokeStop is within max travel distance, unless it's set to 0.
                            _locationUtils.CalculateDistanceInMeters(
                                session.Settings.DefaultLatitude, session.Settings.DefaultLongitude,
                                i.Latitude, i.Longitude) < session.LogicSettings.MaxTravelDistanceInMeters) ||
                        session.LogicSettings.MaxTravelDistanceInMeters == 0
                );

            return pokeStops.ToList();
        }

        // static copy of download profile, to update stardust more accurately
        private async Task DownloadProfile(ISession session)
        {
            session.Profile = await session.Client.Player.GetPlayer();
            session.EventDispatcher.Send(new ProfileEvent { Profile = session.Profile });
        }
    }
}