#region using directives

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.State;
using PoGo.PokeMobBot.Logic.PoGoUtils;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using Newtonsoft.Json.Linq;
using PokemonGo.RocketAPI;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class SniperInfo
    {
        public ulong EncounterId { get; set; }
        public DateTime ExpirationTimestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public PokemonId Id { get; set; }
        public string SpawnPointId { get; set; }
        public PokemonMove Move1 { get; set; }
        public PokemonMove Move2 { get; set; }
        public double IV { get; set; }

        [JsonIgnore]
        public DateTime TimeStampAdded { get; set; } = DateTime.Now;
    }

    public class PokemonLocation
    {
        public PokemonLocation(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public long Id { get; set; }
        [JsonProperty("expires")]
        public double ExpirationTime { get; set; }
        [JsonProperty("latitude")]
        public double latitude { get; set; }
        [JsonProperty("longitude")]
        public double longitude { get; set; }
        public int PokemonId { get; set; }
        [JsonProperty("pokemon_id")]
        public PokemonId PokemonName { get; set; }

        public bool Equals(PokemonLocation obj)
        {
            return Math.Abs(latitude - obj.latitude) < 0.0001 && Math.Abs(longitude - obj.longitude) < 0.0001;
        }

        public override bool Equals(object obj) // contains calls this here
        {
            var p = obj as PokemonLocation;
            if (p == null) // no cast available
            {
                return false;
            }

            return Math.Abs(latitude - p.latitude) < 0.0001 && Math.Abs(longitude - p.longitude) < 0.0001;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return latitude.ToString("0.0000") + ", " + longitude.ToString("0.0000");
        }
    }

    public class ScanResult
    {
        public string Error { get; set; }
        [JsonProperty("pokemons")]
        public List<PokemonLocation> Pokemon { get; set; }
    }

    public class SnipePokemonTask
    {
        private readonly List<SniperInfo> _snipeLocations = new List<SniperInfo>();
        private readonly Inventory _inventory;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ITranslation _translation;
        private readonly ILogicSettings _logicSettings;
        private readonly Client _client;
        private readonly CatchPokemonTask _catchPokemonTask;

        public List<PokemonLocation> LocsVisited = new List<PokemonLocation>();
        private DateTime _lastSnipe = DateTime.MinValue;
        private string _pokeSniperURI = "http://pokesnipers.com/api/v1/pokemon.json";

        public SnipePokemonTask(Inventory inventory, IEventDispatcher eventDispatcher, ITranslation translation, ILogicSettings logicSettings, Client client, CatchPokemonTask catchPokemonTask)
        {
            _inventory = inventory;
            _eventDispatcher = eventDispatcher;
            _translation = translation;
            _logicSettings = logicSettings;
            _client = client;
            _catchPokemonTask = catchPokemonTask;
        }

        public Task AsyncStart(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() => Start(cancellationToken), cancellationToken);
        }

        public async Task<bool> CheckPokeballsToSnipe(int minPokeballs, CancellationToken cancellationToken)
        {
            var pokeBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemPokeBall);
            pokeBallsCount += await _inventory.GetItemAmountByType(ItemId.ItemGreatBall);
            pokeBallsCount += await _inventory.GetItemAmountByType(ItemId.ItemUltraBall);
            pokeBallsCount += await _inventory.GetItemAmountByType(ItemId.ItemMasterBall);

            if (pokeBallsCount < minPokeballs)
            {
                _eventDispatcher.Send(new NoticeEvent
                {
                    Message =
                        _translation.GetTranslation(TranslationString.NotEnoughPokeballsToSnipe, pokeBallsCount,
                            minPokeballs)
                });
                return false;
            }

            return true;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            if (_lastSnipe.AddMilliseconds(_logicSettings.MinDelayBetweenSnipes) > DateTime.Now)
                return;

            if (await CheckPokeballsToSnipe(_logicSettings.MinPokeballsToSnipe, cancellationToken))
            {
                if (_logicSettings.PokemonToSnipe != null)
                {
                    var st = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var t = DateTime.Now.ToUniversalTime() - st;
                    var currentTimestamp = t.TotalMilliseconds;

                    var pokemonIds = _logicSettings.PokemonToSnipe.Pokemon;

                    if (_logicSettings.UseSnipeLocationServer)
                    {
                        var locationsToSnipe = _snipeLocations; // inital
                        if (_logicSettings.UsePokeSnipersLocationServer)
                        {
                            locationsToSnipe = new List<SniperInfo>(_snipeLocations);
                            locationsToSnipe = locationsToSnipe.Where(q => q.Id == PokemonId.Missingno || pokemonIds.Contains(q.Id)).ToList();
                        }
                        else
                        {
                            locationsToSnipe = _snipeLocations?.Where(q =>
                            (!_logicSettings.UseTransferIvForSnipe ||
                             (q.IV == 0 && !_logicSettings.SnipeIgnoreUnknownIv) ||
                             (q.IV >= _inventory.GetPokemonTransferFilter(q.Id).KeepMinIvPercentage)) &&
                            !LocsVisited.Contains(new PokemonLocation(q.Latitude, q.Longitude))
                            && !(q.ExpirationTimestamp != default(DateTime) &&
                                 q.ExpirationTimestamp > new DateTime(2016) &&
                                 // make absolutely sure that the server sent a correct datetime
                                 q.ExpirationTimestamp < DateTime.Now) &&
                            (q.Id == PokemonId.Missingno || pokemonIds.Contains(q.Id))).ToList() ??
                                               new List<SniperInfo>();
                        }

                        _lastSnipe = DateTime.Now;

                        if (locationsToSnipe.Any())
                        {
                            foreach (var location in locationsToSnipe)
                            {
                                _eventDispatcher.Send(new SnipeScanEvent
                                {
                                    Bounds = new Location(location.Latitude, location.Longitude),
                                    PokemonId = location.Id,
                                    Iv = location.IV
                                });

                                if (
                                    !await
                                        CheckPokeballsToSnipe(_logicSettings.MinPokeballsWhileSnipe + 1, cancellationToken))
                                    return;

                                if (!await
                                    Snipe(pokemonIds, location.Latitude, location.Longitude, cancellationToken))
                                {
                                    return;
                                }
                                LocsVisited.Add(new PokemonLocation(location.Latitude, location.Longitude));
                            }
                        }
                    }
                    else
                    {
                        foreach (var location in _logicSettings.PokemonToSnipe.Locations)
                        {
                            _eventDispatcher.Send(new SnipeScanEvent
                            {
                                Bounds = location,
                                PokemonId = PokemonId.Missingno
                            });

                            var scanResult = SnipeScanForPokemon(location); // initialize
                            List<PokemonLocation> locationsToSnipe = new List<PokemonLocation>();

                            if (_logicSettings.UsePokeSnipersLocationServer)
                            {
                                PokeSniperScanForPokemon(location);
                            }
                            else
                            {
                                if (scanResult.Pokemon != null)
                                {
                                    var filteredPokemon = scanResult.Pokemon.Where(q => pokemonIds.Contains((PokemonId)q.PokemonName));
                                    var notVisitedPokemon = filteredPokemon.Where(q => !LocsVisited.Contains(q));
                                    var notExpiredPokemon = notVisitedPokemon.Where(q => q.ExpirationTime < currentTimestamp);

                                    locationsToSnipe.AddRange(notExpiredPokemon);
                                }
                            }

                            if (scanResult.Pokemon != null)
                            {
                                var filteredPokemon = scanResult.Pokemon.Where(q => pokemonIds.Contains((PokemonId)q.PokemonName));
                                var notVisitedPokemon = filteredPokemon.Where(q => !LocsVisited.Contains(q));
                                var notExpiredPokemon = notVisitedPokemon.Where(q => q.ExpirationTime < currentTimestamp);

                                locationsToSnipe.AddRange(notExpiredPokemon);
                            }

                            _lastSnipe = DateTime.Now;

                            if (locationsToSnipe.Any())
                            {
                                foreach (var pokemonLocation in locationsToSnipe)
                                {
                                    if (
                                        !await
                                            CheckPokeballsToSnipe(_logicSettings.MinPokeballsWhileSnipe + 1, cancellationToken))
                                        return;

                                    if (!await
                                        Snipe(pokemonIds, pokemonLocation.latitude, pokemonLocation.longitude,
                                            cancellationToken))
                                    {
                                        return;
                                    }

                                    LocsVisited.Add(pokemonLocation);
                                }
                            }
                            else
                            {
                                _eventDispatcher.Send(new NoticeEvent
                                {
                                    Message = _translation.GetTranslation(TranslationString.NoPokemonToSnipe)
                                });
                            }
                        }
                    }
                }
            }
        }

        private async Task<bool> Snipe(IEnumerable<PokemonId> pokemonIds, double Latitude, double Longitude, CancellationToken cancellationToken)
        {
            var CurrentLatitude = _client.CurrentLatitude;
            var CurrentLongitude = _client.CurrentLongitude;

            _eventDispatcher.Send(new SnipeModeEvent { Active = true });

            await
                _client.Player.UpdatePlayerLocation(Latitude,
                    Longitude, _client.CurrentAltitude);

            _eventDispatcher.Send(new UpdatePositionEvent
            {
                Longitude = Longitude,
                Latitude = Latitude
            });

            var mapObjects = await _client.Map.GetMapObjects();
            var catchablePokemon = mapObjects.Item1.MapCells.SelectMany(q => q.CatchablePokemons)
                    .Where(q => pokemonIds.Contains(q.PokemonId))
                    .ToList();

            await _client.Player.UpdatePlayerLocation(CurrentLatitude, CurrentLongitude,
                _client.CurrentAltitude);

            foreach (var pokemon in catchablePokemon)
            {
                cancellationToken.ThrowIfCancellationRequested();

                EncounterResponse encounter;
                try
                {
                    await
                        _client.Player.UpdatePlayerLocation(Latitude, Longitude, _client.CurrentAltitude);

                    encounter =
                        _client.Encounter.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnPointId).Result;
                }
                finally
                {
                    await
                        _client.Player.UpdatePlayerLocation(CurrentLatitude, CurrentLongitude, _client.CurrentAltitude);
                }

                if (encounter.Status == EncounterResponse.Types.Status.EncounterSuccess)
                {
                    _eventDispatcher.Send(new UpdatePositionEvent
                    {
                        Latitude = CurrentLatitude,
                        Longitude = CurrentLongitude
                    });

                    if (!await _catchPokemonTask.Execute(encounter, pokemon))
                    {
                        // Don't snipe any more pokemon if we ran out of one kind of pokeballs.
                        _eventDispatcher.Send(new SnipeModeEvent { Active = false });
                        return false;
                    }
                }
                else if (encounter.Status == EncounterResponse.Types.Status.PokemonInventoryFull)
                {
                    _eventDispatcher.Send(new WarnEvent
                    {
                        Message =
                            _translation.GetTranslation(
                                TranslationString.InvFullTransferManually)
                    });

                    // Don't snipe any more pokemon if inventory is full.
                    _eventDispatcher.Send(new SnipeModeEvent { Active = false });
                    return false;
                }
                else
                {
                    _eventDispatcher.Send(new WarnEvent
                    {
                        Message =
                            _translation.GetTranslation(
                                TranslationString.EncounterProblem, encounter.Status)
                    });
                }

                if (
                    !Equals(catchablePokemon.ElementAtOrDefault(catchablePokemon.Count - 1),
                        pokemon))
                {
                    await Task.Delay(_logicSettings.DelayBetweenPokemonCatch, cancellationToken);
                }
            }

            _eventDispatcher.Send(new SnipeModeEvent { Active = false });
            await Task.Delay(_logicSettings.DelayBetweenPlayerActions, cancellationToken);

            return true;
        }

        private ScanResult SnipeScanForPokemon(Location location)
        {
            var formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };

            var offset = _logicSettings.SnipingScanOffset;
            // 0.003 = half a mile; maximum 0.06 is 10 miles
            if (offset < 0.001) offset = 0.003;
            if (offset > 0.06) offset = 0.06;

            var boundLowerLeftLat = (location.Latitude - offset).ToString(formatter);
            var boundLowerLeftLng = (location.Longitude - offset).ToString(formatter);
            var boundUpperRightLat = (location.Latitude + offset).ToString(formatter);
            var boundUpperRightLng = (location.Longitude + offset).ToString(formatter);

            var uri = $"http://skiplagged.com/api/pokemon.php?bounds={boundLowerLeftLat},{boundLowerLeftLng},{boundUpperRightLat},{boundUpperRightLng}";

            ScanResult scanResult;

            try
            {
                var request = WebRequest.CreateHttp(uri);
                request.Accept = "application/json";
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
                request.Timeout = _logicSettings.SnipeRequestTimeoutSeconds;
                request.ReadWriteTimeout = 32000;

                var resp = request.GetResponse();
                var reader = new StreamReader(resp.GetResponseStream());
                var fullresp = reader.ReadToEnd();

                scanResult = JsonConvert.DeserializeObject<ScanResult>(fullresp);

                if (!string.IsNullOrEmpty(scanResult.Error))
                {
                    if (scanResult.Error.Contains("down for maintenance") || scanResult.Error.Contains("illegal request"))
                        _eventDispatcher.Send(new WarnEvent { Message = _translation.GetTranslation(TranslationString.SkipLaggedMaintenance) });
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError &&
                    ex.Response != null)
                {
                    var resp = (HttpWebResponse)ex.Response;
                    switch (resp.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            _eventDispatcher.Send(new WarnEvent { Message = _translation.GetTranslation(TranslationString.WebErrorNotFound) });
                            break;
                        case HttpStatusCode.GatewayTimeout:
                            _eventDispatcher.Send(new WarnEvent { Message = _translation.GetTranslation(TranslationString.WebErrorGatewayTimeout) });
                            break;
                        case HttpStatusCode.BadGateway:
                            _eventDispatcher.Send(new WarnEvent { Message = _translation.GetTranslation(TranslationString.WebErrorBadGateway) });
                            break;
                        default:
                            _eventDispatcher.Send(new WarnEvent { Message = ex.ToString() });
                            break;
                    }
                }
                else if (ex.Status == WebExceptionStatus.Timeout)
                {
                    _eventDispatcher.Send(new WarnEvent { Message = _translation.GetTranslation(TranslationString.SkipLaggedTimeout) });
                }
                else
                {
                    _eventDispatcher.Send(new ErrorEvent { Message = ex.ToString() });
                }

                scanResult = new ScanResult { Pokemon = new List<PokemonLocation>() };
            }
            catch (Exception ex)
            {
                _eventDispatcher.Send(new ErrorEvent { Message = ex.ToString() });
                scanResult = new ScanResult { Pokemon = new List<PokemonLocation>() };
            }

            return scanResult;
        }

        private ScanResult PokeSniperScanForPokemon(Location location)
        {
            var formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };

            var offset = _logicSettings.SnipingScanOffset;
            // 0.003 = half a mile; maximum 0.06 is 10 miles
            if (offset < 0.001) offset = 0.003;
            if (offset > 0.06) offset = 0.06;

            ScanResult scanResult;
            try
            {
                var request = WebRequest.CreateHttp(_pokeSniperURI);
                request.Accept = "application/json";
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
                request.Timeout = _logicSettings.SnipeRequestTimeoutSeconds;
                request.ReadWriteTimeout = 32000;

                var resp = request.GetResponse();
                var reader = new StreamReader(resp.GetResponseStream());
                var fullresp = reader.ReadToEnd().Replace(" M", "Male").Replace(" F", "Female");

                dynamic pokesniper = JsonConvert.DeserializeObject(fullresp);
                JArray results = pokesniper.results;
                _snipeLocations.Clear();

                foreach (var result in results)
                {
                    PokemonId id;
                    Enum.TryParse(result.Value<string>("name"), out id);
                    var a = new SniperInfo
                    {
                        Id = id,
                        IV = 100,
                        Latitude = Convert.ToDouble(result.Value<string>("coords").Split(',')[0]),
                        Longitude = Convert.ToDouble(result.Value<string>("coords").Split(',')[1]),
                        ExpirationTimestamp = DateTime.Now
                    };
                    _snipeLocations.Add(a);
                }
            }
            catch (Exception ex)
            {
                // most likely System.IO.IOException
                _eventDispatcher.Send(new ErrorEvent { Message = ex.ToString() });
                scanResult = new ScanResult { Pokemon = new List<PokemonLocation>() };
            }
            return null;
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (_logicSettings.UsePokeSnipersLocationServer)
                {
                    var st = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var t = DateTime.Now.ToUniversalTime() - st;
                    var currentTimestamp = t.TotalMilliseconds;
                    var pokemonIds = _logicSettings.PokemonToSnipe.Pokemon;

                    var formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };

                    var offset = _logicSettings.SnipingScanOffset;
                    // 0.003 = half a mile; maximum 0.06 is 10 miles
                    if (offset < 0.001) offset = 0.003;
                    if (offset > 0.06) offset = 0.06;

                    ScanResult scanResult;
                    try
                    {
                        var request = WebRequest.CreateHttp(_pokeSniperURI);
                        request.Accept = "application/json";
                        request.Method = "GET";
                        request.Timeout = _logicSettings.SnipeRequestTimeoutSeconds;
                        request.ReadWriteTimeout = 32000;

                        var resp = request.GetResponse();
                        var reader = new StreamReader(resp.GetResponseStream());
                        var fullresp = reader.ReadToEnd().Replace(" M", "Male").Replace(" F", "Female");

                        dynamic pokesniper = JsonConvert.DeserializeObject(fullresp);
                        JArray results = pokesniper.results;
                        _snipeLocations.Clear();

                        foreach (var result in results)
                        {
                            PokemonId id;
                            Enum.TryParse(result.Value<string>("name"), out id);
                            var a = new SniperInfo
                            {
                                Id = id,
                                IV = 100,
                                Latitude = Convert.ToDouble(result.Value<string>("coords").Split(',')[0]),
                                Longitude = Convert.ToDouble(result.Value<string>("coords").Split(',')[1]),
                                ExpirationTimestamp = DateTime.Now
                            };
                            _snipeLocations.Add(a);
                        }
                    }
                    catch (WebException ex)
                    {
                        if (ex.Status == WebExceptionStatus.ProtocolError &&
                            ex.Response != null)
                        {
                            var resp = (HttpWebResponse)ex.Response;
                            switch (resp.StatusCode)
                            {
                                case HttpStatusCode.NotFound:
                                    _eventDispatcher.Send(new WarnEvent { Message = _translation.GetTranslation(TranslationString.WebErrorNotFound) });
                                    break;
                                case HttpStatusCode.GatewayTimeout:
                                    _eventDispatcher.Send(new WarnEvent { Message = _translation.GetTranslation(TranslationString.WebErrorGatewayTimeout) });
                                    break;
                                case HttpStatusCode.BadGateway:
                                    _eventDispatcher.Send(new WarnEvent { Message = _translation.GetTranslation(TranslationString.WebErrorBadGateway) });
                                    break;
                                default:
                                    _eventDispatcher.Send(new WarnEvent { Message = ex.ToString() });
                                    break;
                            }
                        }
                        else if (ex.Status == WebExceptionStatus.Timeout)
                        {
                            _eventDispatcher.Send(new WarnEvent { Message = _translation.GetTranslation(TranslationString.SkipLaggedTimeout) });
                        }
                        else
                        {
                            _eventDispatcher.Send(new ErrorEvent { Message = ex.ToString() });
                        }

                        scanResult = new ScanResult { Pokemon = new List<PokemonLocation>() };
                    }
                    catch (Exception ex)
                    {
                        _eventDispatcher.Send(new ErrorEvent { Message = ex.ToString() });
                        scanResult = new ScanResult { Pokemon = new List<PokemonLocation>() };
                    }
                }
                else
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        var lClient = new TcpClient();
                        lClient.Connect(_logicSettings.SnipeLocationServer,
                            _logicSettings.SnipeLocationServerPort);

                        var sr = new StreamReader(lClient.GetStream());

                        while (lClient.Connected)
                        {
                            var line = sr.ReadLine();
                            if (line == null)
                                throw new Exception("Unable to ReadLine from sniper socket");

                            var info = JsonConvert.DeserializeObject<SniperInfo>(line);

                            if (_snipeLocations.Any(x =>
                                Math.Abs(x.Latitude - info.Latitude) < 0.0001 &&
                                Math.Abs(x.Longitude - info.Longitude) < 0.0001))
                                // we might have different precisions from other sources
                                continue;

                            _snipeLocations.RemoveAll(x => DateTime.Now > x.TimeStampAdded.AddMinutes(15));
                            _snipeLocations.Add(info);
                        }
                    }
                    catch (SocketException)
                    {
                        // this is spammed to often. Maybe add it to debug log later
                    }
                    catch (Exception ex)
                    {
                        // most likely System.IO.IOException
                        _eventDispatcher.Send(new ErrorEvent { Message = ex.ToString() });
                    }
                }
                await Task.Delay(5000, cancellationToken);
            }
        }
    }
}