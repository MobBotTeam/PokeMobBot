#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Utils;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Map.Fort;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class UseNearbyPokestopsTask
    {
        private readonly RecycleItemsTask _recycleItemsTask;
        private readonly TransferDuplicatePokemonTask _transferDuplicatePokemonTask;
        private readonly LocationUtils _locationUtils;
        private readonly StringUtils _stringUtils;
        private readonly Client _client;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ILogicSettings _logicSettings;

        public UseNearbyPokestopsTask(RecycleItemsTask recycleItemsTask, TransferDuplicatePokemonTask transferDuplicatePokemonTask, LocationUtils locationUtils, StringUtils stringUtils, Client client, IEventDispatcher eventDispatcher, ILogicSettings logicSettings)
        {
            _recycleItemsTask = recycleItemsTask;
            _transferDuplicatePokemonTask = transferDuplicatePokemonTask;
            _locationUtils = locationUtils;
            _stringUtils = stringUtils;
            _client = client;
            _eventDispatcher = eventDispatcher;
            _logicSettings = logicSettings;
        }

        //Please do not change GetPokeStops() in this file, it's specifically set
        //to only find stops within 40 meters
        //this is for gpx pathing, we are not going to the pokestops,
        //so do not make it more than 40 because it will never get close to those stops.
        public async Task Execute(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pokestopList = await GetPokeStops();

            while (pokestopList.Any())
            {
                cancellationToken.ThrowIfCancellationRequested();

                pokestopList =
                    pokestopList.OrderBy(
                        i =>
                            _locationUtils.CalculateDistanceInMeters(_client.CurrentLatitude,
                                _client.CurrentLongitude, i.Latitude, i.Longitude)).ToList();
                var pokeStop = pokestopList[0];
                pokestopList.RemoveAt(0);

                var fortInfo = await _client.Fort.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);

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

                await _recycleItemsTask.Execute(cancellationToken);

                if (_logicSettings.TransferDuplicatePokemon)
                {
                    await _transferDuplicatePokemonTask.Execute(cancellationToken);
                }
            }
        }


        private async Task<List<FortData>> GetPokeStops()
        {
            var mapObjects = await _client.Map.GetMapObjects();

            // Wasn't sure how to make this pretty. Edit as needed.
            var pokeStops = mapObjects.Item1.MapCells.SelectMany(i => i.Forts)
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