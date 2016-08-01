﻿#region using directives

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

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class SniperInfo
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double Iv { get; set; }
        public DateTime TimeStamp { get; set; }
        public PokemonId Id { get; set; }

        [JsonIgnore]
        public DateTime TimeStampAdded { get; set; } = DateTime.Now;
    }

    public class PokemonLocation
    {
        public PokemonLocation(double lat, double lon)
        {
            latitude = lat;
            longitude = lon;
        }

        public long Id { get; set; }
        public double expires { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int pokemon_id { get; set; }
        public PokemonId pokemon_name { get; set; }

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
        public string Status { get; set; }
        public List<PokemonLocation> pokemons { get; set; }
    }

    public static class SnipePokemonTask
    {
        public static List<PokemonLocation> LocsVisited = new List<PokemonLocation>();
        private static readonly List<SniperInfo> SnipeLocations = new List<SniperInfo>();
        private static DateTime _lastSnipe = DateTime.MinValue;

        public static Task AsyncStart(Session session, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() => Start(session, cancellationToken), cancellationToken);
        }

        public static async Task<bool> CheckPokeballsToSnipe(int minPokeballs, ISession session,
            CancellationToken cancellationToken)
        {
            var pokeBallsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemPokeBall);
            pokeBallsCount += await session.Inventory.GetItemAmountByType(ItemId.ItemGreatBall);
            pokeBallsCount += await session.Inventory.GetItemAmountByType(ItemId.ItemUltraBall);
            pokeBallsCount += await session.Inventory.GetItemAmountByType(ItemId.ItemMasterBall);

            if (pokeBallsCount < minPokeballs)
            {
                session.EventDispatcher.Send(new NoticeEvent
                {
                    Message =
                        session.Translation.GetTranslation(TranslationString.NotEnoughPokeballsToSnipe, pokeBallsCount,
                            minPokeballs)
                });
                return false;
            }

            return true;
        }

        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            if (_lastSnipe.AddMilliseconds(session.LogicSettings.MinDelayBetweenSnipes) > DateTime.Now)
                return;

            if (await CheckPokeballsToSnipe(session.LogicSettings.MinPokeballsToSnipe, session, cancellationToken))
            {
                if (session.LogicSettings.PokemonToSnipe != null)
                {
                    var st = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var t = DateTime.Now.ToUniversalTime() - st;
                    var currentTimestamp = t.TotalMilliseconds;

                    var pokemonIds = session.LogicSettings.PokemonToSnipe.Pokemon;

                    if (session.LogicSettings.UseSnipeLocationServer)
                    {
                        var locationsToSnipe = SnipeLocations; // inital
                        if (session.LogicSettings.UseSnipeOnlineLocationServer)
                        {
                            locationsToSnipe = new List<SniperInfo>(SnipeLocations);
                            locationsToSnipe = locationsToSnipe.Where(q => q.Id == PokemonId.Missingno || pokemonIds.Contains(q.Id)).ToList();
                        }
                        else
                        {
                            locationsToSnipe = SnipeLocations?.Where(q =>
                            (!session.LogicSettings.UseTransferIvForSnipe ||
                             (q.Iv == 0 && !session.LogicSettings.SnipeIgnoreUnknownIv) ||
                             (q.Iv >= session.Inventory.GetPokemonTransferFilter(q.Id).KeepMinIvPercentage)) &&
                            !LocsVisited.Contains(new PokemonLocation(q.latitude, q.longitude))
                            && !(q.TimeStamp != default(DateTime) &&
                                 q.TimeStamp > new DateTime(2016) &&
                                 // make absolutely sure that the server sent a correct datetime
                                 q.TimeStamp < DateTime.Now) &&
                            (q.Id == PokemonId.Missingno || pokemonIds.Contains(q.Id))).ToList() ??
                                               new List<SniperInfo>();
                        }

                        if (locationsToSnipe.Any())
                        {
                            _lastSnipe = DateTime.Now;
                            foreach (var location in locationsToSnipe)
                            {
                                session.EventDispatcher.Send(new SnipeScanEvent
                                {
                                    Bounds = new Location(location.latitude, location.longitude),
                                    PokemonId = location.Id,
                                    Iv = location.Iv
                                });

                                if (
                                    !await
                                        CheckPokeballsToSnipe(session.LogicSettings.MinPokeballsWhileSnipe + 1, session,
                                            cancellationToken))
                                    return;

                                await
                                    Snipe(session, pokemonIds, location.latitude, location.longitude, cancellationToken);
                                LocsVisited.Add(new PokemonLocation(location.latitude, location.longitude));
                            }
                        }
                    }
                    else
                    {
                        foreach (var location in session.LogicSettings.PokemonToSnipe.Locations)
                        {
                            session.EventDispatcher.Send(new SnipeScanEvent
                            {
                                Bounds = location,
                                PokemonId = PokemonId.Missingno
                            });

                            var scanResult = SnipeScanForPokemon(session, location); // initialize
                            List<PokemonLocation> locationsToSnipe = new List<PokemonLocation>();

                            if (session.LogicSettings.UseSnipeOnlineLocationServer)
                            {
                                OnlineSnipeScanForPokemon(session, location);
                            }
                            else
                            {
                                scanResult = SnipeScanForPokemon(session, location);
                                if (scanResult.pokemons != null)
                                {
                                    var filteredPokemon = scanResult.pokemons.Where(q => pokemonIds.Contains((PokemonId)q.pokemon_name));
                                    var notVisitedPokemon = filteredPokemon.Where(q => !LocsVisited.Contains(q));
                                    var notExpiredPokemon = notVisitedPokemon.Where(q => q.expires < currentTimestamp);

                                    locationsToSnipe.AddRange(notExpiredPokemon);
                                }
                            }

                            if (scanResult.pokemons != null)
                            {
                                var filteredPokemon = scanResult.pokemons.Where(q => pokemonIds.Contains((PokemonId)q.pokemon_name));
                                var notVisitedPokemon = filteredPokemon.Where(q => !LocsVisited.Contains(q));
                                var notExpiredPokemon = notVisitedPokemon.Where(q => q.expires < currentTimestamp);

                                locationsToSnipe.AddRange(notExpiredPokemon);
                            }

                            if (locationsToSnipe.Any())
                            {
                                foreach (var pokemonLocation in locationsToSnipe)
                                {
                                    if (
                                        !await
                                            CheckPokeballsToSnipe(session.LogicSettings.MinPokeballsWhileSnipe + 1,
                                                session, cancellationToken))
                                        return;

                                    LocsVisited.Add(pokemonLocation);

                                    await
                                        Snipe(session, pokemonIds, pokemonLocation.latitude, pokemonLocation.longitude,
                                            cancellationToken);
                                }
                            }
                            else
                            {
                                session.EventDispatcher.Send(new NoticeEvent
                                {
                                    Message = session.Translation.GetTranslation(TranslationString.NoPokemonToSnipe)
                                });
                            }

                            _lastSnipe = DateTime.Now;
                        }
                    }
                }
            }
        }

        private static async Task Snipe(ISession session, IEnumerable<PokemonId> pokemonIds, double latitude,
            double longitude, CancellationToken cancellationToken)
        {
            var CurrentLatitude = session.Client.CurrentLatitude;
            var CurrentLongitude = session.Client.CurrentLongitude;

            session.EventDispatcher.Send(new SnipeModeEvent { Active = true });

            await
                session.Client.Player.UpdatePlayerLocation(latitude,
                    longitude, session.Client.CurrentAltitude);

            session.EventDispatcher.Send(new UpdatePositionEvent
            {
                Longitude = longitude,
                Latitude = latitude
            });

            var mapObjects = session.Client.Map.GetMapObjects().Result;
            var catchablePokemon =
                mapObjects.MapCells.SelectMany(q => q.CatchablePokemons)
                    .Where(q => pokemonIds.Contains(q.PokemonId))
                    .ToList();

            await session.Client.Player.UpdatePlayerLocation(CurrentLatitude, CurrentLongitude,
                session.Client.CurrentAltitude);

            foreach (var pokemon in catchablePokemon)
            {
                cancellationToken.ThrowIfCancellationRequested();

                EncounterResponse encounter;
                try
                {
                    await
                        session.Client.Player.UpdatePlayerLocation(latitude, longitude, session.Client.CurrentAltitude);

                    encounter =
                        session.Client.Encounter.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnPointId).Result;
                }
                finally
                {
                    await
                        session.Client.Player.UpdatePlayerLocation(CurrentLatitude, CurrentLongitude, session.Client.CurrentAltitude);
                }

                if (encounter.Status == EncounterResponse.Types.Status.EncounterSuccess)
                {
                    session.EventDispatcher.Send(new UpdatePositionEvent
                    {
                        Latitude = CurrentLatitude,
                        Longitude = CurrentLongitude
                    });

                    await CatchPokemonTask.Execute(session, encounter, pokemon);
                }
                else if (encounter.Status == EncounterResponse.Types.Status.PokemonInventoryFull)
                {
                    session.EventDispatcher.Send(new WarnEvent
                    {
                        Message =
                            session.Translation.GetTranslation(
                                TranslationString.InvFullTransferManually)
                    });
                }
                else
                {
                    session.EventDispatcher.Send(new WarnEvent
                    {
                        Message =
                            session.Translation.GetTranslation(
                                TranslationString.EncounterProblem, encounter.Status)
                    });
                }

                if (
                    !Equals(catchablePokemon.ElementAtOrDefault(catchablePokemon.Count - 1),
                        pokemon))
                {
                    await Task.Delay(session.LogicSettings.DelayBetweenPokemonCatch, cancellationToken);
                }
            }

            session.EventDispatcher.Send(new SnipeModeEvent { Active = false });
            await Task.Delay(session.LogicSettings.DelayBetweenPlayerActions, cancellationToken);
        }

        private static ScanResult SnipeScanForPokemon(ISession session, Location location)
        {
            var formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };

            var offset = session.LogicSettings.SnipingScanOffset;
            // 0.003 = half a mile; maximum 0.06 is 10 miles
            if (offset < 0.001) offset = 0.003;
            if (offset > 0.06) offset = 0.06;

            var boundLowerLeftLat = location.Latitude - offset;
            var boundLowerLeftLng = location.Longitude - offset;
            var boundUpperRightLat = location.Latitude + offset;
            var boundUpperRightLng = location.Longitude + offset;

            var uri =
                $"http://skiplagged.com/api/pokemon.php?bounds={boundLowerLeftLat.ToString(formatter)},{boundLowerLeftLng.ToString(formatter)},{boundUpperRightLat.ToString(formatter)},{boundUpperRightLng.ToString(formatter)}";
            /*var uri =
                $"http://skiplagged.com/api/pokemon.php?address={location.Latitude.ToString(formatter)},{location.Longitude.ToString(formatter)}";
                */
            /*
             * http://skiplagged.com/api/pokemon.php?bounds=40.76356269219236,-73.98657795715332,40.7854671345488,-73.95812508392333
             * bounds = bound_lower_left_lat,bound_lower_left_lng,bound_upper_right_lat,bound_upper_right_lng
             */

            ScanResult scanResult;
            try
            {
                var request = WebRequest.CreateHttp(uri);
                request.Accept = "application/json";
                request.Method = "GET";
                request.Timeout = 5000;
                request.ReadWriteTimeout = 32000;

                var resp = request.GetResponse();
                var reader = new StreamReader(resp.GetResponseStream());
                var fullresp = reader.ReadToEnd().Replace(" M", "Male").Replace(" F", "Female");

                scanResult = JsonConvert.DeserializeObject<ScanResult>(fullresp);
            }
            catch (Exception ex)
            {
                // most likely System.IO.IOException
                session.EventDispatcher.Send(new ErrorEvent { Message = ex.ToString() });
                scanResult = new ScanResult
                {
                    Status = "fail",
                    pokemons = new List<PokemonLocation>()
                };
            }
            return scanResult;
        }

        private static ScanResult OnlineSnipeScanForPokemon(ISession session, Location location)
        {
            var formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };

            var offset = session.LogicSettings.SnipingScanOffset;
            // 0.003 = half a mile; maximum 0.06 is 10 miles
            if (offset < 0.001) offset = 0.003;
            if (offset > 0.06) offset = 0.06;

            var uri =
                $"http://pokesnipers.com/api/v1/pokemon.json";

            ScanResult scanResult;
            try
            {
                var request = WebRequest.CreateHttp(uri);
                request.Accept = "application/json";
                request.Method = "GET";
                request.Timeout = 5000;
                request.ReadWriteTimeout = 32000;

                var resp = request.GetResponse();
                var reader = new StreamReader(resp.GetResponseStream());
                var fullresp = reader.ReadToEnd().Replace(" M", "Male").Replace(" F", "Female");

                dynamic pokesniper = JsonConvert.DeserializeObject(fullresp);
                JArray results = pokesniper.results;
                SnipeLocations.Clear();

                foreach (var result in results)
                {
                    PokemonId id;
                    Enum.TryParse(result.Value<string>("name"), out id);
                    var a = new SniperInfo
                    {
                        Id = id,
                        Iv = 100,
                        latitude = Convert.ToDouble(result.Value<string>("coords").Split(',')[0]),
                        longitude = Convert.ToDouble(result.Value<string>("coords").Split(',')[1]),
                        TimeStamp = DateTime.Now
                    };
                    SnipeLocations.Add(a);
                }
            }
            catch (Exception ex)
            {
                // most likely System.IO.IOException
                session.EventDispatcher.Send(new ErrorEvent { Message = ex.ToString() });
                scanResult = new ScanResult
                {
                    Status = "fail",
                    pokemons = new List<PokemonLocation>()
                };
            }
            return null;
        }

        public static async Task Start(Session session, CancellationToken cancellationToken)
        {
            while (true)
            {
                if (session.LogicSettings.UseSnipeOnlineLocationServer)
                {
                    var st = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var t = DateTime.Now.ToUniversalTime() - st;
                    var currentTimestamp = t.TotalMilliseconds;
                    var pokemonIds = session.LogicSettings.PokemonToSnipe.Pokemon;

                    var formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };

                    var offset = session.LogicSettings.SnipingScanOffset;
                    // 0.003 = half a mile; maximum 0.06 is 10 miles
                    if (offset < 0.001) offset = 0.003;
                    if (offset > 0.06) offset = 0.06;

                    var uri =
                        $"http://pokesnipers.com/api/v1/pokemon.json";

                    ScanResult scanResult;
                    try
                    {
                        var request = WebRequest.CreateHttp(uri);
                        request.Accept = "application/json";
                        request.Method = "GET";
                        request.Timeout = 5000;
                        request.ReadWriteTimeout = 32000;

                        var resp = request.GetResponse();
                        var reader = new StreamReader(resp.GetResponseStream());
                        var fullresp = reader.ReadToEnd().Replace(" M", "Male").Replace(" F", "Female");

                        dynamic pokesniper = JsonConvert.DeserializeObject(fullresp);
                        JArray results = pokesniper.results;
                        SnipeLocations.Clear();

                        foreach (var result in results)
                        {
                            PokemonId id;
                            Enum.TryParse(result.Value<string>("name"), out id);
                            var a = new SniperInfo
                            {
                                Id = id,
                                Iv = 100,
                                latitude = Convert.ToDouble(result.Value<string>("coords").Split(',')[0]),
                                longitude = Convert.ToDouble(result.Value<string>("coords").Split(',')[1]),
                                TimeStamp = DateTime.Now
                            };
                            SnipeLocations.Add(a);
                        }
                    }
                    catch (Exception ex)
                    {
                        // most likely System.IO.IOException
                        session.EventDispatcher.Send(new ErrorEvent { Message = ex.ToString() });
                        scanResult = new ScanResult
                        {
                            Status = "fail",
                            pokemons = new List<PokemonLocation>()
                        };
                    }
                }
                else
                {

                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        var lClient = new TcpClient();
                        lClient.Connect(session.LogicSettings.SnipeLocationServer,
                            session.LogicSettings.SnipeLocationServerPort);

                        var sr = new StreamReader(lClient.GetStream());

                        while (lClient.Connected)
                        {
                            var line = sr.ReadLine();
                            if (line == null)
                                throw new Exception("Unable to ReadLine from sniper socket");

                            var info = JsonConvert.DeserializeObject<SniperInfo>(line);

                            if (SnipeLocations.Any(x =>
                                Math.Abs(x.latitude - info.latitude) < 0.0001 &&
                                Math.Abs(x.longitude - info.longitude) < 0.0001))
                                // we might have different precisions from other sources
                                continue;

                            SnipeLocations.RemoveAll(x => DateTime.Now > x.TimeStampAdded.AddMinutes(15));
                            SnipeLocations.Add(info);
                        }
                    }
                    catch (SocketException)
                    {
                        // this is spammed to often. Maybe add it to debug log later
                    }
                    catch (Exception ex)
                    {
                        // most likely System.IO.IOException
                        session.EventDispatcher.Send(new ErrorEvent { Message = ex.ToString() });
                    }
                }
                await Task.Delay(5000, cancellationToken);
            }
        }
    }
}