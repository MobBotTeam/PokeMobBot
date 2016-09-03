#region using directives

using Newtonsoft.Json;
using PoGo.PokeMobBot.Logic;
using Newtonsoft.Json.Linq;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Logging;
using PoGo.PokeMobBot.Logic.Tasks;
using PokemonGo.RocketAPI;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.WebSocket;
using System;

#endregion

namespace PoGo.PokeMobBot.CLI
{
    public class WebSocketInterface
    {
        private readonly WebSocketServer _server;
        private readonly PokemonListTask _pokemonListTask;
        private readonly EggsListTask _eggsListTask;
        private readonly InventoryListTask _inventoryListTask;
        private readonly PlayerStatsTask _playerStatsTask;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ITranslation _translation;
        private readonly Client _client;
        private readonly PokemonSettingsTask _pokemonSettingsTask;
        private readonly TransferPokemonTask _transferPokemonTask;
        private readonly EvolveSpecificPokemonTask _evolveSpecificPokemonTask;
        private PokeStopListEvent _lastPokeStopList;
        private ProfileEvent _lastProfile;

        public WebSocketInterface(GlobalSettings settings, PokemonListTask pokemonListTask, EggsListTask eggsListTask, InventoryListTask inventoryListTask, ILogger logger, PlayerStatsTask playerStatsTask, IEventDispatcher eventDispatcher, ITranslation translation, Client client, PokemonSettingsTask pokemonSettingsTask, TransferPokemonTask transferPokemonTask, EvolveSpecificPokemonTask evolveSpecificPokemonTask)
        {
            _pokemonListTask = pokemonListTask;
            _eggsListTask = eggsListTask;
            _inventoryListTask = inventoryListTask;
            _playerStatsTask = playerStatsTask;
            _eventDispatcher = eventDispatcher;
            _translation = translation;
            _client = client;
            _pokemonSettingsTask = pokemonSettingsTask;
            _transferPokemonTask = transferPokemonTask;
            _evolveSpecificPokemonTask = evolveSpecificPokemonTask;

            _server = new WebSocketServer();
            var setupComplete = _server.Setup(new ServerConfig
            {
                Name = "MobBotWebSocket",
                Ip = "Any",
                Port = settings.StartUpSettings.WebSocketPort,
                Mode = SocketMode.Tcp,
                Security = "tls",
                Certificate = new CertificateConfig
                {
                    FilePath = @"cert.pfx",
                    Password = "necro"
                }
            });

            if (setupComplete == false)
            {
                _eventDispatcher.Send(new ErrorEvent() { Message = _translation.GetTranslation(TranslationString.WebSocketFailStart, settings.StartUpSettings.WebSocketPort) });
                return;
            }

            _server.NewMessageReceived += HandleMessage;
            _server.NewSessionConnected += HandleSession;

            _server.Start();
        }

        private void Broadcast(string message)
        {
            foreach (var session in _server.GetAllSessions())
            {
                try
                {
                    session.Send(message);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void HandleEvent(PokeStopListEvent evt)
        {
            _lastPokeStopList = evt;
        }

        private void HandleEvent(ProfileEvent evt)
        {
            _lastProfile = evt;
        }

        private async void HandleMessage(WebSocketSession session, string message)
        {
            Models.SocketMessage msgObj = null;
            var command = message;
            try
            {
                msgObj = JsonConvert.DeserializeObject<Models.SocketMessage>(message);
                command = msgObj.Command;
            }
            catch { }

            // Action request from UI should not be broadcasted to all client
            Action<IEvent> action = (evt) => session.Send(Serialize(evt));

            switch (command)
            {
                case "PokemonList":
                    await _pokemonListTask.Execute(action);
                    break;
                case "EggsList":
                    await _eggsListTask.Execute(action);
                    break;
                case "InventoryList":
                    await _inventoryListTask.Execute(action);
                    break;
                case "PlayerStats":
                    await _playerStatsTask.Execute(action);
                    break;
                case "GetPokemonSettings":
                    await _pokemonSettingsTask.Execute(action);
                    break;
                case "TransferPokemon":
                    await _transferPokemonTask.Execute(msgObj?.Data);
                    break;
                case "EvolvePokemon":
                    await _evolveSpecificPokemonTask.Execute(msgObj?.Data);
                    break;
            }
        }

        private void HandleSession(WebSocketSession session)
        {
            if (_lastProfile != null)
                session.Send(Serialize(_lastProfile));

            if (_lastPokeStopList != null)
                session.Send(Serialize(_lastPokeStopList));

            try
            {
                session.Send(Serialize(new UpdatePositionEvent()
                {
                    Latitude = _client.CurrentLatitude,
                    Longitude = _client.CurrentLongitude
                }));
            }
            catch { }
        }

        public void Listen(IEvent evt)
        {
            dynamic eve = evt;

            try
            {
                HandleEvent(eve);
            }
            catch
            {
                // ignored
            }

            Broadcast(Serialize(eve));
        }

        private string Serialize(dynamic evt)
        {
            var jsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

            // Add custom seriaizer to convert uong to string (ulong shoud not appear to json according to json specs)
            jsonSerializerSettings.Converters.Add(new IdToStringConverter());

            return JsonConvert.SerializeObject(evt, Formatting.None, jsonSerializerSettings);
        }
    }

    public class IdToStringConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken jt = JValue.ReadFrom(reader);
            return jt.Value<long>();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(System.Int64).Equals(objectType) || typeof(ulong).Equals(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToString());
        }
    }
}