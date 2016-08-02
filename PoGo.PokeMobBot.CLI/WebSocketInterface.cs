#region using directives

using Newtonsoft.Json;
using PoGo.PokeMobBot.Logic;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Logging;
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
        private readonly PokemonListTask _pokemonListTask;
        private readonly EggsListTask _eggsListTask;
        private readonly InventoryListTask _inventoryListTask;
        private readonly PlayerStatsTask _playerStatsTask;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ITranslation _translation;
        private readonly Client _client;
        private PokeStopListEvent _lastPokeStopList;
        private ProfileEvent _lastProfile;

        public WebSocketInterface(GlobalSettings settings, PokemonListTask pokemonListTask, EggsListTask eggsListTask, InventoryListTask inventoryListTask, ILogger logger, PlayerStatsTask playerStatsTask, IEventDispatcher eventDispatcher, ITranslation translation, Client client)
        {
            _pokemonListTask = pokemonListTask;
            _eggsListTask = eggsListTask;
            _inventoryListTask = inventoryListTask;
            _playerStatsTask = playerStatsTask;
            _eventDispatcher = eventDispatcher;
            _translation = translation;
            _client = client;

            _server = new WebSocketServer();
            var setupComplete = _server.Setup(new ServerConfig
            {
                Name = "MobBotWebSocket",
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
                _eventDispatcher.Send(new ErrorEvent() { Message = _translation.GetTranslation(TranslationString.WebSocketFailStart, settings.WebSocketPort) });
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
                    await _pokemonListTask.Execute();
                    break;
                case "EggsList":
                    await _eggsListTask.Execute();
                    break;
                case "InventoryList":
                    await _inventoryListTask.Execute();
                    break;
                case "PlayerStats":
                    await _playerStatsTask.Execute();
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
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            return JsonConvert.SerializeObject(evt, Formatting.None, jsonSerializerSettings);
        }
    }
}
