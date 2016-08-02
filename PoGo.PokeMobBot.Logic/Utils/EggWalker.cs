#region using directives

using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.State;
using PoGo.PokeMobBot.Logic.Tasks;

#endregion

namespace PoGo.PokeMobBot.Logic.Utils
{
    public class EggWalker
    {
        private readonly double _checkInterval = 1000; // TODO: check for real value
        private readonly UseIncubatorsTask _useIncubatorsTask;
        private readonly ILogicSettings _logicSettings;

        private double _distanceTraveled;

        public EggWalker(UseIncubatorsTask useIncubatorsTask, ILogicSettings logicSettings)
        {
            _useIncubatorsTask = useIncubatorsTask;
            _logicSettings = logicSettings;
        }

        public async Task ApplyDistance(double distanceTraveled, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_logicSettings.UseEggIncubators)
                return;

            _distanceTraveled += distanceTraveled;
            if (_distanceTraveled > _checkInterval)
            {
                await _useIncubatorsTask.Execute(cancellationToken);
                _distanceTraveled = 0;
            }
        }
    }
}