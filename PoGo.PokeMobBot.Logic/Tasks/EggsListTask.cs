#region using directives

using System.Linq;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.State;
using POGOProtos.Inventory.Item;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class EggsListTask
    {
        private readonly Inventory _inventory;
        private readonly IEventDispatcher _eventDispatcher;

        public EggsListTask(Inventory inventory, IEventDispatcher eventDispatcher)
        {
            _inventory = inventory;
            _eventDispatcher = eventDispatcher;
        }

        public async Task Execute()
        {
            // Refresh inventory so that the player stats are fresh
            await _inventory.RefreshCachedInventory();

            var playerStats = (await _inventory.GetPlayerStats()).FirstOrDefault();
            if (playerStats == null)
                return;

            var kmWalked = playerStats.KmWalked;

            var incubators = (await _inventory.GetEggIncubators())
                .Where(x => x.UsesRemaining > 0 || x.ItemId == ItemId.ItemIncubatorBasicUnlimited)
                .OrderByDescending(x => x.ItemId == ItemId.ItemIncubatorBasicUnlimited)
                .ToList();

            var unusedEggs = (await _inventory.GetEggs())
                .Where(x => string.IsNullOrEmpty(x.EggIncubatorId))
                .OrderBy(x => x.EggKmWalkedTarget - x.EggKmWalkedStart)
                .ToList();

            _eventDispatcher.Send(
                new EggsListEvent
                {
                    PlayerKmWalked = kmWalked,
                    Incubators = incubators,
                    UnusedEggs = unusedEggs
                });
        }
    }
}