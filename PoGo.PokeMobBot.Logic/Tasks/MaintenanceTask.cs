using PoGo.PokeMobBot.Logic.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public class MaintenanceTask
    {
        public static async Task Execute(ISession session, CancellationToken cancellationToken)
        {
            if (RuntimeSettings.StopsHit %5 == 0) //TODO: OR item/pokemon bag is full
            {
                RuntimeSettings.StopsHit = 0;
                // need updated stardust information for upgrading, so refresh your profile now
                await DownloadProfile(session);

                await RecycleItemsTask.Execute(session, cancellationToken);
                if (session.LogicSettings.EvolveAllPokemonWithEnoughCandy ||
                    session.LogicSettings.EvolveAllPokemonAboveIv)
                {
                    await EvolvePokemonTask.Execute(session, cancellationToken);
                }
                if (session.LogicSettings.AutomaticallyLevelUpPokemon)
                {
                    await LevelUpPokemonTask.Execute(session, cancellationToken);
                }
                if (session.LogicSettings.TransferDuplicatePokemon)
                {
                    await TransferDuplicatePokemonTask.Execute(session, cancellationToken);
                }
                if (session.LogicSettings.RenamePokemon)
                {
                    await RenamePokemonTask.Execute(session, cancellationToken);
                }
                //Do we need this?
                //await DisplayPokemonStatsTask.Execute(session);

            }
        }
        private static async Task DownloadProfile(ISession session)
        {
            session.Profile = await session.Client.Player.GetPlayer();
        }
    }


}
