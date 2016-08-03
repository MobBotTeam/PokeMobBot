#region using directives

using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.State;
using PoGo.PokeMobBot.Logic.Utils;
using PokemonGo.RocketAPI;
using POGOProtos.Inventory.Item;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class RecycleItemsTask
    {
        private static int diff;
        private readonly Inventory _inventory;
        private readonly ILogicSettings _logicSettings;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly DelayingUtils _delayingUtils;
        private readonly Client _client;

        public RecycleItemsTask(Inventory inventory, ILogicSettings logicSettings, IEventDispatcher eventDispatcher, Client client, DelayingUtils delayingUtils)
        {
            _inventory = inventory;
            _logicSettings = logicSettings;
            _eventDispatcher = eventDispatcher;
            _client = client;
            _delayingUtils = delayingUtils;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _inventory.RefreshCachedInventory();
            var currentTotalItems = await _inventory.GetTotalItemCount();
            var recycleInventoryAtUsagePercentage = _logicSettings.RecycleInventoryAtUsagePercentage > 1
                ? _logicSettings.RecycleInventoryAtUsagePercentage / 100 : _logicSettings.RecycleInventoryAtUsagePercentage;

            var player = await _client.Player.GetPlayer();

            if (player.PlayerData.MaxItemStorage * recycleInventoryAtUsagePercentage > currentTotalItems)
                return;
            var items = await _inventory.GetItemsToRecycle();
            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _client.Inventory.RecycleItem(item.ItemId, item.Count);
                _eventDispatcher.Send(new ItemRecycledEvent { Id = item.ItemId, Count = item.Count });
                if (_logicSettings.Teleport)
                    await Task.Delay(_logicSettings.DelayRecyleItem);
                else
                    await _delayingUtils.Delay(_logicSettings.DelayBetweenPlayerActions, 500);
            }

            await OptimizedRecycleBalls(cancellationToken);
            await OptimizedRecyclePotions( cancellationToken);
            await OptimizedRecycleRevives( cancellationToken);
            await OptimizedRecycleBerries( cancellationToken);

            await _inventory.RefreshCachedInventory();
        }

        private async Task OptimizedRecycleBalls(CancellationToken cancellationToken)
        {
            var pokeBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemPokeBall);
            var greatBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemGreatBall);
            var ultraBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemUltraBall);
            var masterBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemMasterBall);
            int totalBallsCount = pokeBallsCount + greatBallsCount + ultraBallsCount + masterBallsCount;

            var pokeBallsToKeep = _logicSettings.TotalAmountOfPokeballsToKeep;
            var greatBallsToKeep = _logicSettings.TotalAmountOfGreatballsToKeep;
            var ultraBallsToKeep = _logicSettings.TotalAmountOfUltraballsToKeep;
            var masterBallsToKeep = _logicSettings.TotalAmountOfMasterballsToKeep;

            int pokeBallsToRecycle = pokeBallsCount - pokeBallsToKeep;
            int greatBallsToRecycle = greatBallsCount - greatBallsToKeep;
            int ultraBallsToRecycle = ultraBallsCount - ultraBallsToKeep;
            int masterBallsToRecycle = masterBallsCount - masterBallsToKeep;

            if (!_logicSettings.AutomaticInventoryManagement)
            {
                if (pokeBallsCount > pokeBallsToKeep)
                {
                    await RemoveItems(pokeBallsToRecycle, ItemId.ItemPokeBall, cancellationToken);
                }
                if (greatBallsCount > greatBallsToKeep)
                {
                    await RemoveItems(greatBallsToRecycle, ItemId.ItemGreatBall, cancellationToken);
                }
                if (ultraBallsCount > ultraBallsToKeep)
                {
                    await RemoveItems(ultraBallsToRecycle, ItemId.ItemUltraBall, cancellationToken);
                }
                if (masterBallsCount > masterBallsToKeep)
                {
                    await RemoveItems(masterBallsToRecycle, ItemId.ItemMasterBall, cancellationToken);
                }
            }
            else
            {
                if (totalBallsCount > _logicSettings.AutomaticMaxAllPokeballs)
                {
                    diff = totalBallsCount - _logicSettings.AutomaticMaxAllPokeballs;
                    if (diff > 0)
                    {
                        await RemoveItems(pokeBallsCount, ItemId.ItemPokeBall, cancellationToken);
                    }
                    if (diff > 0)
                    {
                        await RemoveItems(greatBallsCount, ItemId.ItemGreatBall, cancellationToken);
                    }
                    if (diff > 0)
                    {
                        await RemoveItems(ultraBallsCount, ItemId.ItemUltraBall, cancellationToken);
                    }
                    if (diff > 0)
                    {
                        await RemoveItems(masterBallsCount, ItemId.ItemMasterBall, cancellationToken);
                    }
                }
            }
        }


        private async Task OptimizedRecyclePotions(CancellationToken cancellationToken)
        {
            var potionCount = await _inventory.GetItemAmountByType(ItemId.ItemPotion);
            var superPotionCount = await _inventory.GetItemAmountByType(ItemId.ItemSuperPotion);
            var hyperPotionsCount = await _inventory.GetItemAmountByType(ItemId.ItemHyperPotion);
            var maxPotionCount = await _inventory.GetItemAmountByType(ItemId.ItemMaxPotion);
            int totalPotionsCount = potionCount + superPotionCount + hyperPotionsCount + maxPotionCount;

            int potionsToKeep = _logicSettings.TotalAmountOfPotionsToKeep;
            int superPotionsToKeep = _logicSettings.TotalAmountOfSuperPotionsToKeep;
            int hyperPotionsToKeep = _logicSettings.TotalAmountOfHyperPotionsToKeep;
            int maxPotionsToKeep = _logicSettings.TotalAmountOfMaxPotionsToKeep;

            int potionsToRecycle = potionCount - potionsToKeep;
            int superPotionsToRecycle = superPotionCount - superPotionsToKeep;
            int hyperPotionsToRecycle = hyperPotionsCount - hyperPotionsToKeep;
            int maxPotionsToRecycle = maxPotionCount - maxPotionsToKeep;

            if (!_logicSettings.AutomaticInventoryManagement)
            {
                if (potionCount > potionsToKeep)
                {
                    await RemoveItems(potionsToRecycle, ItemId.ItemPotion, cancellationToken);
                }
                if (superPotionCount > superPotionsToKeep)
                {
                    await RemoveItems(superPotionsToRecycle, ItemId.ItemSuperPotion, cancellationToken);
                }
                if (hyperPotionsCount > hyperPotionsToKeep)
                {
                    await RemoveItems(hyperPotionsToRecycle, ItemId.ItemHyperPotion, cancellationToken);
                }
                if (maxPotionCount > maxPotionsToKeep)
                {
                    await RemoveItems(maxPotionsToRecycle, ItemId.ItemMaxPotion, cancellationToken);
                }
            }
            else
            {
                if (totalPotionsCount > _logicSettings.AutomaticMaxAllPotions)
                {
                    diff = totalPotionsCount - _logicSettings.AutomaticMaxAllPotions;
                    if (diff > 0)
                    {
                        await RemoveItems(potionCount, ItemId.ItemPotion, cancellationToken);
                    }
                    if (diff > 0)
                    {
                        await RemoveItems(superPotionCount, ItemId.ItemSuperPotion, cancellationToken);
                    }
                    if (diff > 0)
                    {
                        await RemoveItems(hyperPotionsCount, ItemId.ItemHyperPotion, cancellationToken);
                    }
                    if (diff > 0)
                    {
                        await RemoveItems(maxPotionCount, ItemId.ItemMaxPotion, cancellationToken);
                    }
                }
            }
        }

        private async Task OptimizedRecycleBerries(CancellationToken cancellationToken)
        {
            var razzCount = await _inventory.GetItemAmountByType(ItemId.ItemRazzBerry);
            var blukCount = await _inventory.GetItemAmountByType(ItemId.ItemBlukBerry);
            var nanabCount = await _inventory.GetItemAmountByType(ItemId.ItemNanabBerry);
            var pinapCount = await _inventory.GetItemAmountByType(ItemId.ItemPinapBerry);
            var weparCount = await _inventory.GetItemAmountByType(ItemId.ItemWeparBerry);
            int totalBerryCount = razzCount + blukCount + nanabCount + pinapCount + weparCount;

            int razzToKeep = _logicSettings.TotalAmountOfRazzToKeep;
            //int blukToKeep = _logicSettings.TotalAmountOfBlukToKeep;
            //int nanabToKeep = _logicSettings.TotalAmountOfNanabToKeep;
            //int pinapToKeep = _logicSettings.TotalAmountOfPinapToKeep;
            //int weparToKeep = _logicSettings.TotalAmountOfWeparToKeep;

            int razzToRecycle = razzCount - razzToKeep;
            //int blukToRecycle = blukCount - blukToKeep;
            //int nanabToRecycle = nanabCount - nanabToKeep;
            //int pinapToRecycle = pinapCount - pinapToKeep;
            //int weparToRecycle = weparCount - weparToKeep;

            if (!_logicSettings.AutomaticInventoryManagement)
            {
                if (razzCount > razzToKeep)
                {
                    await RemoveItems(razzToRecycle, ItemId.ItemRazzBerry, cancellationToken);
                }
                //if (blukCount > blukToKeep)
                //{
                //    await RemoveItems(blukToRecycle, ItemId.ItemBlukBerry, cancellationToken);
                //}
                //if nanabCount > nanabToKeep)
                //{
                //    await RemoveItems(nanabToRecycle, ItemId.ItemNanabBerry, cancellationToken);
                //}
                //if (pinapCount > pinapToKeep)
                //{
                //    await RemoveItems(pinapToRecycle, ItemId.ItemPinapBerry, cancellationToken);
                //}
                //if (weparCount > weparToKeep)
                //{
                //    await RemoveItems(weparToRecycle, ItemId.ItemWeparBerry, cancellationToken);
                //}
            }
            else
            {
                if (totalBerryCount > _logicSettings.AutomaticMaxAllBerries)
                {
                    diff = totalBerryCount - _logicSettings.AutomaticMaxAllBerries;
                    if (diff > 0)
                    {
                        await RemoveItems(razzCount, ItemId.ItemRazzBerry, cancellationToken);
                    }

                    if (diff > 0)
                    {
                        await RemoveItems(blukCount, ItemId.ItemBlukBerry, cancellationToken);
                    }

                    if (diff > 0)
                    {
                        await RemoveItems(nanabCount, ItemId.ItemNanabBerry, cancellationToken);
                    }

                    if (diff > 0)
                    {
                        await RemoveItems(pinapCount, ItemId.ItemPinapBerry, cancellationToken);
                    }

                    if (diff > 0)
                    {
                        await RemoveItems(weparCount, ItemId.ItemWeparBerry, cancellationToken);
                    }
                }
            }
        }

        private async Task OptimizedRecycleRevives(CancellationToken cancellationToken)
        {
            var reviveCount = await _inventory.GetItemAmountByType(ItemId.ItemRevive);
            var maxReviveCount = await _inventory.GetItemAmountByType(ItemId.ItemMaxRevive);
            int totalRevivesCount = reviveCount + maxReviveCount;

            var revivesToKeep = _logicSettings.TotalAmountOfRevivesToKeep;
            var maxRevivesToKeep = _logicSettings.TotalAmountOfMaxRevivesToKeep;

            int revivesToRecycle = reviveCount - revivesToKeep;
            int maxRevivesToRecycle = maxReviveCount - maxRevivesToKeep;

            if (!_logicSettings.AutomaticInventoryManagement)
            {
                if (reviveCount > revivesToKeep)
                {
                    await RemoveItems(revivesToRecycle, ItemId.ItemRevive, cancellationToken);
                }
                if (maxReviveCount > maxRevivesToKeep)
                {
                    await RemoveItems(maxRevivesToRecycle, ItemId.ItemMaxRevive, cancellationToken);
                }
            }
            else
            {
                if (totalRevivesCount > _logicSettings.AutomaticMaxAllRevives)
                {
                    diff = totalRevivesCount - _logicSettings.AutomaticMaxAllRevives;
                    if (diff > 0)
                    {
                        await RemoveItems(reviveCount, ItemId.ItemRevive, cancellationToken);
                    }
                    if (diff > 0)
                    {
                        await RemoveItems(maxReviveCount, ItemId.ItemMaxRevive, cancellationToken);
                    }
                }
            }
        }

        private async Task RemoveItems(int itemCount, ItemId item, CancellationToken cancellationToken)
        {
            int itemsToRecycle = 0;
            if (_logicSettings.AutomaticInventoryManagement)
            {  
                int itemsToKeep = itemCount - diff;
                if (itemsToKeep < 0)
                {
                    itemsToKeep = 0;
                }
                itemsToRecycle = itemCount - itemsToKeep;
                diff -= itemsToRecycle;
            } else
            {
                itemsToRecycle = itemCount;
            }
            if (itemsToRecycle != 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _client.Inventory.RecycleItem(item, itemsToRecycle);
                _eventDispatcher.Send(new ItemRecycledEvent { Id = item, Count = itemsToRecycle });
                if (_logicSettings.Teleport)
                    await Task.Delay(_logicSettings.DelayRecyleItem);
                else
                    await _delayingUtils.Delay(_logicSettings.DelayBetweenPlayerActions, 500);
            }
        }
    }
}