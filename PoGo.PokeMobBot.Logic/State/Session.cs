#region using directives

using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PokemonGo.RocketAPI;
using POGOProtos.Networking.Responses;

#endregion

namespace PoGo.PokeMobBot.Logic.State
{
    public interface ISession
    {
        ISettings Settings { get; }
        Inventory Inventory { get; }
        Client Client { get; }
        GetPlayerResponse Profile { get; set; }
        HumanNavigation Navigation { get; }
        MapCache MapCache { get; }
        ILogicSettings LogicSettings { get; }
        ITranslation Translation { get; }
        IEventDispatcher EventDispatcher { get; }
    }


    public class Session : ISession
    {
        public Session(ISettings settings, ILogicSettings logicSettings)
        {
            Settings = settings;
            LogicSettings = logicSettings;
            ApiFailureStrategy = new ApiFailureStrategy(this);
            EventDispatcher = new EventDispatcher();
            Translation = Common.Translation.Load(logicSettings);
            Reset(settings, LogicSettings);
        }

        public ISettings Settings { get; }

        public Inventory Inventory { get; private set; }

        public Client Client { get; private set; }

        public GetPlayerResponse Profile { get; set; }
        public HumanNavigation Navigation { get; private set; }

        public MapCache MapCache { get; private set; }
        public ILogicSettings LogicSettings { get; }

        public ITranslation Translation { get; }

        public IEventDispatcher EventDispatcher { get; }

        public ApiFailureStrategy ApiFailureStrategy { get; set; }

        public void Reset(ISettings settings, ILogicSettings logicSettings)
        {
            Client = new Client(Settings, ApiFailureStrategy);
            // ferox wants us to set this manually
            Inventory = new Inventory(Client, logicSettings);
            Navigation = new HumanNavigation(Client);
            MapCache = new MapCache();
        }
    }
}