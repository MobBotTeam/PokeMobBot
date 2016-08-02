#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.DataDumper;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.PoGoUtils;
using PoGo.PokeMobBot.Logic.State;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class DisplayPokemonStatsTask
    {
        private readonly Dumper _dumper;
        private readonly PokemonInfo _pokemonInfo;
        private readonly Inventory _inventory;
        private readonly ILogicSettings _logicSettings;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ITranslation _translation;

        public List<ulong> PokemonId = new List<ulong>();
        
        public List<ulong> PokemonIdcp = new List<ulong>();

        public DisplayPokemonStatsTask(Dumper dumper, PokemonInfo pokemonInfo, Inventory inventory, ILogicSettings logicSettings, IEventDispatcher eventDispatcher, ITranslation translation)
        {
            _dumper = dumper;
            _pokemonInfo = pokemonInfo;
            _inventory = inventory;
            _logicSettings = logicSettings;
            _eventDispatcher = eventDispatcher;
            _translation = translation;
        }

        public async Task Execute()
        {
            var highestsPokemonCp =
                await _inventory.GetHighestsCp(_logicSettings.AmountOfPokemonToDisplayOnStart);
            var highestsPokemonCpForUpgrade = await _inventory.GetHighestsCp(50);
            var highestsPokemonIvForUpgrade = await _inventory.GetHighestsPerfect(50);
            var pokemonPairedWithStatsCp =
                highestsPokemonCp.Select(
                    pokemon =>
                        Tuple.Create(pokemon, _pokemonInfo.CalculateMaxCp(pokemon),
                            _pokemonInfo.CalculatePokemonPerfection(pokemon), _pokemonInfo.GetLevel(pokemon),
                            _pokemonInfo.GetPokemonMove1(pokemon), _pokemonInfo.GetPokemonMove2(pokemon))).ToList();
            var pokemonPairedWithStatsCpForUpgrade =
                highestsPokemonCpForUpgrade.Select(
                    pokemon =>
                        Tuple.Create(pokemon, _pokemonInfo.CalculateMaxCp(pokemon),
                            _pokemonInfo.CalculatePokemonPerfection(pokemon), _pokemonInfo.GetLevel(pokemon),
                            _pokemonInfo.GetPokemonMove1(pokemon), _pokemonInfo.GetPokemonMove2(pokemon))).ToList();
            var highestsPokemonPerfect =
                await _inventory.GetHighestsPerfect(_logicSettings.AmountOfPokemonToDisplayOnStart);

            var pokemonPairedWithStatsIv =
                highestsPokemonPerfect.Select(
                    pokemon =>
                        Tuple.Create(pokemon, _pokemonInfo.CalculateMaxCp(pokemon),
                            _pokemonInfo.CalculatePokemonPerfection(pokemon), _pokemonInfo.GetLevel(pokemon),
                            _pokemonInfo.GetPokemonMove1(pokemon), _pokemonInfo.GetPokemonMove2(pokemon))).ToList();
            var pokemonPairedWithStatsIvForUpgrade =
                highestsPokemonIvForUpgrade.Select(
                    pokemon =>
                        Tuple.Create(pokemon, _pokemonInfo.CalculateMaxCp(pokemon),
                            _pokemonInfo.CalculatePokemonPerfection(pokemon), _pokemonInfo.GetLevel(pokemon),
                            _pokemonInfo.GetPokemonMove1(pokemon), _pokemonInfo.GetPokemonMove2(pokemon))).ToList();

            _eventDispatcher.Send(
                new DisplayHighestsPokemonEvent
                {
                    SortedBy = "CP",
                    PokemonList = pokemonPairedWithStatsCp
                });
            if(_logicSettings.Teleport)
                await Task.Delay(_logicSettings.DelayDisplayPokemon);
            else
                await Task.Delay(500);

            _eventDispatcher.Send(
                new DisplayHighestsPokemonEvent
                {
                    SortedBy = "IV",
                    PokemonList = pokemonPairedWithStatsIv
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
            if(_logicSettings.Teleport)
                await Task.Delay(_logicSettings.DelayDisplayPokemon);
            else
                await Task.Delay(500);
        }
    }
}