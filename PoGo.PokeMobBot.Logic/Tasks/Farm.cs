#region using directives

using System.Threading;
using PoGo.PokeMobBot.Logic.State;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public interface IFarm
    {
        void Run(CancellationToken cancellationToken);
    }

    public class Farm : IFarm
    {
        private readonly ISession _session;
        private readonly EvolvePokemonTask _evolvePokemonTask;
        private readonly LevelUpPokemonTask _levelUpPokemonTask;
        private readonly TransferDuplicatePokemonTask _transferDuplicatePokemonTask;
        private readonly RenamePokemonTask _renamePokemonTask;
        private readonly RecycleItemsTask _recycleItemsTask;
        private readonly UseIncubatorsTask _useIncubatorsTask;
        private readonly FarmPokestopsGpxTask _farmPokestopsGpxTask;
        private readonly FarmPokestopsTask _farmPokestopsTask;

        public Farm(ISession session, EvolvePokemonTask evolvePokemonTask, LevelUpPokemonTask levelUpPokemonTask, TransferDuplicatePokemonTask transferDuplicatePokemonTask, RenamePokemonTask renamePokemonTask, RecycleItemsTask recycleItemsTask, UseIncubatorsTask useIncubatorsTask, FarmPokestopsGpxTask farmPokestopsGpxTask, FarmPokestopsTask farmPokestopsTask)
        {
            _session = session;
            _evolvePokemonTask = evolvePokemonTask;
            _levelUpPokemonTask = levelUpPokemonTask;
            _transferDuplicatePokemonTask = transferDuplicatePokemonTask;
            _renamePokemonTask = renamePokemonTask;
            _recycleItemsTask = recycleItemsTask;
            _useIncubatorsTask = useIncubatorsTask;
            _farmPokestopsGpxTask = farmPokestopsGpxTask;
            _farmPokestopsTask = farmPokestopsTask;
        }

        public void Run(CancellationToken cancellationToken)
        {
            if (_session.LogicSettings.EvolveAllPokemonAboveIv || _session.LogicSettings.EvolveAllPokemonWithEnoughCandy)
            {
                _evolvePokemonTask.Execute(_session, cancellationToken).Wait(cancellationToken);
            }
            if (_session.LogicSettings.AutomaticallyLevelUpPokemon)
            {
                _levelUpPokemonTask.Execute(_session, cancellationToken).Wait(cancellationToken);
            }
            if (_session.LogicSettings.TransferDuplicatePokemon)
            {
                _transferDuplicatePokemonTask.Execute(_session, cancellationToken).Wait(cancellationToken);
            }

            if (_session.LogicSettings.RenamePokemon)
            {
                _renamePokemonTask.Execute(_session, cancellationToken).Wait(cancellationToken);
            }

            _recycleItemsTask.Execute(_session, cancellationToken).Wait(cancellationToken);

            if (_session.LogicSettings.UseEggIncubators)
            {
                _useIncubatorsTask.Execute(_session, cancellationToken).Wait(cancellationToken);
            }

            if (_session.LogicSettings.UseGpxPathing)
            {
                _farmPokestopsGpxTask.Execute(_session, cancellationToken).Wait(cancellationToken);
            }
            else
            {
                _farmPokestopsTask.Execute(_session, cancellationToken).Wait(cancellationToken);
            }
        }
    }
}