#region using directives

using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PokemonGo.RocketAPI;
using POGOProtos.Networking.Responses;

#endregion

namespace PoGo.PokeMobBot.Logic.State
{
    //public interface ISession
    //{
    //    ISettings Settings { get; }
    //    Inventory Inventory { get; }
    //    Client Client { get; }
    //    GetPlayerResponse Profile { get; set; }
    //    Navigation Navigation { get; }
    //    ILogicSettings LogicSettings { get; }
    //    ITranslation Translation { get; }
    //    IEventDispatcher EventDispatcher { get; }
    //}


    //public class Session : ISession
    //{
    //    public Session(ISettings settings, ILogicSettings logicSettings, Translation translation, Inventory inventory, Navigation navigation, Client client)
    //    {
    //        Settings = settings;
    //        LogicSettings = logicSettings;
    //        EventDispatcher = new EventDispatcher();
    //        Translation = translation;
    //        Inventory = inventory;
    //        Navigation = navigation;
    //        Client = client;
    //    }

    //    public ISettings Settings { get; }

    //    public Inventory Inventory { get; private set; }

    //    public Client Client { get; private set; }

    //    public GetPlayerResponse Profile { get; set; }
    //    public Navigation Navigation { get; private set; }

    //    public ILogicSettings LogicSettings { get; }

    //    public ITranslation Translation { get; }

    //    public IEventDispatcher EventDispatcher { get; }
    //}
}