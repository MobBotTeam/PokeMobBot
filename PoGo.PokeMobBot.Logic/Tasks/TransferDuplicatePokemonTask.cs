#region using directives

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.PoGoUtils;
using PoGo.PokeMobBot.Logic.State;
using PoGo.PokeMobBot.Logic.Utils;
using PokemonGo.RocketAPI;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class TransferDuplicatePokemonTask
    {
        private readonly PokemonInfo _pokemonInfo;
        private readonly DelayingUtils _delayingUtils;
        private readonly Inventory _inventory;
        private readonly ILogicSettings _logicSettings;
        private readonly Client _client;
        private readonly IEventDispatcher _eventDispatcher;

        public TransferDuplicatePokemonTask(PokemonInfo pokemonInfo, DelayingUtils delayingUtils, Inventory inventory, ILogicSettings logicSettings, Client client, IEventDispatcher eventDispatcher)
        {
            _pokemonInfo = pokemonInfo;
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

            var duplicatePokemons =
                await
                    _inventory.GetDuplicatePokemonToTransfer(_logicSettings.KeepPokemonsThatCanEvolve,
                        _logicSettings.PrioritizeIvOverCp,
                        _logicSettings.PokemonsNotToTransfer);

            var pokemonSettings = await _inventory.GetPokemonSettings();
            var pokemonFamilies = await _inventory.GetPokemonFamilies();

            foreach (var duplicatePokemon in duplicatePokemons)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (duplicatePokemon.Cp >=
                    _inventory.GetPokemonTransferFilter(duplicatePokemon.PokemonId).KeepMinCp ||
                    _pokemonInfo.CalculatePokemonPerfection(duplicatePokemon) >
                    _inventory.GetPokemonTransferFilter(duplicatePokemon.PokemonId).KeepMinIvPercentage)
                {
                    continue;
                }

                await _client.Inventory.TransferPokemon(duplicatePokemon.Id);
                await _inventory.DeletePokemonFromInvById(duplicatePokemon.Id);

                var bestPokemonOfType = (_logicSettings.PrioritizeIvOverCp
                    ? await _inventory.GetHighestPokemonOfTypeByIv(duplicatePokemon)
                    : await _inventory.GetHighestPokemonOfTypeByCp(duplicatePokemon)) ?? duplicatePokemon;

                var setting = pokemonSettings.Single(q => q.PokemonId == duplicatePokemon.PokemonId);
                var family = pokemonFamilies.First(q => q.FamilyId == setting.FamilyId);

                family.Candy_++;

                _eventDispatcher.Send(new TransferPokemonEvent
                {
                    Id = duplicatePokemon.PokemonId,
                    Perfection = _pokemonInfo.CalculatePokemonPerfection(duplicatePokemon),
                    Cp = duplicatePokemon.Cp,
                    BestCp = bestPokemonOfType.Cp,
                    BestPerfection = _pokemonInfo.CalculatePokemonPerfection(bestPokemonOfType),
                    FamilyCandies = family.Candy_
                });
                if(_logicSettings.Teleport)
                    await Task.Delay(_logicSettings.DelayTransferPokemon, cancellationToken);
                else
                    await _delayingUtils.Delay(_logicSettings.DelayBetweenPlayerActions, 0);
            }
        }
    }
}