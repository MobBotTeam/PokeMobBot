#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public List<ulong> PokemonId = new List<ulong>();
        
        public List<ulong> PokemonIdcp = new List<ulong>();

        public DisplayPokemonStatsTask(Dumper dumper, PokemonInfo pokemonInfo)
        {
            _dumper = dumper;
            _pokemonInfo = pokemonInfo;
        }

        public async Task Execute(ISession session)
        {
            var highestsPokemonCp =
                await session.Inventory.GetHighestsCp(session.LogicSettings.AmountOfPokemonToDisplayOnStart);
            var highestsPokemonCpForUpgrade = await session.Inventory.GetHighestsCp(50);
            var highestsPokemonIvForUpgrade = await session.Inventory.GetHighestsPerfect(50);
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
                await session.Inventory.GetHighestsPerfect(session.LogicSettings.AmountOfPokemonToDisplayOnStart);

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

            session.EventDispatcher.Send(
                new DisplayHighestsPokemonEvent
                {
                    SortedBy = "CP",
                    PokemonList = pokemonPairedWithStatsCp
                });
            if(session.LogicSettings.Teleport)
                await Task.Delay(session.LogicSettings.DelayDisplayPokemon);
            else
                await Task.Delay(500);

            session.EventDispatcher.Send(
                new DisplayHighestsPokemonEvent
                {
                    SortedBy = "IV",
                    PokemonList = pokemonPairedWithStatsIv
                });

            foreach (var pokemon in pokemonPairedWithStatsIvForUpgrade)
            {
                var dgdfs = pokemon.ToString();

                var tokens = dgdfs.Split(new[] {"id"}, StringSplitOptions.None);
                var splitone = tokens[1].Split('"');
                var iv = session.Inventory.GetPerfect(pokemon.Item1);
                if (iv >= session.LogicSettings.UpgradePokemonIvMinimum)
                {
                    PokemonId.Add(ulong.Parse(splitone[2]));
                }
            }
            foreach (var t in pokemonPairedWithStatsCpForUpgrade)
            {
                var dgdfs = t.ToString();
                var tokens = dgdfs.Split(new[] {"id"}, StringSplitOptions.None);
                var splitone = tokens[1].Split('"');
                var tokensSplit = tokens[1].Split(new[] {"cp"}, StringSplitOptions.None);
                var tokenSplitAgain = tokensSplit[1].Split(' ');
                var tokenSplitAgain2 = tokenSplitAgain[1].Split(',');
                if (float.Parse(tokenSplitAgain2[0]) >= session.LogicSettings.UpgradePokemonCpMinimum)
                {
                    PokemonIdcp.Add(ulong.Parse(splitone[2]));
                }
            }
            var allPokemonInBag = session.LogicSettings.PrioritizeIvOverCp
                ? await session.Inventory.GetHighestsPerfect(1000)
                : await session.Inventory.GetHighestsCp(1000);
            if (session.LogicSettings.DumpPokemonStats)
            {
                const string dumpFileName = "PokeBagStats";
                _dumper.ClearDumpFile(session, dumpFileName);
                foreach (var pokemon in allPokemonInBag)
                {
                    _dumper.Dump(session,
                        $"NAME: {pokemon.PokemonId.ToString().PadRight(16, ' ')}Lvl: {_pokemonInfo.GetLevel(pokemon).ToString("00")}\t\tCP: {pokemon.Cp.ToString().PadRight(8, ' ')}\t\t IV: {_pokemonInfo.CalculatePokemonPerfection(pokemon).ToString("0.00")}%\t\t\tMOVE1: {pokemon.Move1}\t\t\tMOVE2: {pokemon.Move2}",
                        dumpFileName);
                }
            }
            if(session.LogicSettings.Teleport)
                await Task.Delay(session.LogicSettings.DelayDisplayPokemon);
            else
                await Task.Delay(500);
        }
    }
}