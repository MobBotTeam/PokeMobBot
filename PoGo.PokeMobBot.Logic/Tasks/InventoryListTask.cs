#region using directives

using System.Linq;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;
using System;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class InventoryListTask
    {
        private readonly Inventory _inventory;
        private readonly ILogicSettings _logicSettings;

        public InventoryListTask(Inventory inventory, ILogicSettings logicSettings)
        {
            _inventory = inventory;
            _logicSettings = logicSettings;
        }

        public async Task Execute(Action<IEvent> action)
        {
            // Refresh inventory so that the player stats are fresh
            await _inventory.RefreshCachedInventory();

            var inventory = await _inventory.GetItems();

            action(
                new InventoryListEvent
                {
                    Items = inventory.ToList()
                });

            await Task.Delay(_logicSettings.DelayBetweenPlayerActions);
        }
    }
}