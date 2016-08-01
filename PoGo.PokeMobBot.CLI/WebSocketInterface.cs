﻿#region using directives

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.State;
using PoGo.PokeMobBot.Logic.Tasks;
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
        private readonly Session _session;
        private PokeStopListEvent _lastPokeStopList;
        private ProfileEvent _lastProfile;

        public WebSocketInterface(int port, Session session)
        {
            _session = session;
            var translations = session.Translation;
            _server = new WebSocketServer();
            var setupComplete = _server.Setup(new ServerConfig
            {
                Name = "MobBotWebSocket",
                Ip = "Any",
                Port = port,
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
                session.EventDispatcher.Send(new ErrorEvent { Message = translations.GetTranslation(TranslationString.WebSocketFailStart, port) });
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
            var command = message;
            try
            {
                var msgObj = JsonConvert.DeserializeObject<Models.SocketMessage>(message);
                command = msgObj.Command;
            }
            catch
            {
                // ignored
            }

            switch (command)
            {
                case "PokemonList":
                    await PokemonListTask.Execute(_session);
                    break;
                case "EggsList":
                    await EggsListTask.Execute(_session);
                    break;
                case "InventoryList":
                    await InventoryListTask.Execute(_session);
                    break;
                case "PlayerStats":
                    await PlayerStatsTask.Execute(_session);
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
                session.Send(Serialize(new UpdatePositionEvent
                {
                    Latitude = _session.Client.CurrentLatitude,
                    Longitude = _session.Client.CurrentLongitude
                }));
            }
            catch
            {
                // ignored
            }
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
            JToken jt = JToken.ReadFrom(reader);
            return jt.Value<long>();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Int64) == objectType || typeof(ulong) == objectType;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToString());
        }
    }
}