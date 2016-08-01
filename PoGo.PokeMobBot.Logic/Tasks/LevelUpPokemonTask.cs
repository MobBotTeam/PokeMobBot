#region using directives

using System;
using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Logging;
using PoGo.PokeMobBot.Logic.State;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class LevelUpPokemonTask
    {
        private readonly DisplayPokemonStatsTask _displayPokemonStatsTask;
        private readonly ILogger _logger;

        public LevelUpPokemonTask(DisplayPokemonStatsTask displayPokemonStatsTask, ILogger logger)
        {
            _displayPokemonStatsTask = displayPokemonStatsTask;
            _logger = logger;
        }

        public async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            if (_displayPokemonStatsTask.PokemonId.Count == 0 || _displayPokemonStatsTask.PokemonIdcp.Count == 0)
            {
                return;
            }
            if (session.LogicSettings.LevelUpByCPorIv.ToLower().Contains("iv"))
            {
                var rand = new Random();
                var randomNumber = rand.Next(0, _displayPokemonStatsTask.PokemonId.Count - 1);

                var upgradeResult =
                    await session.Inventory.UpgradePokemon(_displayPokemonStatsTask.PokemonId[randomNumber]);
                if (upgradeResult.Result.ToString().ToLower().Contains("success"))
                {
                    _logger.Write("Pokemon Upgraded:" + upgradeResult.UpgradedPokemon.PokemonId + ":" +
                                 upgradeResult.UpgradedPokemon.Cp);
                }
                else if (upgradeResult.Result.ToString().ToLower().Contains("insufficient"))
                {
                    _logger.Write("Pokemon Upgrade Failed Not Enough Resources");
                }
                else
                {
                    _logger.Write(
                        "Pokemon Upgrade Failed Unknown Error, Pokemon Could Be Max Level For Your Level The Pokemon That Caused Issue Was:" +
                        upgradeResult.UpgradedPokemon.PokemonId);
                }
            }
            else if (session.LogicSettings.LevelUpByCPorIv.ToLower().Contains("cp"))
            {
                var rand = new Random();
                var randomNumber = rand.Next(0, _displayPokemonStatsTask.PokemonIdcp.Count - 1);
                var upgradeResult =
                    await session.Inventory.UpgradePokemon(_displayPokemonStatsTask.PokemonIdcp[randomNumber]);
                if (upgradeResult.Result.ToString().ToLower().Contains("success"))
                {
                    _logger.Write("Pokemon Upgraded:" + upgradeResult.UpgradedPokemon.PokemonId + ":" +
                                 upgradeResult.UpgradedPokemon.Cp);
                }
                else if (upgradeResult.Result.ToString().ToLower().Contains("insufficient"))
                {
                    _logger.Write("Pokemon Upgrade Failed Not Enough Resources");
                }
                else
                {
                    _logger.Write(
                        "Pokemon Upgrade Failed Unknown Error, Pokemon Could Be Max Level For Your Level The Pokemon That Caused Issue Was:" +
                        upgradeResult.UpgradedPokemon.PokemonId);
                }
            }
        }
    }
}