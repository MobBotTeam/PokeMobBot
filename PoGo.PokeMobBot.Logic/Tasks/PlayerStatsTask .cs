#region using directives

using System.Linq;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.State;
using POGOProtos.Inventory.Item;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class PlayerStatsTask
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly Inventory _inventory;

        public PlayerStatsTask(IEventDispatcher eventDispatcher, Inventory inventory)
        {
            _eventDispatcher = eventDispatcher;
            _inventory = inventory;
        }

        public async Task Execute()
        {
            var PlayersProfile = (await _inventory.GetPlayerStats())
                .ToList();


            _eventDispatcher.Send(
                new PlayerStatsEvent
                {
                    PlayerStats = PlayersProfile,
                });
        }
    }
}