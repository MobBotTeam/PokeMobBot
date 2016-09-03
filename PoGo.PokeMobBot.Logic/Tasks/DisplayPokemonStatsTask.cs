#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.DataDumper;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.PoGoUtils;
using PoGo.PokeMobBot.Logic.Service;
using PoGo.PokeMobBot.Logic.State;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class DisplayPokemonStatsTask
    {
        private readonly Inventory _inventory;
        private readonly ILogicSettings _logicSettings;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly Dumper _dumper;
        private readonly ITranslation _translation;
        private readonly PokemonInfo _pokemonInfo;
        private readonly PokemonAnalysisService _pokemonAnalysisService;

        public static List<ulong> PokemonId = new List<ulong>();


        public static List<ulong> PokemonIdcp = new List<ulong>();

        public DisplayPokemonStatsTask(Inventory inventory, ILogicSettings logicSettings, IEventDispatcher eventDispatcher, Dumper dumper, ITranslation translation, PokemonInfo pokemonInfo, PokemonAnalysisService pokemonAnalysisService)
        {
            _inventory = inventory;
            _logicSettings = logicSettings;
            _eventDispatcher = eventDispatcher;
            _dumper = dumper;
            _translation = translation;
            _pokemonInfo = pokemonInfo;
            _pokemonAnalysisService = pokemonAnalysisService;
        }

        public async Task Execute()
        {

            var trainerLevel = 40;

            var highestsPokemonCp = await _inventory.GetHighestsCp(_logicSettings.AmountOfPokemonToDisplayOnStart);
            var pokemonPairedWithStatsCp = highestsPokemonCp.Select(pokemon => _pokemonAnalysisService.GetPokemonAnalysis(pokemon, trainerLevel)).ToList();

            var highestsPokemonCpForUpgrade = await _inventory.GetHighestsCp(50);
            var pokemonPairedWithStatsCpForUpgrade = highestsPokemonCpForUpgrade.Select(pokemon => _pokemonAnalysisService.GetPokemonAnalysis(pokemon, trainerLevel)).ToList();

            var highestsPokemonPerfect = await _inventory.GetHighestsPerfect(_logicSettings.AmountOfPokemonToDisplayOnStart);
            var pokemonPairedWithStatsIv = highestsPokemonPerfect.Select(pokemon => _pokemonAnalysisService.GetPokemonAnalysis(pokemon, trainerLevel)).ToList();

            var highestsPokemonIvForUpgrade = await _inventory.GetHighestsPerfect(50);
            var pokemonPairedWithStatsIvForUpgrade = highestsPokemonIvForUpgrade.Select(pokemon => _pokemonAnalysisService.GetPokemonAnalysis(pokemon, trainerLevel)).ToList();

            _eventDispatcher.Send(
                new DisplayHighestsPokemonEvent
                {
                    SortedBy = "CP",
                    PokemonList = pokemonPairedWithStatsCp,
                    DisplayPokemonMaxPoweredCp = _logicSettings.DisplayPokemonMaxPoweredCp,
                    DisplayPokemonMovesetRank = _logicSettings.DisplayPokemonMovesetRank
                });
            if (_logicSettings.Teleport)
                await Task.Delay(_logicSettings.DelayDisplayPokemon);
            else
                await Task.Delay(500);

            _eventDispatcher.Send(
                new DisplayHighestsPokemonEvent
                {
                    SortedBy = "IV",
                    PokemonList = pokemonPairedWithStatsIv,
                    DisplayPokemonMaxPoweredCp = _logicSettings.DisplayPokemonMaxPoweredCp,
                    DisplayPokemonMovesetRank = _logicSettings.DisplayPokemonMovesetRank
                });

            var allPokemonInBag = _logicSettings.PrioritizeIvOverCp
                ? await _inventory.GetHighestsPerfect(1000)
                : await _inventory.GetHighestsCp(1000);
            if (_logicSettings.DumpPokemonStats)
            {
                const string dumpFileName = "PokeBagStats";
                string toDumpCSV = "Name,Level,CP,IV,Move1,Move2\r\n";
                string toDumpTXT = "";
                _dumper.ClearDumpFile(dumpFileName);
                _dumper.ClearDumpFile(dumpFileName, "csv");

                foreach (var pokemon in allPokemonInBag)
                {
                    toDumpTXT += $"NAME: {_translation.GetPokemonName(pokemon.PokemonId).PadRight(16, ' ')}Lvl: {_pokemonInfo.GetLevel(pokemon).ToString("00")}\t\tCP: {pokemon.Cp.ToString().PadRight(8, ' ')}\t\t IV: {_pokemonInfo.CalculatePokemonPerfection(pokemon).ToString("0.00")}%\t\t\tMOVE1: {pokemon.Move1}\t\t\tMOVE2: {pokemon.Move2}\r\n";
                    toDumpCSV += $"{_translation.GetPokemonName(pokemon.PokemonId)},{_pokemonInfo.GetLevel(pokemon).ToString("00")},{pokemon.Cp},{_pokemonInfo.CalculatePokemonPerfection(pokemon).ToString("0.00")}%,{pokemon.Move1},{pokemon.Move2}\r\n";
                }

                _dumper.Dump(toDumpTXT, dumpFileName);
                _dumper.Dump(toDumpCSV, dumpFileName, "csv");
            }
            if (_logicSettings.Teleport)
                await Task.Delay(_logicSettings.DelayDisplayPokemon);
            else
                await Task.Delay(500);
        }
    }
}
