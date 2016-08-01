#region using directives

using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.State;
using PoGo.PokeMobBot.Logic.Utils;
using POGOProtos.Inventory.Item;

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

            var currentTotalItems = await session.Inventory.GetTotalItemCount();
            if (session.Profile.PlayerData.MaxItemStorage * session.LogicSettings.RecycleInventoryAtUsagePercentage > currentTotalItems)
                return;

            var items = await session.Inventory.GetItemsToRecycle(session);

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await session.Client.Inventory.RecycleItem(item.ItemId, item.Count);

                session.EventDispatcher.Send(new ItemRecycledEvent { Id = item.ItemId, Count = item.Count });
                if (session.LogicSettings.Teleport)
                    await Task.Delay(session.LogicSettings.DelayRecyleItem, cancellationToken);
                else
                    await _delayingUtils.Delay(session.LogicSettings.DelayBetweenPlayerActions, 500);
            }

            if (session.LogicSettings.TotalAmountOfPokeballsToKeep != 0)
            {
                await OptimizedRecycleBalls(session, cancellationToken);
            }

            if (session.LogicSettings.TotalAmountOfPotionsToKeep != 0)
            {
                await OptimizedRecyclePotions(session, cancellationToken);
            }

            if (session.LogicSettings.TotalAmountOfRevivesToKeep != 0)
            {
                await OptimizedRecycleRevives(session, cancellationToken);
            }

            await session.Inventory.RefreshCachedInventory();
        }

        private async Task OptimizedRecycleBalls(ISession session, CancellationToken cancellationToken)
        {
            var pokeBallsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemPokeBall);
            var greatBallsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemGreatBall);
            var ultraBallsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemUltraBall);
            var masterBallsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemMasterBall);

            int pokeBallsToRecycle = 0;
            int greatBallsToRecycle = 0;
            //int ultraBallsToRecycle = 0;
            //int masterBallsToRecycle = 0;
            //unused at the moment
            //TODO: implement these with reasonable settings


            int totalBallsCount = pokeBallsCount + greatBallsCount + ultraBallsCount + masterBallsCount;
            if (totalBallsCount > session.LogicSettings.TotalAmountOfPokeballsToKeep)
            {
                int diff = totalBallsCount - session.LogicSettings.TotalAmountOfPokeballsToKeep;
                if (diff > 0)
                {
                    int pokeBallsToKeep = pokeBallsCount - diff;
                    if (pokeBallsToKeep < 0)
                    {
                        pokeBallsToKeep = 0;
                    }
                    pokeBallsToRecycle = pokeBallsCount - pokeBallsToKeep;

                    if (pokeBallsToRecycle != 0)
                    {
                        diff -= pokeBallsToRecycle;
                        cancellationToken.ThrowIfCancellationRequested();
                        await session.Client.Inventory.RecycleItem(ItemId.ItemPokeBall, pokeBallsToRecycle);
                        session.EventDispatcher.Send(new ItemRecycledEvent { Id = ItemId.ItemPokeBall, Count = pokeBallsToRecycle });
                        if (session.LogicSettings.Teleport)
                            await Task.Delay(session.LogicSettings.DelayRecyleItem);
                        else
                            await _delayingUtils.Delay(session.LogicSettings.DelayBetweenPlayerActions, 500);
                    }
                }

                if (diff > 0)
                {
                    int greatBallsToKeep = greatBallsCount - diff;
                    if (greatBallsToKeep < 0)
                    {
                        greatBallsToKeep = 0;
                    }
                    greatBallsToRecycle = greatBallsCount - greatBallsToKeep;

                    if (greatBallsToRecycle != 0)
                    {
                        diff -= greatBallsToRecycle;
                        cancellationToken.ThrowIfCancellationRequested();
                        await session.Client.Inventory.RecycleItem(ItemId.ItemGreatBall, greatBallsToRecycle);
                        session.EventDispatcher.Send(new ItemRecycledEvent { Id = ItemId.ItemGreatBall, Count = greatBallsToRecycle });
                        if (session.LogicSettings.Teleport)
                            await Task.Delay(session.LogicSettings.DelayRecyleItem);
                        else
                            await _delayingUtils.Delay(session.LogicSettings.DelayBetweenPlayerActions, 500);
                    }
                }


            }
        }

        private async Task OptimizedRecyclePotions(ISession session, CancellationToken cancellationToken)
        {
            var potionCount = await session.Inventory.GetItemAmountByType(ItemId.ItemPotion);
            var superPotionCount = await session.Inventory.GetItemAmountByType(ItemId.ItemSuperPotion);
            var hyperPotionsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemHyperPotion);
            var maxPotionCount = await session.Inventory.GetItemAmountByType(ItemId.ItemMaxPotion);

            int potionsToRecycle = 0;
            int superPotionsToRecycle = 0;
            int hyperPotionsToRecycle = 0;
            int maxPotionsToRecycle = 0;

            int totalPotionsCount = potionCount + superPotionCount + hyperPotionsCount + maxPotionCount;
            if (totalPotionsCount > session.LogicSettings.TotalAmountOfPotionsToKeep)
            {
                int diff = totalPotionsCount - session.LogicSettings.TotalAmountOfPotionsToKeep;
                if (diff > 0)
                {
                    int potionsToKeep = potionCount - diff;
                    if (potionsToKeep < 0)
                    {
                        potionsToKeep = 0;
                    }
                    potionsToRecycle = potionCount - potionsToKeep;

                    if (potionsToRecycle != 0)
                    {
                        diff -= potionsToRecycle;
                        cancellationToken.ThrowIfCancellationRequested();
                        await session.Client.Inventory.RecycleItem(ItemId.ItemPotion, potionsToRecycle);
                        session.EventDispatcher.Send(new ItemRecycledEvent { Id = ItemId.ItemPotion, Count = potionsToRecycle });
                        if (session.LogicSettings.Teleport)
                            await Task.Delay(session.LogicSettings.DelayRecyleItem);
                        else
                            await _delayingUtils.Delay(session.LogicSettings.DelayBetweenPlayerActions, 500);
                    }
                }

                if (diff > 0)
                {
                    int superPotionsToKeep = superPotionCount - diff;
                    if (superPotionsToKeep < 0)
                    {
                        superPotionsToKeep = 0;
                    }
                    superPotionsToRecycle = superPotionCount - superPotionsToKeep;

                    if (superPotionsToRecycle != 0)
                    {
                        diff -= superPotionsToRecycle;
                        cancellationToken.ThrowIfCancellationRequested();
                        await session.Client.Inventory.RecycleItem(ItemId.ItemSuperPotion, superPotionsToRecycle);
                        session.EventDispatcher.Send(new ItemRecycledEvent { Id = ItemId.ItemSuperPotion, Count = superPotionsToRecycle });
                        if (session.LogicSettings.Teleport)
                            await Task.Delay(session.LogicSettings.DelayRecyleItem);
                        else
                            await _delayingUtils.Delay(session.LogicSettings.DelayBetweenPlayerActions, 500);
                    }
                }

                if (diff > 0)
                {
                    int hyperPotionsToKeep = hyperPotionsCount - diff;
                    if (hyperPotionsToKeep < 0)
                    {
                        hyperPotionsToKeep = 0;
                    }
                    hyperPotionsToRecycle = hyperPotionsCount - hyperPotionsToKeep;

                    if (hyperPotionsToRecycle != 0)
                    {
                        diff -= hyperPotionsToRecycle;
                        cancellationToken.ThrowIfCancellationRequested();
                        await session.Client.Inventory.RecycleItem(ItemId.ItemHyperPotion, hyperPotionsToRecycle);
                        session.EventDispatcher.Send(new ItemRecycledEvent { Id = ItemId.ItemHyperPotion, Count = hyperPotionsToRecycle });
                        if (session.LogicSettings.Teleport)
                            await Task.Delay(session.LogicSettings.DelayRecyleItem);
                        else
                            await _delayingUtils.Delay(session.LogicSettings.DelayBetweenPlayerActions, 500);
                    }
                }

                if (diff > 0)
                {
                    int maxPotionsToKeep = maxPotionCount - diff;
                    if (maxPotionsToKeep < 0)
                    {
                        maxPotionsToKeep = 0;
                    }
                    maxPotionsToRecycle = maxPotionCount - maxPotionsToKeep;

                    if (maxPotionsToRecycle != 0)
                    {
                        diff -= maxPotionsToRecycle;
                        cancellationToken.ThrowIfCancellationRequested();
                        await session.Client.Inventory.RecycleItem(ItemId.ItemMaxPotion, maxPotionsToRecycle);
                        session.EventDispatcher.Send(new ItemRecycledEvent { Id = ItemId.ItemMaxPotion, Count = maxPotionsToRecycle });
                        if (session.LogicSettings.Teleport)
                            await Task.Delay(session.LogicSettings.DelayRecyleItem);
                        else
                            await _delayingUtils.Delay(session.LogicSettings.DelayBetweenPlayerActions, 500);
                    }
                }
            }
        }

        private async Task OptimizedRecycleRevives(ISession session, CancellationToken cancellationToken)
        {
            var reviveCount = await session.Inventory.GetItemAmountByType(ItemId.ItemRevive);
            var maxReviveCount = await session.Inventory.GetItemAmountByType(ItemId.ItemMaxRevive);

            int revivesToRecycle = 0;
            int maxRevivesToRecycle = 0;

            int totalRevivesCount = reviveCount + maxReviveCount;
            if (totalRevivesCount > session.LogicSettings.TotalAmountOfRevivesToKeep)
            {
                int diff = totalRevivesCount - session.LogicSettings.TotalAmountOfRevivesToKeep;
                if (diff > 0)
                {
                    int revivesToKeep = reviveCount - diff;
                    if (revivesToKeep < 0)
                    {
                        revivesToKeep = 0;
                    }
                    revivesToRecycle = reviveCount - revivesToKeep;

                    if (revivesToRecycle != 0)
                    {
                        diff -= revivesToRecycle;
                        cancellationToken.ThrowIfCancellationRequested();
                        await session.Client.Inventory.RecycleItem(ItemId.ItemRevive, revivesToRecycle);
                        session.EventDispatcher.Send(new ItemRecycledEvent { Id = ItemId.ItemRevive, Count = revivesToRecycle });
                        if (session.LogicSettings.Teleport)
                            await Task.Delay(session.LogicSettings.DelayRecyleItem);
                        else
                            await _delayingUtils.Delay(session.LogicSettings.DelayBetweenPlayerActions, 500);
                    }
                }

                if (diff > 0)
                {
                    int maxRevivesToKeep = maxReviveCount - diff;
                    if (maxRevivesToKeep < 0)
                    {
                        maxRevivesToKeep = 0;
                    }
                    maxRevivesToRecycle = maxReviveCount - maxRevivesToKeep;

                    if (maxRevivesToRecycle != 0)
                    {
                        diff -= maxRevivesToRecycle;
                        cancellationToken.ThrowIfCancellationRequested();
                        await session.Client.Inventory.RecycleItem(ItemId.ItemMaxRevive, maxRevivesToRecycle);
                        session.EventDispatcher.Send(new ItemRecycledEvent { Id = ItemId.ItemMaxRevive, Count = maxRevivesToRecycle });
                        if (session.LogicSettings.Teleport)
                            await Task.Delay(session.LogicSettings.DelayRecyleItem);
                        else
                            await _delayingUtils.Delay(session.LogicSettings.DelayBetweenPlayerActions, 500);
                    }
                }
            }
        }
    }
}