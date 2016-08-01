#region using directives

using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Tasks;

#endregion

namespace PoGo.PokeMobBot.Logic.State
{
    public class FarmState : IState
    {
        private readonly EvolvePokemonTask _evolvePokemonTask;
        private readonly TransferDuplicatePokemonTask _transferDuplicatePokemonTask;
        private readonly LevelUpPokemonTask _levelUpPokemonTask;
        private readonly RenamePokemonTask _renamePokemonTask;
        private readonly RecycleItemsTask _recycleItemsTask;
        private readonly UseIncubatorsTask _useIncubatorsTask;
        private readonly FarmPokestopsGpxTask _farmPokestopsGpxTask;
        private readonly FarmPokestopsTask _farmPokestopsTask;

        public FarmState(EvolvePokemonTask evolvePokemonTask, TransferDuplicatePokemonTask transferDuplicatePokemonTask, LevelUpPokemonTask levelUpPokemonTask, RenamePokemonTask renamePokemonTask, RecycleItemsTask recycleItemsTask, UseIncubatorsTask useIncubatorsTask, FarmPokestopsGpxTask farmPokestopsGpxTask, FarmPokestopsTask farmPokestopsTask)
        {
            _evolvePokemonTask = evolvePokemonTask;
            _transferDuplicatePokemonTask = transferDuplicatePokemonTask;
            _levelUpPokemonTask = levelUpPokemonTask;
            _renamePokemonTask = renamePokemonTask;
            _recycleItemsTask = recycleItemsTask;
            _useIncubatorsTask = useIncubatorsTask;
            _farmPokestopsGpxTask = farmPokestopsGpxTask;
            _farmPokestopsTask = farmPokestopsTask;
        }

        public async Task<IState> Execute(ISession session, CancellationToken cancellationToken)
        {
            if (session.LogicSettings.EvolveAllPokemonAboveIv || session.LogicSettings.EvolveAllPokemonWithEnoughCandy)
            {
                await _evolvePokemonTask.Execute(session, cancellationToken);
            }

            if (session.LogicSettings.TransferDuplicatePokemon)
            {
                await _transferDuplicatePokemonTask.Execute(session, cancellationToken);
            }
            if (session.LogicSettings.AutomaticallyLevelUpPokemon)
            {
                await _levelUpPokemonTask.Execute(session, cancellationToken);
            }
            if (session.LogicSettings.RenamePokemon)
            {
                await _renamePokemonTask.Execute(session, cancellationToken);
            }

            await _recycleItemsTask.Execute(session, cancellationToken);

            if (session.LogicSettings.UseEggIncubators)
            {
                await _useIncubatorsTask.Execute(session, cancellationToken);
            }

            if (session.LogicSettings.UseGpxPathing)
            {
                await _farmPokestopsGpxTask.Execute(session, cancellationToken);
            }
            else
            {
                await _farmPokestopsTask.Execute(session, cancellationToken);
            }

            return this;
        }
    }
}