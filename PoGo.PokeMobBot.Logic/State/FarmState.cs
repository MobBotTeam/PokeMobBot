#region using directives

using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Tasks;

#endregion

namespace PoGo.PokeMobBot.Logic.State
{
    public class FarmState : IState
    {
        public async Task<IState> Execute(ISession session, CancellationToken cancellationToken)
        {
            if (!session.LogicSettings.OnlyFarmItems && (session.LogicSettings.EvolveAllPokemonAboveIv || session.LogicSettings.EvolveAllPokemonWithEnoughCandy))
            {
                await EvolvePokemonTask.Execute(session, cancellationToken);
            }

            if (!session.LogicSettings.OnlyFarmItems && session.LogicSettings.TransferDuplicatePokemon)
            {
                await TransferDuplicatePokemonTask.Execute(session, cancellationToken);
            }
            if (!session.LogicSettings.OnlyFarmItems && session.LogicSettings.AutomaticallyLevelUpPokemon)
            {
                await LevelUpPokemonTask.Execute(session, cancellationToken);
            }
            if (!session.LogicSettings.OnlyFarmItems && session.LogicSettings.RenamePokemon)
            {
                await RenamePokemonTask.Execute(session, cancellationToken);
            }

            await RecycleItemsTask.Execute(session, cancellationToken);

            if (!session.LogicSettings.OnlyFarmItems && session.LogicSettings.UseEggIncubators)
            {
                await UseIncubatorsTask.Execute(session, cancellationToken);
            }

            if (session.LogicSettings.UseGpxPathing)
            {
                await FarmPokestopsGpxTask.Execute(session, cancellationToken);
            }
            else
            {
                await FarmPokestopsTask.Execute(session, cancellationToken);
            }

            return this;
        }
    }
}