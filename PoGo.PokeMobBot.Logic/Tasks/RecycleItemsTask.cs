#region using directives

using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Utils;
using PokemonGo.RocketAPI;
using POGOProtos.Inventory.Item;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class RecycleItemsTask
    {
        private readonly DelayingUtils _delayingUtils;
        private readonly Inventory _inventory;
        private readonly ILogicSettings _logicSettings;
        private readonly Client _client;
        private readonly IEventDispatcher _eventDispatcher;

        public RecycleItemsTask(DelayingUtils delayingUtils, Inventory inventory, ILogicSettings logicSettings, Client client, IEventDispatcher eventDispatcher)
        {
            _delayingUtils = delayingUtils;
            _inventory = inventory;
            _logicSettings = logicSettings;
            _client = client;
            _eventDispatcher = eventDispatcher;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _inventory.RefreshCachedInventory();
            var currentTotalItems = await _inventory.GetTotalItemCount();
            var profile = await _client.Player.GetPlayer();
            if (profile.PlayerData.MaxItemStorage * _logicSettings.RecycleInventoryAtUsagePercentage > currentTotalItems)
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
            await OptimizedRecyclePotions(cancellationToken);
            await OptimizedRecycleRevives(cancellationToken);
            await OptimizedRecycleBerries(cancellationToken);

            await _inventory.RefreshCachedInventory();
        }

        private async Task OptimizedRecycleBalls(CancellationToken cancellationToken)
        {
            var pokeBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemPokeBall);
            var greatBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemGreatBall);
            var ultraBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemUltraBall);
            //var masterBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemMasterBall);
            var pokeBallsToKeep = _logicSettings.TotalAmountOfPokeballsToKeep;
            var greatBallsToKeep = _logicSettings.TotalAmountOfGreatballsToKeep;
            var ultraBallsToKeep = _logicSettings.TotalAmountOfUltraballsToKeep;
            //var masterBallsToKeep = _logicSettings.TotalAmountOfMasterballsToKeep;
            int pokeBallsToRecycle = pokeBallsCount - pokeBallsToKeep;
            int greatBallsToRecycle = greatBallsCount - greatBallsToKeep;
            int ultraBallsToRecycle = ultraBallsCount - ultraBallsToKeep;
            //int masterBallsToRecycle = masterBallsCount - masterBallsToKeep;
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
            //if (masterBallsCount > masterBallsToKeep)
            //{
            //    await RemoveItems(masterBallsToRecycle, ItemId.ItemMasterBall, cancellationToken, session);
            //}
        }


        private async Task OptimizedRecyclePotions(CancellationToken cancellationToken)
        {
            var potionCount = await _inventory.GetItemAmountByType(ItemId.ItemPotion);
            var superPotionCount = await _inventory.GetItemAmountByType(ItemId.ItemSuperPotion);
            var hyperPotionsCount = await _inventory.GetItemAmountByType(ItemId.ItemHyperPotion);
            var maxPotionCount = await _inventory.GetItemAmountByType(ItemId.ItemMaxPotion);
            int potionsToKeep = _logicSettings.TotalAmountOfPotionsToKeep;
            int superPotionsToKeep = _logicSettings.TotalAmountOfSuperPotionsToKeep;
            int hyperPotionsToKeep = _logicSettings.TotalAmountOfHyperPotionsToKeep;
            int maxPotionsToKeep = _logicSettings.TotalAmountOfMaxPotionsToKeep;
            int potionsToRecycle = potionCount - potionsToKeep;
            int superPotionsToRecycle = superPotionCount - superPotionsToKeep;
            int hyperPotionsToRecycle = hyperPotionsCount - hyperPotionsToKeep;
            int maxPotionsToRecycle = maxPotionCount - maxPotionsToKeep;
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

        private async Task OptimizedRecycleBerries(CancellationToken cancellationToken)
        {
            var razzCount = await _inventory.GetItemAmountByType(ItemId.ItemRazzBerry);
            //var blukCount = await _inventory.GetItemAmountByType(ItemId.ItemBlukBerry);
            //var nanabCount = await _inventory.GetItemAmountByType(ItemId.ItemNanabBerry);
            //var pinapCount = await _inventory.GetItemAmountByType(ItemId.ItemPinapBerry);
            //var weparCount = await _inventory.GetItemAmountByType(ItemId.ItemWeparBerry);
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
            if (razzCount > razzToKeep)
            {
                await RemoveItems(razzToRecycle, ItemId.ItemRazzBerry, cancellationToken);
            }

            //if (blukCount > blukToKeep)
            //{
            //    await RemoveItems(blukToRecycle, ItemId.ItemBlukBerry, cancellationToken, session);
            //}

            //if nanabCount > nanabToKeep)
            //{
            //    await RemoveItems(nanabToRecycle, ItemId.ItemNanabBerry, cancellationToken, session);
            //}

            //if (pinapCount > pinapToKeep)
            //{
            //    await RemoveItems(pinapToRecycle, ItemId.ItemPinapBerry, cancellationToken, session);
            //}

            //if (weparCount > weparToKeep)
            //{
            //    await RemoveItems(weparToRecycle, ItemId.ItemWeparBerry, cancellationToken, session);
            //}
        }

        private async Task OptimizedRecycleRevives(CancellationToken cancellationToken)
        {
            var reviveCount = await _inventory.GetItemAmountByType(ItemId.ItemRevive);
            var maxReviveCount = await _inventory.GetItemAmountByType(ItemId.ItemMaxRevive);
            var revivesToKeep = _logicSettings.TotalAmountOfRevivesToKeep;
            var maxRevivesToKeep = _logicSettings.TotalAmountOfMaxRevivesToKeep;
            int revivesToRecycle = reviveCount - revivesToKeep;
            int maxRevivesToRecycle = maxReviveCount - maxRevivesToKeep;
            if (reviveCount > revivesToKeep)
            {

                await RemoveItems(revivesToRecycle, ItemId.ItemRevive, cancellationToken);
            }
            if (maxReviveCount > maxRevivesToKeep)
            {
                await RemoveItems(maxRevivesToRecycle, ItemId.ItemMaxRevive, cancellationToken);
            }
        }

        private async Task RemoveItems(int itemCount, ItemId item, CancellationToken cancellationToken)
        {
            if (itemCount != 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _client.Inventory.RecycleItem(item, itemCount);
                _eventDispatcher.Send(new ItemRecycledEvent { Id = item, Count = itemCount });
                if (_logicSettings.Teleport)
                    await Task.Delay(_logicSettings.DelayRecyleItem);
                else
                    await _delayingUtils.Delay(_logicSettings.DelayBetweenPlayerActions, 500);
            }
        }
    }
}