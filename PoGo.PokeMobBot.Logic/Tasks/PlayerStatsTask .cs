#region using directives

using System.Linq;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.State;
using POGOProtos.Inventory.Item;
using System;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class PlayerStatsTask
    {
        private readonly Inventory _inventory;
        private readonly ILogicSettings _logicSettings;

        public PlayerStatsTask(Inventory inventory, ILogicSettings logicSettings)
        {
            _inventory = inventory;
            _logicSettings = logicSettings;
        }

        public async Task Execute(Action<IEvent> action)
        {
            var PlayersProfile = (await _inventory.GetPlayerStats())
                .ToList();

            action(
                new PlayerStatsEvent
                {
                    PlayerStats = PlayersProfile,
                });

            await Task.Delay(_logicSettings.DelayBetweenPlayerActions);
        }
    }
}