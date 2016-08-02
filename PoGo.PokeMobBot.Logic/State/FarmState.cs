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
        private readonly ILogicSettings _logicSettings;

        public FarmState(EvolvePokemonTask evolvePokemonTask, TransferDuplicatePokemonTask transferDuplicatePokemonTask, LevelUpPokemonTask levelUpPokemonTask, RenamePokemonTask renamePokemonTask, RecycleItemsTask recycleItemsTask, UseIncubatorsTask useIncubatorsTask, FarmPokestopsGpxTask farmPokestopsGpxTask, FarmPokestopsTask farmPokestopsTask, ILogicSettings logicSettings)
        {
            _evolvePokemonTask = evolvePokemonTask;
            _transferDuplicatePokemonTask = transferDuplicatePokemonTask;
            _levelUpPokemonTask = levelUpPokemonTask;
            _renamePokemonTask = renamePokemonTask;
            _recycleItemsTask = recycleItemsTask;
            _useIncubatorsTask = useIncubatorsTask;
            _farmPokestopsGpxTask = farmPokestopsGpxTask;
            _farmPokestopsTask = farmPokestopsTask;
            _logicSettings = logicSettings;
        }

        public async Task<IState> Execute(CancellationToken cancellationToken)
        {
            if (_logicSettings.EvolveAllPokemonAboveIv || _logicSettings.EvolveAllPokemonWithEnoughCandy)
            {
                await _evolvePokemonTask.Execute(cancellationToken);
            }

            if (_logicSettings.TransferDuplicatePokemon)
            {
                await _transferDuplicatePokemonTask.Execute(cancellationToken);
            }
            if (_logicSettings.AutomaticallyLevelUpPokemon)
            {
                await _levelUpPokemonTask.Execute(cancellationToken);
            }
            if (_logicSettings.RenamePokemon)
            {
                await _renamePokemonTask.Execute(cancellationToken);
            }

            await _recycleItemsTask.Execute(cancellationToken);

            if (_logicSettings.UseEggIncubators)
            {
                await _useIncubatorsTask.Execute(cancellationToken);
            }

            if (_logicSettings.UseGpxPathing)
            {
                await _farmPokestopsGpxTask.Execute(cancellationToken);
            }
            else
            {
                await _farmPokestopsTask.Execute( cancellationToken);
            }

            return this;
        }
    }
}