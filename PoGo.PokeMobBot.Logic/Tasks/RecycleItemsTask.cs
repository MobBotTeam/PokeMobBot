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
        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await session.Inventory.RefreshCachedInventory();
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
                    await Task.Delay(session.LogicSettings.DelayRecyleItem);
                else
                    await DelayingUtils.Delay(session.LogicSettings.DelayBetweenPlayerActions, 500);
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

        private static async Task OptimizedRecycleBalls(ISession session, CancellationToken cancellationToken)
        {
            var pokeBallsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemPokeBall);
            var greatBallsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemGreatBall);
            var ultraBallsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemUltraBall);
            var masterBallsCount = await session.Inventory.GetItemAmountByType(ItemId.ItemMasterBall);

            int pokeBallsToRecycle = 0;
            int greatBallsToRecycle = 0;
            int ultraBallsToRecycle = 0;
            int masterBallsToRecycle = 0;

            int totalBallsCount = pokeBallsCount + greatBallsCount + ultraBallsCount + masterBallsCount;
            if (totalBallsCount > session.LogicSettings.TotalAmountOfPokeballsToKeep)
            {
                int diff = totalBallsCount - session.LogicSettings.TotalAmountOfPokeballsToKeep;
                if (diff > 0)
                {
                    await removeItems(pokeBallsCount, pokeBallsToRecycle, diff, ItemId.ItemPokeBall, cancellationToken, session);
                }
                if (diff > 0)
                {
                    await removeItems(greatBallsCount, greatBallsToRecycle, diff, ItemId.ItemGreatBall, cancellationToken, session);
                }
                if (diff > 0)
                {
                    await removeItems(ultraBallsCount, ultraBallsToRecycle, diff, ItemId.ItemUltraBall, cancellationToken, session);
                }
                if (diff > 0)
                {
                    await removeItems(masterBallsCount, masterBallsToRecycle, diff, ItemId.ItemMasterBall, cancellationToken, session);
                }
            }
        }

        private static async Task OptimizedRecyclePotions(ISession session, CancellationToken cancellationToken)
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
                    await removeItems(potionCount, potionsToRecycle, diff, ItemId.ItemPotion, cancellationToken, session);
                }
                if (diff > 0)
                {
                    await removeItems(superPotionCount, superPotionsToRecycle, diff, ItemId.ItemSuperPotion, cancellationToken, session);
                }
                if (diff > 0)
                {
                    await removeItems(hyperPotionsCount, hyperPotionsToRecycle, diff, ItemId.ItemHyperPotion, cancellationToken, session);
                }
                if (diff > 0)
                {
                    await removeItems(maxPotionCount, maxPotionsToRecycle, diff, ItemId.ItemMaxPotion, cancellationToken, session);
                }
            }
        }

        private static async Task OptimizedRecycleBerries(ISession session, CancellationToken cancellationToken)
        {
            var razz = await session.Inventory.GetItemAmountByType(ItemId.ItemRazzBerry);
            var bluk = await session.Inventory.GetItemAmountByType(ItemId.ItemBlukBerry);
            var nanab = await session.Inventory.GetItemAmountByType(ItemId.ItemNanabBerry);
            var pinap = await session.Inventory.GetItemAmountByType(ItemId.ItemPinapBerry);
            var wepar = await session.Inventory.GetItemAmountByType(ItemId.ItemWeparBerry);

            int razzToRecycle = 0;
            int blukToRecycle = 0;
            int nanabToRecycle = 0;
            int pinapToRecycle = 0;
            int weparToRecycle = 0;

            int totalBerryCount = razz + bluk + nanab + pinap + wepar;
            if (totalBerryCount > session.LogicSettings.TotalAmountOfBerriesToKeep)
            {
                int diff = totalBerryCount - session.LogicSettings.TotalAmountOfPotionsToKeep;
                if (diff > 0)
                {
                    await removeItems(razz, razzToRecycle, diff, ItemId.ItemRazzBerry, cancellationToken, session);
                }

                if (diff > 0)
                {
                    await removeItems(bluk, blukToRecycle, diff, ItemId.ItemBlukBerry, cancellationToken, session);
                }

                if (diff > 0)
                {
                    await removeItems(nanab, nanabToRecycle, diff, ItemId.ItemNanabBerry, cancellationToken, session);
                }

                if (diff > 0)
                {
                    await removeItems(pinap, pinapToRecycle, diff, ItemId.ItemPinapBerry, cancellationToken, session);
                }

                if (diff > 0)
                {
                    await removeItems(wepar, weparToRecycle, diff, ItemId.ItemWeparBerry, cancellationToken, session);
                }
            }
        }

        private static async Task OptimizedRecycleRevives(ISession session, CancellationToken cancellationToken)
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
                    await removeItems(reviveCount, revivesToRecycle, diff, ItemId.ItemRevive, cancellationToken, session);
                }
                if (diff > 0)
                {
                    await removeItems(maxReviveCount, maxRevivesToRecycle, diff, ItemId.ItemMaxRevive, cancellationToken, session);
                }
            }
        }

        private static async Task removeItems(int itemCount, int itemsToRecycle, int diff,
            ItemId item, CancellationToken cancellationToken, ISession session)
        {
            int itemsToKeep = itemCount - diff;
            if (itemsToKeep < 0)
            {
                itemsToKeep = 0;
            }
            itemsToRecycle = itemCount - itemsToKeep;

            if (itemsToRecycle != 0)
            {
                diff -= itemsToRecycle;
                cancellationToken.ThrowIfCancellationRequested();
                await session.Client.Inventory.RecycleItem(item, itemsToRecycle);
                session.EventDispatcher.Send(new ItemRecycledEvent { Id = item, Count = itemsToRecycle });
                if (session.LogicSettings.Teleport)
                    await Task.Delay(session.LogicSettings.DelayRecyleItem);
                else
                    await DelayingUtils.Delay(session.LogicSettings.DelayBetweenPlayerActions, 500);
            }
        }
    }
}