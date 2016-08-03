#region using directives

using System.Threading;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public interface IFarm
    {
        void Run(CancellationToken cancellationToken);
    }

    public class Farm : IFarm
    {
        private readonly EvolvePokemonTask _evolvePokemonTask;
        private readonly LevelUpPokemonTask _levelUpPokemonTask;
        private readonly TransferDuplicatePokemonTask _transferDuplicatePokemonTask;
        private readonly RenamePokemonTask _renamePokemonTask;
        private readonly RecycleItemsTask _recycleItemsTask;
        private readonly UseIncubatorsTask _useIncubatorsTask;
        private readonly FarmPokestopsGpxTask _farmPokestopsGpxTask;
        private readonly FarmPokestopsTask _farmPokestopsTask;
        private readonly ILogicSettings _logicSettings;
        private readonly TransferLowStatPokemonTask _transferLowStatPokemonTask;

        public Farm(EvolvePokemonTask evolvePokemonTask, LevelUpPokemonTask levelUpPokemonTask, TransferDuplicatePokemonTask transferDuplicatePokemonTask, RenamePokemonTask renamePokemonTask, RecycleItemsTask recycleItemsTask, UseIncubatorsTask useIncubatorsTask, FarmPokestopsGpxTask farmPokestopsGpxTask, FarmPokestopsTask farmPokestopsTask, ILogicSettings logicSettings, TransferLowStatPokemonTask transferLowStatPokemonTask)
        {
            _evolvePokemonTask = evolvePokemonTask;
            _levelUpPokemonTask = levelUpPokemonTask;
            _transferDuplicatePokemonTask = transferDuplicatePokemonTask;
            _renamePokemonTask = renamePokemonTask;
            _recycleItemsTask = recycleItemsTask;
            _useIncubatorsTask = useIncubatorsTask;
            _farmPokestopsGpxTask = farmPokestopsGpxTask;
            _farmPokestopsTask = farmPokestopsTask;
            _logicSettings = logicSettings;
            _transferLowStatPokemonTask = transferLowStatPokemonTask;
        }

        public void Run(CancellationToken cancellationToken)
        {
            if (_logicSettings.EvolveAllPokemonAboveIv || _logicSettings.EvolveAllPokemonWithEnoughCandy)
            {
                _evolvePokemonTask.Execute(cancellationToken).Wait(cancellationToken);
            }
            if (_logicSettings.AutomaticallyLevelUpPokemon)
            {
                _levelUpPokemonTask.Execute(cancellationToken).Wait(cancellationToken);
            }
            if (_logicSettings.TransferDuplicatePokemon)
            {
                _transferDuplicatePokemonTask.Execute(cancellationToken).Wait(cancellationToken);
            }
            if (_logicSettings.TransferLowStatPokemon)
            {
                _transferLowStatPokemonTask.Execute(cancellationToken).Wait(cancellationToken);
            }

            if (_logicSettings.RenamePokemon)
            {
                _renamePokemonTask.Execute(cancellationToken).Wait(cancellationToken);
            }

            _recycleItemsTask.Execute(cancellationToken).Wait(cancellationToken);

            if (_logicSettings.UseEggIncubators)
            {
                _useIncubatorsTask.Execute(cancellationToken).Wait(cancellationToken);
            }

            if (_logicSettings.UseGpxPathing)
            {
                _farmPokestopsGpxTask.Execute(cancellationToken).Wait(cancellationToken);
            }
            else
            {
                _farmPokestopsTask.Execute(cancellationToken).Wait(cancellationToken);
            }
        }
    }
}