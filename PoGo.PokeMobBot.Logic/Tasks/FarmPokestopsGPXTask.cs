#region using directives

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Utils;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Map.Fort;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class FarmPokestopsGpxTask
    {
        private readonly RecycleItemsTask _recycleItemsTask;
        private readonly EvolvePokemonTask _evolvePokemonTask;
        private readonly SnipePokemonTask _snipePokemonTask;
        private readonly CatchLurePokemonsTask _catchLurePokemonsTask;
        private readonly TransferDuplicatePokemonTask _transferDuplicatePokemonTask;
        private readonly RenamePokemonTask _renamePokemonTask;
        private readonly CatchNearbyPokemonsTask _catchNearbyPokemonsTask;
        private readonly CatchIncensePokemonsTask _catchIncensePokemonsTask;
        private readonly UseNearbyPokestopsTask _useNearbyPokestopsTask;
        private readonly StringUtils _stringUtils;
        private readonly LocationUtils _locationUtils;
        private readonly GpxReader _gpxReader;
        private readonly EggWalker _eggWalker;
        private readonly Client _client;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ITranslation _translation;
        private readonly Inventory _inventory;
        private readonly ILogicSettings _logicSettings;
        private readonly Navigation _navigation;
        private readonly TransferLowStatPokemonTask _transferLowStatPokemonTask;

        private DateTime _lastTasksCall = DateTime.Now;

        public FarmPokestopsGpxTask(RecycleItemsTask recycleItemsTask, EvolvePokemonTask evolvePokemonTask, SnipePokemonTask snipePokemonTask, EggWalker eggWalker, CatchLurePokemonsTask catchLurePokemonsTask, TransferDuplicatePokemonTask transferDuplicatePokemonTask, RenamePokemonTask renamePokemonTask, CatchNearbyPokemonsTask catchNearbyPokemonsTask, CatchIncensePokemonsTask catchIncensePokemonsTask, UseNearbyPokestopsTask useNearbyPokestopsTask, StringUtils stringUtils, LocationUtils locationUtils, GpxReader gpxReader, Client client, IEventDispatcher eventDispatcher, ITranslation translation, Inventory inventory, ILogicSettings logicSettings, Navigation navigation, TransferLowStatPokemonTask transferLowStatPokemonTask)
        {
            _recycleItemsTask = recycleItemsTask;
            _evolvePokemonTask = evolvePokemonTask;
            _snipePokemonTask = snipePokemonTask;
            _eggWalker = eggWalker;
            _catchLurePokemonsTask = catchLurePokemonsTask;
            _transferDuplicatePokemonTask = transferDuplicatePokemonTask;
            _renamePokemonTask = renamePokemonTask;
            _catchNearbyPokemonsTask = catchNearbyPokemonsTask;
            _catchIncensePokemonsTask = catchIncensePokemonsTask;
            _useNearbyPokestopsTask = useNearbyPokestopsTask;
            _stringUtils = stringUtils;
            _locationUtils = locationUtils;
            _gpxReader = gpxReader;
            _client = client;
            _eventDispatcher = eventDispatcher;
            _translation = translation;
            _inventory = inventory;
            _logicSettings = logicSettings;
            _navigation = navigation;
            _transferLowStatPokemonTask = transferLowStatPokemonTask;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            var tracks = GetGpxTracks();

            for (var curTrk = 0; curTrk < tracks.Count; curTrk++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var track = tracks.ElementAt(curTrk);
                var trackSegments = track.Segments;
                for (var curTrkSeg = 0; curTrkSeg < trackSegments.Count; curTrkSeg++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var trackPoints = track.Segments.ElementAt(0).TrackPoints;
                    for (var curTrkPt = 0; curTrkPt < trackPoints.Count; curTrkPt++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var nextPoint = trackPoints.ElementAt(curTrkPt);
                        var distance = _locationUtils.CalculateDistanceInMeters(_client.CurrentLatitude,
                            _client.CurrentLongitude,
                            Convert.ToDouble(nextPoint.Lat, CultureInfo.InvariantCulture),
                            Convert.ToDouble(nextPoint.Lon, CultureInfo.InvariantCulture));

                        if (distance > 5000)
                        {
                            _eventDispatcher.Send(new ErrorEvent
                            {
                                Message =
                                    _translation.GetTranslation(TranslationString.DesiredDestTooFar,
                                        nextPoint.Lat, nextPoint.Lon, _client.CurrentLatitude,
                                        _client.CurrentLongitude)
                            });
                            break;
                        }

                        var pokestopList = await GetPokeStops();
                        _eventDispatcher.Send(new PokeStopListEvent { Forts = pokestopList });

                        while (pokestopList.Any())
                        // warning: this is never entered due to ps cooldowns from UseNearbyPokestopsTask 
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            pokestopList =
                                pokestopList.OrderBy(
                                    i =>
                                        _locationUtils.CalculateDistanceInMeters(_client.CurrentLatitude,
                                            _client.CurrentLongitude, i.Latitude, i.Longitude)).ToList();
                            var pokeStop = pokestopList[0];
                            pokestopList.RemoveAt(0);

                            var fortInfo =
                                await _client.Fort.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);

                            if (pokeStop.LureInfo != null)
                            {
                                await _catchLurePokemonsTask.Execute(pokeStop, cancellationToken);
                            }

                            var fortSearch =
                                await _client.Fort.SearchFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);

                            if (fortSearch.ExperienceAwarded > 0)
                            {
                                _eventDispatcher.Send(new FortUsedEvent
                                {
                                    Id = pokeStop.Id,
                                    Name = fortInfo.Name,
                                    Exp = fortSearch.ExperienceAwarded,
                                    Gems = fortSearch.GemsAwarded,
                                    Items = _stringUtils.GetSummedFriendlyNameOfItemAwardList(fortSearch.ItemsAwarded),
                                    Latitude = pokeStop.Latitude,
                                    Longitude = pokeStop.Longitude
                                });
                            }
                            if (fortSearch.ItemsAwarded.Count > 0)
                            {
                                await _inventory.RefreshCachedInventory();
                            }
                        }

                        if (DateTime.Now > _lastTasksCall)
                        {
                            _lastTasksCall =
                                DateTime.Now.AddMilliseconds(Math.Min(_logicSettings.DelayBetweenPlayerActions,
                                    3000));

                            await _recycleItemsTask.Execute(cancellationToken);

                            if (_logicSettings.SnipeAtPokestops || _logicSettings.UseSnipeLocationServer)
                            {
                                await _snipePokemonTask.Execute(cancellationToken);
                            }

                            if (_logicSettings.EvolveAllPokemonWithEnoughCandy ||
                                _logicSettings.EvolveAllPokemonAboveIv)
                            {
                                await _evolvePokemonTask.Execute(cancellationToken);
                            }

                            if (_logicSettings.TransferDuplicatePokemon)
                            {
                                await _transferDuplicatePokemonTask.Execute(cancellationToken);
                            }
                            if (_logicSettings.TransferLowStatPokemon)
                            {
                                await _transferLowStatPokemonTask.Execute(cancellationToken);
                            }

                            if (_logicSettings.RenamePokemon)
                            {
                                await _renamePokemonTask.Execute(cancellationToken);
                            }
                        }

                        await _navigation.HumanPathWalking(
                            trackPoints.ElementAt(curTrkPt),
                            _logicSettings.WalkingSpeedInKilometerPerHour,
                            async () =>
                            {
                                await _catchNearbyPokemonsTask.Execute(cancellationToken);
                                //Catch Incense Pokemon
                                await _catchIncensePokemonsTask.Execute(cancellationToken);
                                await _useNearbyPokestopsTask.Execute(cancellationToken);
                                return true;
                            },
                            cancellationToken
                            );

                        await _eggWalker.ApplyDistance(distance, cancellationToken);
                    } //end trkpts
                } //end trksegs
            } //end tracks
        }

        private List<GpxReader.Trk> GetGpxTracks()
        {
            var xmlString = File.ReadAllText(_logicSettings.GpxFile);
            _gpxReader.Read(xmlString);
            return _gpxReader.Tracks;
        }

        //Please do not change GetPokeStops() in this file, it's specifically set
        //to only find stops within 40 meters
        //this is for gpx pathing, we are not going to the pokestops,
        //so do not make it more than 40 because it will never get close to those stops.
        private async Task<List<FortData>> GetPokeStops()
        {
            var mapObjects = await _client.Map.GetMapObjects();

            // Wasn't sure how to make this pretty. Edit as needed.
            var pokeStops = mapObjects.MapCells.SelectMany(i => i.Forts)
                .Where(
                    i =>
                        i.Type == FortType.Checkpoint &&
                        i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime() &&
                        ( // Make sure PokeStop is within 40 meters or else it is pointless to hit it
                            _locationUtils.CalculateDistanceInMeters(
                                _client.CurrentLatitude, _client.CurrentLongitude,
                                i.Latitude, i.Longitude) < 40) ||
                        _logicSettings.MaxTravelDistanceInMeters == 0
                );

            return pokeStops.ToList();
        }
    }
}