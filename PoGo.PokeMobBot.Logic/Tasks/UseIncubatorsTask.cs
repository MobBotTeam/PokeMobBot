﻿#region using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.PoGoUtils;
using PoGo.PokeMobBot.Logic.State;
using PokemonGo.RocketAPI;
using POGOProtos.Inventory.Item;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class UseIncubatorsTask
    {
        private readonly PokemonInfo _pokemonInfo;
        private readonly Inventory _inventory;
        private readonly ILogicSettings _logicSettings;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly Client _client;

        public UseIncubatorsTask(PokemonInfo pokemonInfo, Inventory inventory, ILogicSettings logicSettings, IEventDispatcher eventDispatcher, Client client)
        {
            _pokemonInfo = pokemonInfo;
            _inventory = inventory;
            _logicSettings = logicSettings;
            _eventDispatcher = eventDispatcher;
            _client = client;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

            var rememberedIncubatorsFilePath = Path.Combine(_logicSettings.ProfilePath, "temp", "incubators.json");
            var rememberedIncubators = GetRememberedIncubators(rememberedIncubatorsFilePath);
            var pokemons = (await _inventory.GetPokemons()).ToList();

            // Check if eggs in remembered incubator usages have since hatched
            // (instead of calling session.Client.Inventory.GetHatchedEgg(), which doesn't seem to work properly)
            foreach (var incubator in rememberedIncubators)
            {
                var hatched = pokemons.FirstOrDefault(x => !x.IsEgg && x.Id == incubator.PokemonId);
                if (hatched == null) continue;

                _eventDispatcher.Send(new EggHatchedEvent
                {
                    Id = hatched.Id,
                    PokemonId = hatched.PokemonId,
                    Level = _pokemonInfo.GetLevel(hatched),
                    Cp = hatched.Cp,
                    MaxCp = _pokemonInfo.CalculateMaxCp(hatched),
                    Perfection = Math.Round(_pokemonInfo.CalculatePokemonPerfection(hatched), 2)
                });
            }

            var newRememberedIncubators = new List<IncubatorUsage>();

            foreach (var incubator in incubators)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (incubator.PokemonId == 0)
                {
                    // Unlimited incubators prefer short eggs, limited incubators prefer long eggs
                    var egg = incubator.ItemId == ItemId.ItemIncubatorBasicUnlimited
                        ? unusedEggs.FirstOrDefault()
                        : unusedEggs.LastOrDefault();

                    if (egg == null)
                        continue;

                    var response = await _client.Inventory.UseItemEggIncubator(incubator.Id, egg.Id);
                    unusedEggs.Remove(egg);

                    newRememberedIncubators.Add(new IncubatorUsage {IncubatorId = incubator.Id, PokemonId = egg.Id});

                    _eventDispatcher.Send(new EggIncubatorStatusEvent
                    {
                        IncubatorId = incubator.Id,
                        WasAddedNow = true,
                        PokemonId = egg.Id,
                        KmToWalk = egg.EggKmWalkedTarget,
                        KmRemaining = response.EggIncubator.TargetKmWalked - kmWalked
                    });
                }
                else
                {
                    newRememberedIncubators.Add(new IncubatorUsage
                    {
                        IncubatorId = incubator.Id,
                        PokemonId = incubator.PokemonId
                    });

                    _eventDispatcher.Send(new EggIncubatorStatusEvent
                    {
                        IncubatorId = incubator.Id,
                        PokemonId = incubator.PokemonId,
                        KmToWalk = incubator.TargetKmWalked - incubator.StartKmWalked,
                        KmRemaining = incubator.TargetKmWalked - kmWalked
                    });
                }
            }

            if (!newRememberedIncubators.SequenceEqual(rememberedIncubators))
                SaveRememberedIncubators(newRememberedIncubators, rememberedIncubatorsFilePath);
        }

        private static List<IncubatorUsage> GetRememberedIncubators(string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            if (File.Exists(filePath))
                return JsonConvert.DeserializeObject<List<IncubatorUsage>>(File.ReadAllText(filePath, Encoding.UTF8));

            return new List<IncubatorUsage>(0);
        }

        private static void SaveRememberedIncubators(List<IncubatorUsage> incubators, string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            File.WriteAllText(filePath, JsonConvert.SerializeObject(incubators), Encoding.UTF8);
        }

        private class IncubatorUsage : IEquatable<IncubatorUsage>
        {
            public string IncubatorId;
            public ulong PokemonId;

            public bool Equals(IncubatorUsage other)
            {
                return other != null && other.IncubatorId == IncubatorId && other.PokemonId == PokemonId;
            }
        }
    }
}