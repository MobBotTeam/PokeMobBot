#region using directives

using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using PoGo.PokeMobBot.Logic;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Logging;
using PoGo.PokeMobBot.Logic.State;
using PoGo.PokeMobBot.Logic.Tasks;
using PokemonGo.RocketAPI;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.WebSocket;

#endregion

namespace PoGo.PokeMobBot.CLI
{
    public class WebSocketInterface
    {
        private readonly WebSocketServer _server;
        private readonly Session _session;
        private readonly PokemonListTask _pokemonListTask;
        private readonly EggsListTask _eggsListTask;
        private readonly InventoryListTask _inventoryListTask;
        private PokeStopListEvent _lastPokeStopList;
        private ProfileEvent _lastProfile;
        private readonly ILogger _logger;

        public WebSocketInterface(GlobalSettings settings, Session session, PokemonListTask pokemonListTask, EggsListTask eggsListTask, InventoryListTask inventoryListTask, ILogger logger)
        {
            _session = session;
            _pokemonListTask = pokemonListTask;
            _eggsListTask = eggsListTask;
            _inventoryListTask = inventoryListTask;
            _logger = logger;

            var translations = session.Translation;
            _server = new WebSocketServer();
            var setupComplete = _server.Setup(new ServerConfig
            {
                Name = "NecroWebSocket",
                Ip = "Any",
                Port = settings.WebSocketPort,
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
                _logger.Write(translations.GetTranslation(TranslationString.WebSocketFailStart, settings.WebSocketPort), LogLevel.Error);
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
            switch (message)
            {
                case "PokemonList":
                    await _pokemonListTask.Execute(_session);
                    break;
                case "EggsList":
                    await _eggsListTask.Execute(_session);
                    break;
                case "InventoryList":
                    await _inventoryListTask.Execute(_session);
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
            catch { }
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
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            return JsonConvert.SerializeObject(evt, Formatting.None, jsonSerializerSettings);
        }
    }
}