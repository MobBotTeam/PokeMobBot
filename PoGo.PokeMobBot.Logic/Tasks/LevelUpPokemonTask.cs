#region using directives

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using POGOProtos.Settings.Master;
using POGOProtos.Data;
using PoGo.PokeMobBot.Logic.PoGoUtils;
using PokemonGo.RocketAPI;
using POGOProtos.Networking.Responses;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class LevelUpPokemonTask
    {
        private readonly PokemonInfo _pokemonInfo;
        private readonly Inventory _inventory;
        private readonly ILogicSettings _logicSettings;
        private readonly Client _client;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ITranslation _translation;

        public LevelUpPokemonTask(PokemonInfo pokemonInfo, Inventory inventory, ILogicSettings logicSettings, Client client, IEventDispatcher eventDispatcher, ITranslation translation)
        {
            _pokemonInfo = pokemonInfo;
            _inventory = inventory;
            _logicSettings = logicSettings;
            _client = client;
            _eventDispatcher = eventDispatcher;
            _translation = translation;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            // Refresh inventory so that the player stats are fresh
            await _inventory.RefreshCachedInventory();

            // get the families and the pokemons settings to do some actual smart stuff like checking if you have enough candy in the first place
            var pokemonFamilies = await _inventory.GetPokemonFamilies();
            var pokemonSettings = await _inventory.GetPokemonSettings();
            var pokemonUpgradeSettings = await _inventory.GetPokemonUpgradeSettings();
            var playerLevel = await _inventory.GetPlayerStats();

            List<PokemonData> allPokemon = new List<PokemonData>();

            var playerResponse = await _client.Player.GetPlayer();

            // priority for upgrading
            if (_logicSettings.LevelUpByCPorIv?.ToLower() == "iv")
            {
                allPokemon = _inventory.GetHighestsPerfect(playerResponse.PlayerData.MaxPokemonStorage).Result.ToList();
            }
            else if (_logicSettings.LevelUpByCPorIv?.ToLower() == "cp")
            {
                allPokemon = _inventory.GetPokemons().Result.OrderByDescending(p => p.Cp).ToList();
            }

            // iterate on whatever meets both minimums
            // to disable one or the other, set to 0
            foreach (var pokemon in allPokemon.Where(p => _inventory.GetPerfect(p) >= _logicSettings.UpgradePokemonIvMinimum && p.Cp >= _logicSettings.UpgradePokemonCpMinimum))
            {
                int pokeLevel = (int)_pokemonInfo.GetLevel(pokemon);
                var currentPokemonSettings = pokemonSettings.FirstOrDefault(q => pokemon != null && q.PokemonId.Equals(pokemon.PokemonId));
                var family = pokemonFamilies.FirstOrDefault(q => currentPokemonSettings != null && q.FamilyId.Equals(currentPokemonSettings.FamilyId));
                int candyToEvolveTotal = GetCandyMinToKeep(pokemonSettings, currentPokemonSettings);

                // you can upgrade up to player level+2 right now
                // may need translation for stardust???
                if (pokeLevel < playerLevel?.FirstOrDefault().Level + pokemonUpgradeSettings.FirstOrDefault().AllowedLevelsAbovePlayer
                    && family.Candy_ > pokemonUpgradeSettings.FirstOrDefault()?.CandyCost[pokeLevel]
                    && family.Candy_ >= candyToEvolveTotal
                    && playerResponse.PlayerData.Currencies.FirstOrDefault(c => c.Name.ToLower().Contains("stardust")).Amount >= pokemonUpgradeSettings.FirstOrDefault()?.StardustCost[pokeLevel])
                {
                    await DoUpgrade(pokemon);
                }
            }
        }

        private int GetCandyMinToKeep(IEnumerable<PokemonSettings> pokemonSettings, PokemonSettings currentPokemonSettings)
        {
            // total up required candy for evolution, for yourself and your ancestors to allow for others to be evolved before upgrading
            // always keeps a minimum amount in reserve, should never have 0 except for cases where a pokemon is in both first and final form (ie onix)
            var ancestor = pokemonSettings.FirstOrDefault(q => q.PokemonId == currentPokemonSettings.ParentPokemonId);
            var ancestor2 = pokemonSettings.FirstOrDefault(q => q.PokemonId == ancestor?.ParentPokemonId);

            int candyToEvolveTotal = currentPokemonSettings.CandyToEvolve;
            if (ancestor != null)
            {
                candyToEvolveTotal += ancestor.CandyToEvolve;
            }

            if (ancestor2 != null)
            {
                candyToEvolveTotal += ancestor2.CandyToEvolve;
            }

            return candyToEvolveTotal;
        }

        private async Task DoUpgrade(PokemonData pokemon)
        {
            var upgradeResult = await _inventory.UpgradePokemon(pokemon.Id);

            if (upgradeResult.Result == UpgradePokemonResponse.Types.Result.Success)
            {
                _eventDispatcher.Send(new NoticeEvent()
                {
                    Message = _translation.GetTranslation(TranslationString.PokemonUpgradeSuccess, _translation.GetPokemonName(upgradeResult.UpgradedPokemon.PokemonId), upgradeResult.UpgradedPokemon.Cp)
                });
            }
            else if (upgradeResult.Result == UpgradePokemonResponse.Types.Result.ErrorInsufficientResources)
            {
                _eventDispatcher.Send(new NoticeEvent()
                {
                    Message = _translation.GetTranslation(TranslationString.PokemonUpgradeFailed)
                });
            }
            // pokemon max level
            else if (upgradeResult.Result == UpgradePokemonResponse.Types.Result.ErrorUpgradeNotAvailable)
            {
                _eventDispatcher.Send(new NoticeEvent()
                {
                    Message = _translation.GetTranslation(TranslationString.PokemonUpgradeUnavailable, _translation.GetPokemonName(pokemon.PokemonId), pokemon.Cp, _pokemonInfo.CalculateMaxCp(pokemon))
                });
            }
            else
            {
                _eventDispatcher.Send(new NoticeEvent()
                {
                    Message = _translation.GetTranslation(TranslationString.PokemonUpgradeFailedError, _translation.GetPokemonName(pokemon.PokemonId))
                });
            }
        }
    }
}