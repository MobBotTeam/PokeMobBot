#region using directives

using System;
using System.Linq;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Extensions;
using PoGo.PokeMobBot.Logic.PoGoUtils;
using PoGo.PokeMobBot.Logic.Utils;
using PokemonGo.RocketAPI;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class CatchPokemonTask
    {
        private readonly Random _rng = new Random();
        private readonly PokemonInfo _pokemonInfo;
        private readonly DelayingUtils _delayingUtils;
        private readonly LocationUtils _locationUtils;
        private readonly ILogicSettings _logicSettings;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ITranslation _translation;
        private readonly Client _client;
        private readonly Inventory _inventory;

        public CatchPokemonTask(PokemonInfo pokemonInfo, DelayingUtils delayingUtils, LocationUtils locationUtils, ILogicSettings logicSettings, IEventDispatcher eventDispatcher, ITranslation translation, Client client, Inventory inventory)
        {
            _pokemonInfo = pokemonInfo;
            _delayingUtils = delayingUtils;
            _locationUtils = locationUtils;
            _logicSettings = logicSettings;
            _eventDispatcher = eventDispatcher;
            _translation = translation;
            _client = client;
            _inventory = inventory;
        }

        public async Task Execute(dynamic encounter, MapPokemon pokemon,
            FortData currentFortData = null, ulong encounterId = 0)
        {
            if (encounter is EncounterResponse && pokemon == null)
                throw new ArgumentException("Parameter pokemon must be set, if encounter is of type EncounterResponse",
                    "pokemon");

            CatchPokemonResponse caughtPokemonResponse;
            var attemptCounter = 1;
            do
            {
                if (_logicSettings.MaxPokeballsPerPokemon > 0 && attemptCounter > _logicSettings.MaxPokeballsPerPokemon)
                    break;

                float probability = encounter?.CaptureProbability?.CaptureProbability_[0];

                var pokeball = await GetBestBall(encounter, probability);
                if (pokeball == ItemId.ItemUnknown)
                {
                    _eventDispatcher.Send(new NoPokeballEvent
                    {
                        Id = encounter is EncounterResponse ? pokemon.PokemonId : encounter?.PokemonData.PokemonId,
                        Cp =
                            (encounter is EncounterResponse
                                ? encounter.WildPokemon?.PokemonData?.Cp
                                : encounter?.PokemonData?.Cp) ?? 0
                    });
                    return;
                }

                var isLowProbability = probability < _logicSettings.UseBerryBelowCatchProbability;
                var isHighCp = encounter != null &&
                               (encounter is EncounterResponse
                                   ? encounter.WildPokemon?.PokemonData?.Cp
                                   : encounter.PokemonData?.Cp) > _logicSettings.UseBerryMinCp;
                var isHighPerfection =
                    _pokemonInfo.CalculatePokemonPerfection(encounter is EncounterResponse
                        ? encounter.WildPokemon?.PokemonData
                        : encounter?.PokemonData) >= _logicSettings.UseBerryMinIv;

                if (isLowProbability && ((_logicSettings.PrioritizeIvOverCp && isHighPerfection) || isHighCp))
                {
                    await
                        UseBerry(encounter is EncounterResponse || encounter is IncenseEncounterResponse
                                ? pokemon.EncounterId
                                : encounterId,
                            encounter is EncounterResponse || encounter is IncenseEncounterResponse
                                ? pokemon.SpawnPointId
                                : currentFortData?.Id);
                }

                var distance = _locationUtils.CalculateDistanceInMeters(_client.CurrentLatitude,
                    _client.CurrentLongitude,
                    encounter is EncounterResponse || encounter is IncenseEncounterResponse
                        ? pokemon.Latitude
                        : currentFortData.Latitude,
                    encounter is EncounterResponse || encounter is IncenseEncounterResponse
                        ? pokemon.Longitude
                        : currentFortData.Longitude);

                double normalizedRecticleSize, spinModifier;
                if (_logicSettings.HumanizeThrows)
                {
                    normalizedRecticleSize =
                        _rng.NextInRange(_logicSettings.ThrowAccuracyMin, _logicSettings.ThrowAccuracyMax) *
                        1.85 + 0.1; // 0.1..1.95
                    spinModifier = _rng.NextDouble() > _logicSettings.ThrowSpinFrequency ? 0.0 : 1.0;
                }
                else
                {
                    normalizedRecticleSize = 1.95;
                    spinModifier = 1.00;
                }
                caughtPokemonResponse =
                    await _client.Encounter.CatchPokemon(
                        encounter is EncounterResponse || encounter is IncenseEncounterResponse
                            ? pokemon.EncounterId
                            : encounterId,
                        encounter is EncounterResponse || encounter is IncenseEncounterResponse
                            ? pokemon.SpawnPointId
                            : currentFortData.Id, pokeball,
                        normalizedRecticleSize,
                        spinModifier);

                var lat = encounter is EncounterResponse || encounter is IncenseEncounterResponse
                    ? pokemon.Latitude
                    : currentFortData.Latitude;
                var lng = encounter is EncounterResponse || encounter is IncenseEncounterResponse
                    ? pokemon.Longitude
                    : currentFortData.Longitude;
                var evt = new PokemonCaptureEvent
                {
                    Status = caughtPokemonResponse.Status,
                    Latitude = lat,
                    Longitude = lng
                };

                if (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess)
                {
                    var totalExp = 0;

                    foreach (var xp in caughtPokemonResponse.CaptureAward.Xp)
                    {
                        totalExp += xp;
                    }
                    var profile = await _client.Player.GetPlayer();

                    evt.Exp = totalExp;
                    evt.Stardust = profile.PlayerData.Currencies.ToArray()[1].Amount;

                    var pokemonSettings = await _inventory.GetPokemonSettings();
                    var pokemonFamilies = await _inventory.GetPokemonFamilies();

                    var setting =
                        pokemonSettings.FirstOrDefault(q => pokemon != null && q.PokemonId == pokemon.PokemonId);
                    var family = pokemonFamilies.FirstOrDefault(q => setting != null && q.FamilyId == setting.FamilyId);

                    if (family != null)
                    {
                        family.Candy_ += caughtPokemonResponse.CaptureAward.Candy.Sum();

                        evt.FamilyCandies = family.Candy_;
                    }
                    else
                    {
                        evt.FamilyCandies = caughtPokemonResponse.CaptureAward.Candy.Sum();
                    }
                }


                evt.CatchType = encounter is EncounterResponse
                    ? _translation.GetTranslation(TranslationString.CatchTypeNormal)
                    : encounter is DiskEncounterResponse
                        ? _translation.GetTranslation(TranslationString.CatchTypeLure)
                        : _translation.GetTranslation(TranslationString.CatchTypeIncense);
                evt.Id = encounter is EncounterResponse ? pokemon.PokemonId : encounter?.PokemonData.PokemonId;
                evt.Level =
                    _pokemonInfo.GetLevel(encounter is EncounterResponse
                        ? encounter.WildPokemon?.PokemonData
                        : encounter?.PokemonData);
                evt.Cp = encounter is EncounterResponse
                    ? encounter.WildPokemon?.PokemonData?.Cp
                    : encounter?.PokemonData?.Cp ?? 0;
                evt.MaxCp =
                    _pokemonInfo.CalculateMaxCp(encounter is EncounterResponse
                        ? encounter.WildPokemon?.PokemonData
                        : encounter?.PokemonData);
                evt.Perfection =
                    Math.Round(
                        _pokemonInfo.CalculatePokemonPerfection(encounter is EncounterResponse
                            ? encounter.WildPokemon?.PokemonData
                            : encounter?.PokemonData), 2);
                evt.Probability =
                    Math.Round(probability * 100, 2);
                evt.Distance = distance;
                evt.Pokeball = pokeball;
                evt.Attempt = attemptCounter;
                await _inventory.RefreshCachedInventory();
                evt.BallAmount = await _inventory.GetItemAmountByType(pokeball);

                _eventDispatcher.Send(evt);

                attemptCounter++;
                if (_logicSettings.Teleport)
                    await Task.Delay(_logicSettings.DelayCatchPokemon);
                else
                    await _delayingUtils.Delay(_logicSettings.DelayBetweenPokemonCatch, 2000);
            } while (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed ||
                     caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape);
        }

        private async Task<ItemId> GetBestBall(dynamic encounter, float probability)
        {
            /*var pokemonCp = encounter is EncounterResponse //commented for possible future uses
                ? encounter.WildPokemon?.PokemonData?.Cp
                : encounter?.PokemonData?.Cp;*/
            var pokemonId = encounter is EncounterResponse
                ? encounter.WildPokemon?.PokemonData?.PokemonId
                : encounter?.PokemonData?.PokemonId;
            var iV =
                Math.Round(
                    _pokemonInfo.CalculatePokemonPerfection(encounter is EncounterResponse
                        ? encounter.WildPokemon?.PokemonData
                        : encounter?.PokemonData));

            var pokeBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemPokeBall);
            var greatBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemGreatBall);
            var ultraBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemUltraBall);
            var masterBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemMasterBall);

            if (masterBallsCount > 0 && !_logicSettings.PokemonToUseMasterball.Any() ||
                _logicSettings.PokemonToUseMasterball.Contains(pokemonId))
                return ItemId.ItemMasterBall;
            if (ultraBallsCount > 0 && iV >= _logicSettings.UseUltraBallAboveIv ||
                probability <= _logicSettings.UseUltraBallBelowCatchProbability)
                return ItemId.ItemUltraBall;
            if (greatBallsCount > 0 && iV >= _logicSettings.UseGreatBallAboveIv ||
                probability <= _logicSettings.UseGreatBallBelowCatchProbability)
                return ItemId.ItemGreatBall;
            if (pokeBallsCount > 0 && iV < _logicSettings.UseGreatBallAboveIv ||
                probability > _logicSettings.UseGreatBallBelowCatchProbability)
                return ItemId.ItemPokeBall;

            return ItemId.ItemUnknown;
        }

        private async Task UseBerry(ulong encounterId, string spawnPointId)
        {
            var inventoryBalls = await _inventory.GetItems();
            var berries = inventoryBalls.Where(p => p.ItemId == ItemId.ItemRazzBerry);
            var berry = berries.FirstOrDefault();

            if (berry == null || berry.Count <= 0)
                return;

            await _client.Encounter.UseCaptureItem(encounterId, ItemId.ItemRazzBerry, spawnPointId);
            berry.Count -= 1;
            _eventDispatcher.Send(new UseBerryEvent { Count = berry.Count });
        }
    }
}