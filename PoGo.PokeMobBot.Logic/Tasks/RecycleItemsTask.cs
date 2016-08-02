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

            // Refresh inventory so that the player stats are fresh
            await _inventory.RefreshCachedInventory();

            var currentTotalItems = await _inventory.GetTotalItemCount();
            var playerResponse = await _client.Player.GetPlayer();
            if (playerResponse.PlayerData.MaxItemStorage * _logicSettings.RecycleInventoryAtUsagePercentage > currentTotalItems)
                return;

            var items = await _inventory.GetItemsToRecycle();

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await _client.Inventory.RecycleItem(item.ItemId, item.Count);

                _eventDispatcher.Send(new ItemRecycledEvent { Id = item.ItemId, Count = item.Count });
                if (_logicSettings.Teleport)
                    await Task.Delay(_logicSettings.DelayRecyleItem, cancellationToken);
                else
                    await _delayingUtils.Delay(_logicSettings.DelayBetweenPlayerActions, 500);
            }

            if (_logicSettings.TotalAmountOfPokeballsToKeep != 0)
            {
                await OptimizedRecycleBalls(cancellationToken);
            }

            if (_logicSettings.TotalAmountOfPotionsToKeep != 0)
            {
                await OptimizedRecyclePotions(cancellationToken);
            }

            if (_logicSettings.TotalAmountOfRevivesToKeep != 0)
            {
                await OptimizedRecycleRevives(cancellationToken);
            }

            await _inventory.RefreshCachedInventory();
        }

        private async Task OptimizedRecycleBalls(CancellationToken cancellationToken)
        {
            var pokeBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemPokeBall);
            var greatBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemGreatBall);
            var ultraBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemUltraBall);
            var masterBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemMasterBall);

            int pokeBallsToRecycle = 0;
            int greatBallsToRecycle = 0;
            //int ultraBallsToRecycle = 0;
            //int masterBallsToRecycle = 0;
            //unused at the moment
            //TODO: implement these with reasonable settings


            int totalBallsCount = pokeBallsCount + greatBallsCount + ultraBallsCount + masterBallsCount;
            if (totalBallsCount > _logicSettings.TotalAmountOfPokeballsToKeep)
            {
                int diff = totalBallsCount - _logicSettings.TotalAmountOfPokeballsToKeep;
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
                        await _client.Inventory.RecycleItem(ItemId.ItemPokeBall, pokeBallsToRecycle);
                        _eventDispatcher.Send(new ItemRecycledEvent { Id = ItemId.ItemPokeBall, Count = pokeBallsToRecycle });
                        if (_logicSettings.Teleport)
                            await Task.Delay(_logicSettings.DelayRecyleItem);
                        else
                            await _delayingUtils.Delay(_logicSettings.DelayBetweenPlayerActions, 500);
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
                        await _client.Inventory.RecycleItem(ItemId.ItemGreatBall, greatBallsToRecycle);
                        _eventDispatcher.Send(new ItemRecycledEvent { Id = ItemId.ItemGreatBall, Count = greatBallsToRecycle });
                        if (_logicSettings.Teleport)
                            await Task.Delay(_logicSettings.DelayRecyleItem);
                        else
                            await _delayingUtils.Delay(_logicSettings.DelayBetweenPlayerActions, 500);
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

            int potionsToRecycle = 0;
            int superPotionsToRecycle = 0;
            int hyperPotionsToRecycle = 0;
            int maxPotionsToRecycle = 0;

            int totalPotionsCount = potionCount + superPotionCount + hyperPotionsCount + maxPotionCount;
            if (totalPotionsCount > _logicSettings.TotalAmountOfPotionsToKeep)
            {
                int diff = totalPotionsCount - _logicSettings.TotalAmountOfPotionsToKeep;
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
                        await _client.Inventory.RecycleItem(ItemId.ItemPotion, potionsToRecycle);
                        _eventDispatcher.Send(new ItemRecycledEvent { Id = ItemId.ItemPotion, Count = potionsToRecycle });
                        if (_logicSettings.Teleport)
                            await Task.Delay(_logicSettings.DelayRecyleItem);
                        else
                            await _delayingUtils.Delay(_logicSettings.DelayBetweenPlayerActions, 500);
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
                        await _client.Inventory.RecycleItem(ItemId.ItemSuperPotion, superPotionsToRecycle);
                        _eventDispatcher.Send(new ItemRecycledEvent { Id = ItemId.ItemSuperPotion, Count = superPotionsToRecycle });
                        if (_logicSettings.Teleport)
                            await Task.Delay(_logicSettings.DelayRecyleItem);
                        else
                            await _delayingUtils.Delay(_logicSettings.DelayBetweenPlayerActions, 500);
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
                        await _client.Inventory.RecycleItem(ItemId.ItemHyperPotion, hyperPotionsToRecycle);
                        _eventDispatcher.Send(new ItemRecycledEvent { Id = ItemId.ItemHyperPotion, Count = hyperPotionsToRecycle });
                        if (_logicSettings.Teleport)
                            await Task.Delay(_logicSettings.DelayRecyleItem);
                        else
                            await _delayingUtils.Delay(_logicSettings.DelayBetweenPlayerActions, 500);
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
                        await _client.Inventory.RecycleItem(ItemId.ItemMaxPotion, maxPotionsToRecycle);
                        _eventDispatcher.Send(new ItemRecycledEvent { Id = ItemId.ItemMaxPotion, Count = maxPotionsToRecycle });
                        if (_logicSettings.Teleport)
                            await Task.Delay(_logicSettings.DelayRecyleItem);
                        else
                            await _delayingUtils.Delay(_logicSettings.DelayBetweenPlayerActions, 500);
                    }
                }
            }
        }

        private async Task OptimizedRecycleRevives(CancellationToken cancellationToken)
        {
            var reviveCount = await _inventory.GetItemAmountByType(ItemId.ItemRevive);
            var maxReviveCount = await _inventory.GetItemAmountByType(ItemId.ItemMaxRevive);

            int revivesToRecycle = 0;
            int maxRevivesToRecycle = 0;

            int totalRevivesCount = reviveCount + maxReviveCount;
            if (totalRevivesCount > _logicSettings.TotalAmountOfRevivesToKeep)
            {
                int diff = totalRevivesCount - _logicSettings.TotalAmountOfRevivesToKeep;
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
                        await _client.Inventory.RecycleItem(ItemId.ItemRevive, revivesToRecycle);
                        _eventDispatcher.Send(new ItemRecycledEvent { Id = ItemId.ItemRevive, Count = revivesToRecycle });
                        if (_logicSettings.Teleport)
                            await Task.Delay(_logicSettings.DelayRecyleItem);
                        else
                            await _delayingUtils.Delay(_logicSettings.DelayBetweenPlayerActions, 500);
                    }
                }
            }
        }
    }
}