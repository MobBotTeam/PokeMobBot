#region using directives

using System.Linq;
using System.Threading.Tasks;

using POGOProtos.Inventory.Item;
using PoGo.PokeMobBot.Logic.State;
using PoGo.PokeMobBot.Logic.Event;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class InventoryListTask
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly Inventory _inventory;

        public InventoryListTask(Inventory inventory, IEventDispatcher eventDispatcher)
        {
            _inventory = inventory;
            _eventDispatcher = eventDispatcher;
        }

        public async Task Execute()
        {
            var inventory = await _inventory.GetItems();

            _eventDispatcher.Send(
                new InventoryListEvent
                {
                    Items = inventory.ToList()
                });
        }
    }
}