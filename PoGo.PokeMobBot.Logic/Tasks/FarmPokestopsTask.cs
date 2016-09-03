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
using PokemonGo.RocketAPI;
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
        private readonly DisplayPokemonStatsTask _displayPokemonStatsTask;
        private readonly ISettings _settings;
        private readonly Client _client;
        private readonly Navigation _navigation;
        private readonly ILogicSettings _logicSettings;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ITranslation _translation;
        private readonly Inventory _inventory;
        private readonly ILogger _logger;

        public int TimesZeroXPawarded;

        public FarmPokestopsTask(RecycleItemsTask recycleItemsTask, EvolvePokemonTask evolvePokemonTask, LevelUpPokemonTask levelUpPokemonTask, TransferDuplicatePokemonTask transferDuplicatePokemonTask, RenamePokemonTask renamePokemonTask, SnipePokemonTask snipePokemonTask, EggWalker eggWalker, CatchNearbyPokemonsTask catchNearbyPokemonsTask, CatchIncensePokemonsTask catchIncensePokemonsTask, CatchLurePokemonsTask catchLurePokemonsTask, DelayingUtils delayingUtils, LocationUtils locationUtils, StringUtils stringUtils, DisplayPokemonStatsTask displayPokemonStatsTask, ISettings settings, Client client, Navigation navigation, ILogicSettings logicSettings, IEventDispatcher eventDispatcher, ITranslation translation, Inventory inventory, ILogger logger)
        {
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
            _displayPokemonStatsTask = displayPokemonStatsTask;
            _settings = settings;
            _client = client;
            _navigation = navigation;
            _logicSettings = logicSettings;
            _eventDispatcher = eventDispatcher;
            _translation = translation;
            _inventory = inventory;
            _logger = logger;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            if (_logicSettings.Teleport)
                await Teleport(cancellationToken);
            else
                await NoTeleport(cancellationToken);
        }

        public async Task Teleport(CancellationToken cancellationToken)
        {
            TeleportAI tele = new TeleportAI();
            int stopsToHit = 20; //We should return to the main loop after some point, might as well limit this.
            //Not sure where else we could put this? Configs maybe if we incorporate
            //deciding how many pokestops in a row we want to hit before doing things like recycling?
            //might increase xp/hr not stopping every 5 stops. - Pocket


            //TODO: run through this with a fine-tooth comb and optimize it.
            var pokestopList = await GetPokeStops();
            for (int stopsHit = 0; stopsHit < stopsToHit; stopsHit++)
            {
                if (pokestopList.Count > 0)
                {
                    //start at 0 ends with 19 = 20 for the leechers{
                    cancellationToken.ThrowIfCancellationRequested();

                    var distanceFromStart = _locationUtils.CalculateDistanceInMeters(
                        _settings.DefaultLatitude, _settings.DefaultLongitude,
                        _client.CurrentLatitude, _client.CurrentLongitude);

                    // Edge case for when the client somehow ends up outside the defined radius
                    if (_logicSettings.MaxTravelDistanceInMeters != 0 &&
                        distanceFromStart > _logicSettings.MaxTravelDistanceInMeters)
                    {
                        _eventDispatcher.Send(new WarnEvent()
                        {
                            Message = _translation.GetTranslation(TranslationString.FarmPokestopsOutsideRadius, distanceFromStart)
                        });
                        await Task.Delay(1000, cancellationToken);

                        await _navigation.HumanLikeWalking(
                            new GeoCoordinate(_settings.DefaultLatitude, _settings.DefaultLongitude),
                            _logicSettings.WalkingSpeedInKilometerPerHour, null, cancellationToken);
                    }



                    var displayStatsHit = 0;

                    if (pokestopList.Count <= 0)
                    {
                        _eventDispatcher.Send(new WarnEvent
                        {
                            Message = _translation.GetTranslation(TranslationString.FarmPokestopsNoUsableFound)
                        });
                    }

                    _eventDispatcher.Send(new PokeStopListEvent { Forts = pokestopList });

                    cancellationToken.ThrowIfCancellationRequested();

                    //resort
                    pokestopList =
                        pokestopList.OrderBy(
                            i =>
                                _locationUtils.CalculateDistanceInMeters(_client.CurrentLatitude,
                                    _client.CurrentLongitude, i.Latitude, i.Longitude)).ToList();
                    var pokeStop = pokestopList[0];
                    pokestopList.RemoveAt(0);
					
                  Random rand = new Random();
                    int MaxDistanceFromStop = 25;
        var distance = _locationUtils.CalculateDistanceInMeters(_client.CurrentLatitude,
                        _client.CurrentLongitude, pokeStop.Latitude, pokeStop.Longitude);
                    var randLat = pokeStop.Latitude + rand.NextDouble() * ((double)MaxDistanceFromStop / 111111);
                    var randLong = pokeStop.Longitude + rand.NextDouble() * ((double)MaxDistanceFromStop / 111111);
                    var fortInfo = await _client.Fort.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);

                    _eventDispatcher.Send(new FortTargetEvent { Id = fortInfo.FortId, Name = fortInfo.Name, Distance = distance, Latitude = fortInfo.Latitude, Longitude = fortInfo.Longitude, Description = fortInfo.Description, url = fortInfo.ImageUrls[0] });
                    if (_logicSettings.TeleAI)

                    {
                        //test purposes
                        _logger.Write("We are within " + distance + " meters of the PokeStop.");
                        await _client.Player.UpdatePlayerLocation(randLat, randLong, _client.Settings.DefaultAltitude);
                        tele.getDelay(distance);
                    }
                    else if (_logicSettings.Teleport)
                        await _client.Player.UpdatePlayerLocation(randLat, randLong,
                            _client.Settings.DefaultAltitude);

                    else
                    {
                        await _navigation.HumanLikeWalking(new GeoCoordinate(randLat, randLong),
                        _logicSettings.WalkingSpeedInKilometerPerHour,
                        async () =>
                        {
                            if (_logicSettings.CatchWildPokemon)
                            {
                                // Catch normal map Pokemon
                                await _catchNearbyPokemonsTask.Execute(cancellationToken);
                                //Catch Incense Pokemon
                                await _catchIncensePokemonsTask.Execute(cancellationToken);
                            }
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
                            await _client.Fort.SearchFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                        if (fortSearch.ExperienceAwarded > 0 && timesZeroXPawarded > 0) timesZeroXPawarded = 0;
                        if (fortSearch.ExperienceAwarded == 0)
                        {
                            timesZeroXPawarded++;

                            if (timesZeroXPawarded > zeroCheck)
                            {
                                if ((int)fortSearch.CooldownCompleteTimestampMs != 0)
                                {
                                    break;
                                    // Check if successfully looted, if so program can continue as this was "false alarm".
                                }

                                fortTry += 1;

                                _eventDispatcher.Send(new FortFailedEvent
                                {
                                    Name = fortInfo.Name,
                                    Try = fortTry,
                                    Max = retryNumber - zeroCheck
                                });
                                if (_logicSettings.Teleport)
                                    await Task.Delay(_logicSettings.DelaySoftbanRetry);
                                else
                                    await _delayingUtils.Delay(_logicSettings.DelayBetweenPlayerActions, 400);
                            }
                        }
                        else
                        {
                            _eventDispatcher.Send(new FortUsedEvent
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
                    if (fortTry > 1)
                    {
                        int distance2 = (int)distance;
                        tele.addDelay(distance2);
                    }

                    if (_logicSettings.Teleport)
                        await Task.Delay(_logicSettings.DelayPokestop);
                    else
                        await Task.Delay(1000, cancellationToken);


                    //Catch Lure Pokemon

                    if (_logicSettings.CatchWildPokemon)
                    {
                        if (pokeStop.LureInfo != null)
                        {
                            await _catchLurePokemonsTask.Execute(pokeStop, cancellationToken);
                        }
                        // Catch normal map Pokemon
                        if (_logicSettings.Teleport)
                            await _catchNearbyPokemonsTask.Execute(cancellationToken);
                        //Catch Incense Pokemon
                        await _catchIncensePokemonsTask.Execute(cancellationToken);
                    }
                    

                    await _eggWalker.ApplyDistance(distance, cancellationToken);

                    if (++stopsHit % 5 == 0) //TODO: OR item/pokemon bag is full
                    {
                        // need updated stardust information for upgrading, so refresh your profile now
                        await DownloadProfile();

                        if (fortSearch.ItemsAwarded.Count > 0)
                        {
                            await _inventory.RefreshCachedInventory();
                        }
                        await _recycleItemsTask.Execute(cancellationToken);
                        if (_logicSettings.EvolveAllPokemonWithEnoughCandy ||
                            _logicSettings.EvolveAllPokemonAboveIv)
                        {
                            await _evolvePokemonTask.Execute(cancellationToken);
                        }
                        if (_logicSettings.AutomaticallyLevelUpPokemon)
                        {
                            await _levelUpPokemonTask.Execute(cancellationToken);
                        }
                        if (_logicSettings.TransferDuplicatePokemon)
                        {
                            await _transferDuplicatePokemonTask.Execute(cancellationToken);
                        }
                        if (_logicSettings.RenamePokemon)
                        {
                            await _renamePokemonTask.Execute(cancellationToken);
                        }
                        if (++displayStatsHit >= 4)
                        {
                            await _displayPokemonStatsTask.Execute();
                            displayStatsHit = 0;
                        }
                    }

                    if (_logicSettings.SnipeAtPokestops || _logicSettings.UseSnipeLocationServer)
                    {
                        await _snipePokemonTask.Execute(cancellationToken);
                    }
                }
            }
        }

        public async Task NoTeleport(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var distanceFromStart = _locationUtils.CalculateDistanceInMeters(
                _settings.DefaultLatitude, _settings.DefaultLongitude,
                _client.CurrentLatitude, _client.CurrentLongitude);

            // Edge case for when the client somehow ends up outside the defined radius
            if (_logicSettings.MaxTravelDistanceInMeters != 0 &&
                distanceFromStart > _logicSettings.MaxTravelDistanceInMeters)
            {
                _eventDispatcher.Send(new WarnEvent()
                {
                    Message = _translation.GetTranslation(TranslationString.FarmPokestopsOutsideRadius, distanceFromStart)
                });
                await Task.Delay(1000, cancellationToken);

                await _navigation.HumanLikeWalking(
                    new GeoCoordinate(_settings.DefaultLatitude, _settings.DefaultLongitude),
                    _logicSettings.WalkingSpeedInKilometerPerHour, null, cancellationToken);
            }

            var pokestopList = await GetPokeStops();
            var stopsHit = 0;
            var displayStatsHit = 0;

            if (pokestopList.Count <= 0)
            {
                _eventDispatcher.Send(new WarnEvent
                {
                    Message = _translation.GetTranslation(TranslationString.FarmPokestopsNoUsableFound)
                });
            }

            while (pokestopList.Any())
            {
                cancellationToken.ThrowIfCancellationRequested();

                //resort
                pokestopList =
                    pokestopList.OrderBy(
                        i =>
                            _locationUtils.CalculateDistanceInMeters(_client.CurrentLatitude,
                                _client.CurrentLongitude, i.Latitude, i.Longitude)).ToList();
                var pokeStop = pokestopList[0];
                pokestopList.RemoveAt(0);

                var distance = _locationUtils.CalculateDistanceInMeters(_client.CurrentLatitude,
                    _client.CurrentLongitude, pokeStop.Latitude, pokeStop.Longitude);
                var fortInfo = await _client.Fort.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);

                _eventDispatcher.Send(new FortTargetEvent { Id = fortInfo.FortId, Name = fortInfo.Name, Distance = distance,Latitude = fortInfo.Latitude, Longitude = fortInfo.Longitude, Description = fortInfo.Description, url = fortInfo.ImageUrls?.Count > 0 ? fortInfo.ImageUrls[0] : ""});
                if (_logicSettings.Teleport)
                    await _navigation.Teleport(new GeoCoordinate(fortInfo.Latitude, fortInfo.Longitude,
                       _client.Settings.DefaultAltitude));
                else
                {
                    await _navigation.HumanLikeWalking(new GeoCoordinate(pokeStop.Latitude, pokeStop.Longitude),
                    _logicSettings.WalkingSpeedInKilometerPerHour,
                    async () =>
                    {
                        if (_logicSettings.CatchWildPokemon)
                        {
                            // Catch normal map Pokemon
                            await _catchNearbyPokemonsTask.Execute(cancellationToken);
                            //Catch Incense Pokemon
                            await _catchIncensePokemonsTask.Execute(cancellationToken);
                        }
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
                        await _client.Fort.SearchFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                    if (fortSearch.Result == FortSearchResponse.Types.Result.InventoryFull)
                    {
                        await _recycleItemsTask.Execute(cancellationToken);
                    }
                    if (fortSearch.ExperienceAwarded > 0 && timesZeroXPawarded > 0) timesZeroXPawarded = 0;
                    if (fortSearch.ExperienceAwarded == 0)
                    {
                        timesZeroXPawarded++;

                        if (timesZeroXPawarded > zeroCheck)
                        {
                            if ((int)fortSearch.CooldownCompleteTimestampMs != 0)
                            {
                                break;
                                // Check if successfully looted, if so program can continue as this was "false alarm".
                            }

                            fortTry += 1;

                            _eventDispatcher.Send(new FortFailedEvent
                            {
                                Name = fortInfo.Name,
                                Try = fortTry,
                                Max = retryNumber - zeroCheck
                            });
                            if (_logicSettings.Teleport)
                                await Task.Delay(_logicSettings.DelaySoftbanRetry, cancellationToken);
                            else
                                await _delayingUtils.Delay(_logicSettings.DelayBetweenPlayerActions, 400);
                        }
                    }
                    else
                    {
                        _eventDispatcher.Send(new FortUsedEvent
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


                if (_logicSettings.Teleport)
                    await Task.Delay(_logicSettings.DelayPokestop);
                else
                    await Task.Delay(1000, cancellationToken);


                //Catch Lure Pokemon

                if (_logicSettings.CatchWildPokemon)
                {
                    if (pokeStop.LureInfo != null)
                    {
                        await _catchLurePokemonsTask.Execute(pokeStop, cancellationToken);
                    }
                    if (_logicSettings.Teleport)
                        await _catchNearbyPokemonsTask.Execute(cancellationToken);
                }
                await _eggWalker.ApplyDistance(distance, cancellationToken);

                if (++stopsHit % 5 == 0) //TODO: OR item/pokemon bag is full
                {
                    stopsHit = 0;
                    // need updated stardust information for upgrading, so refresh your profile now
                    await DownloadProfile();

                    if (fortSearch.ItemsAwarded.Count > 0)
                    {
                        await _inventory.RefreshCachedInventory();
                    }
                    await _recycleItemsTask.Execute(cancellationToken);
                    if (_logicSettings.EvolveAllPokemonWithEnoughCandy ||
                        _logicSettings.EvolveAllPokemonAboveIv)
                    {
                        await _evolvePokemonTask.Execute(cancellationToken);
                    }
                    if (_logicSettings.AutomaticallyLevelUpPokemon)
                    {
                        await _levelUpPokemonTask.Execute(cancellationToken);
                    }
                    if (_logicSettings.TransferDuplicatePokemon)
                    {
                        await _transferDuplicatePokemonTask.Execute(cancellationToken);
                    }
                    if (_logicSettings.RenamePokemon)
                    {
                        await _renamePokemonTask.Execute(cancellationToken);
                    }
                    if (++displayStatsHit >= 4)
                    {
                        await _displayPokemonStatsTask.Execute();
                        displayStatsHit = 0;
                    }
                }

                if (_logicSettings.SnipeAtPokestops || _logicSettings.UseSnipeLocationServer)
                {
                    await _snipePokemonTask.Execute(cancellationToken);
                }
            }
        }

        private async Task<List<FortData>> GetPokeStops()
        {
            var mapObjects = await _client.Map.GetMapObjects();

            var pokeStops = mapObjects.Item1.MapCells.SelectMany(i => i.Forts);

            _eventDispatcher.Send(new PokeStopListEvent { Forts = pokeStops.ToList() });

            // Wasn't sure how to make this pretty. Edit as needed.
            if (_logicSettings.Teleport)
            {
                pokeStops = mapObjects.Item1.MapCells.SelectMany(i => i.Forts)
                    .Where(
                        i =>
                            i.Type == FortType.Checkpoint &&
                            i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime() &&
                            ( // Make sure PokeStop is within max travel distance, unless it's set to 0.
                                _locationUtils.CalculateDistanceInMeters(
                                    _client.CurrentLatitude, _client.CurrentLongitude,
                                    i.Latitude, i.Longitude) < _logicSettings.MaxTravelDistanceInMeters) ||
                            _logicSettings.MaxTravelDistanceInMeters == 0
                    );
            }
            else
            {
                pokeStops = mapObjects.Item1.MapCells.SelectMany(i => i.Forts)
                    .Where(
                        i =>
                            i.Type == FortType.Checkpoint &&
                            i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime() &&
                            ( // Make sure PokeStop is within max travel distance, unless it's set to 0.
                                _locationUtils.CalculateDistanceInMeters(
                                    _settings.DefaultLatitude, _settings.DefaultLongitude,
                                    i.Latitude, i.Longitude) < _logicSettings.MaxTravelDistanceInMeters) ||
                            _logicSettings.MaxTravelDistanceInMeters == 0
                    );
            }

            return pokeStops.ToList();
        }

        // static copy of download profile, to update stardust more accurately
        private async Task DownloadProfile()
        {
            _eventDispatcher.Send(new ProfileEvent { Profile = await _client.Player.GetPlayer() });
        }
    }
}
