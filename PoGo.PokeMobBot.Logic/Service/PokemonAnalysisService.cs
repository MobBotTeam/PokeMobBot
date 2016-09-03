using PoGo.PokeMobBot.Logic.PoGoUtils;
using POGOProtos.Data;

namespace PoGo.PokeMobBot.Logic.Service
{
    public class PokemonAnalysisService
    {
        private readonly PokemonInfo _pokemonInfo;

        public PokemonAnalysisService(PokemonInfo pokemonInfo)
        {
            _pokemonInfo = pokemonInfo;
        }

        public PokemonAnalysis GetPokemonAnalysis(PokemonData pokemon, int trainerLevel)
        {
            return new PokemonAnalysis
            {
                PokeData = pokemon,
                PerfectCp = _pokemonInfo.CalculateMaxCp(pokemon),
                MaximumPoweredCp = (int) _pokemonInfo.GetMaxCpAtTrainerLevel(pokemon, trainerLevel),
                Perfection = _pokemonInfo.CalculatePokemonPerfection(pokemon),
                Level = _pokemonInfo.GetLevel(pokemon),
                Move1 = _pokemonInfo.GetPokemonMove1(pokemon),
                Move2 = _pokemonInfo.GetPokemonMove2(pokemon),
                AverageRankVsTypes =
                    PokemonMoveInfo.GetPokemonMoveSet(PokemonMoveInfo.GetMoveSetCombinationIndex(pokemon.PokemonId,
                        _pokemonInfo.GetPokemonMove1(pokemon), _pokemonInfo.GetPokemonMove2(pokemon))) != null
                        ? PokemonMoveInfo.GetPokemonMoveSet(PokemonMoveInfo.GetMoveSetCombinationIndex(
                            pokemon.PokemonId, _pokemonInfo.GetPokemonMove1(pokemon),
                            _pokemonInfo.GetPokemonMove2(pokemon))).GetRankVsType("Average")
                        : 0,
            };
        }
    }
}
