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
        Navigation Navigation { get; }
        ILogicSettings LogicSettings { get; }
        TeleDelay TeleSetting { get; set; }
        ITranslation Translation { get; }
        IEventDispatcher EventDispatcher { get; }
    }


    public class Session : ISession
    {
        public Session(ISettings settings, ILogicSettings logicSettings)
        {
            Settings = settings;
            LogicSettings = logicSettings;
            EventDispatcher = new EventDispatcher();
            Translation = Common.Translation.Load(logicSettings);
            Reset(settings, LogicSettings);
        }

        public ISettings Settings { get; }

        public Inventory Inventory { get; private set; }

        public Client Client { get; private set; }

        public GetPlayerResponse Profile { get; set; }
        public Navigation Navigation { get; private set; }

        public ILogicSettings LogicSettings { get; }

        public TeleDelay TeleSetting { get; set; }

        public ITranslation Translation { get; }

        public IEventDispatcher EventDispatcher { get; }

        public void Reset(ISettings settings, ILogicSettings logicSettings)
        {
            Client = new Client(Settings) {AuthType = settings.AuthType};
            // ferox wants us to set this manually
            Inventory = new Inventory(Client, logicSettings);
            Navigation = new Navigation(Client);
        }
    }
}