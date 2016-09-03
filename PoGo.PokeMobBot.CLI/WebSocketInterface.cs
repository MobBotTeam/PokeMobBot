#region using directives

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Logging;
using PoGo.PokeMobBot.Logic.State;
using PoGo.PokeMobBot.Logic.Tasks;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.WebSocket;
using System;
using System.Collections.Generic;

#endregion

namespace PoGo.PokeMobBot.CLI
{
    public class WebSocketInterface
    {
        private readonly WebSocketServer _server;
        private readonly Session _session;
        private PokeStopListEvent _lastPokeStopList;
        private ProfileEvent _lastProfile;

        public WebSocketInterface(int port, Session session)
        {
            _session = session;
            var translations = session.Translation;
            _server = new WebSocketServer();
            var config = new ServerConfig
            {
                Name = "MobBotWebSocket",
                Mode = SocketMode.Tcp,
                Certificate = new CertificateConfig
                {
                    FilePath = @"cert.pfx",
                    Password = "pokemobbot"
                },
            };
            config.Listeners = new List<ListenerConfig>
            {
                new ListenerConfig()
                {
                    Ip = "Any", Port = port, Security = "tls"
                },
                new ListenerConfig()
                {
                    Ip = "Any", Port = port + 1, Security = "none"
                }
            };

            var setupComplete = _server.Setup(config);

            if (setupComplete == false)
            {
                session.EventDispatcher.Send(new ErrorEvent() { Message = translations.GetTranslation(TranslationString.WebSocketFailStart, port) });
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
            Console.WriteLine(message);
            try
            {
                msgObj = JsonConvert.DeserializeObject<Models.SocketMessage>(message);
                command = msgObj.Command;
            }
            catch (Exception ex) {Logger.Write(ex.Message, LogLevel.Error); }

            // Action request from UI should not be broadcasted to all client
            Action<IEvent> action = (evt) => session.Send(Serialize(evt));
            
            switch (command)
            {
                case "PokemonList":
                    await PokemonListTask.Execute(_session, action);
                    break;
                case "EggsList":
                    await EggsListTask.Execute(_session, action);
                    break;
                case "InventoryList":
                    await InventoryListTask.Execute(_session, action);
                    break;
                case "PlayerStats":
                    await PlayerStatsTask.Execute(_session, action);
                    break;
                case "GetPokemonSettings":
                    await PokemonSettingsTask.Execute(_session, action);
                    break;
                case "TransferPokemon":
                    await TransferPokemonTask.Execute(_session, msgObj?.Data);
                    break;
                case "EvolvePokemon":
                    await EvolveSpecificPokemonTask.Execute(_session, msgObj?.Data);
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
                    Latitude = _session.Client.CurrentLatitude,
                    Longitude = _session.Client.CurrentLongitude
                }));
            }
            catch (Exception ex) {Logger.Write(ex.Message, LogLevel.Error); }
        }

        public void Listen(IEvent evt, Session session)
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
            return typeof(Int64).Equals(objectType) || typeof(ulong).Equals(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToString());
        }
    }
}