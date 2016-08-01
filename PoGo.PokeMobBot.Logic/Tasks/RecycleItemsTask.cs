#region using directives

using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.State;
using PoGo.PokeMobBot.Logic.Utils;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class RecycleItemsTask
    {
        private readonly DelayingUtils _delayingUtils;

        public RecycleItemsTask(DelayingUtils delayingUtils)
        {
            _delayingUtils = delayingUtils;
        }

        public async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var items = await session.Inventory.GetItemsToRecycle(session);

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await session.Client.Inventory.RecycleItem(item.ItemId, item.Count);

                session.EventDispatcher.Send(new ItemRecycledEvent {Id = item.ItemId, Count = item.Count});
                if (session.LogicSettings.Teleport)
                    await Task.Delay(session.LogicSettings.DelayRecyleItem, cancellationToken);
                else
                    await _delayingUtils.Delay(session.LogicSettings.DelayBetweenPlayerActions, 500);
            }
                await session.Inventory.RefreshCachedInventory();
        }
    }
}