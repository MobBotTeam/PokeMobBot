#region using directives

using System;
using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Logging;
using PoGo.PokeMobBot.Logic.State;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    internal class LevelUpPokemonTask
    {
        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            if (DisplayPokemonStatsTask.PokemonId.Count == 0 || DisplayPokemonStatsTask.PokemonIdcp.Count == 0)
            {
                return;
            }
            if (session.LogicSettings.LevelUpByCPorIv.ToLower().Contains("iv"))
            {
                var rand = new Random();
                var randomNumber = rand.Next(0, DisplayPokemonStatsTask.PokemonId.Count - 1);

                var upgradeResult =
                    await session.Inventory.UpgradePokemon(DisplayPokemonStatsTask.PokemonId[randomNumber]);
                if (upgradeResult.Result.ToString().ToLower().Contains("success"))
                {
                    Logger.Write("Pokemon Upgraded:" + upgradeResult.UpgradedPokemon.PokemonId + ":" +
                                 upgradeResult.UpgradedPokemon.Cp);
                }
                else if (upgradeResult.Result.ToString().ToLower().Contains("insufficient"))
                {
                    Logger.Write("Pokemon Upgrade Failed Not Enough Resources");
                }
                else if (upgradeResult.UpgradedPokemon != null)
                {
                    Logger.Write(
                        "Pokemon Upgrade Failed Unknown Error, Pokemon Could Be Max Level For Your Level The Pokemon That Caused Issue Was:" +
                        upgradeResult.UpgradedPokemon.PokemonId);
                }
                else
                {
                    Logger.Write(
                        "Pokemon Upgrade Faild due to unknown 'System.NullReferenceException'");
                }
            }

            else if (session.LogicSettings.LevelUpByCPorIv.ToLower().Contains("cp"))
            {
                var rand = new Random();
                var randomNumber = rand.Next(0, DisplayPokemonStatsTask.PokemonIdcp.Count - 1);
                var upgradeResult =
                    await session.Inventory.UpgradePokemon(DisplayPokemonStatsTask.PokemonIdcp[randomNumber]);
                if (upgradeResult.Result.ToString().ToLower().Contains("success"))
                {
                    Logger.Write("Pokemon Upgraded:" + upgradeResult.UpgradedPokemon.PokemonId + ":" +
                                 upgradeResult.UpgradedPokemon.Cp);
                }
                else if (upgradeResult.Result.ToString().ToLower().Contains("insufficient"))
                {
                    Logger.Write("Pokemon Upgrade Failed Not Enough Resources");
                }
                else
                {
                    Logger.Write(
                        "Pokemon Upgrade Failed Unknown Error, Pokemon Could Be Max Level For Your Level The Pokemon That Caused Issue Was:" +
                        upgradeResult.UpgradedPokemon.PokemonId);
                }
            }
        }
    }
}