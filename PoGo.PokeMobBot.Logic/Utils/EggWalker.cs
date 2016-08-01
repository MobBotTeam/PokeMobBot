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
        private readonly ISession _session;
        private readonly UseIncubatorsTask _useIncubatorsTask;

        private double _distanceTraveled;

        public EggWalker(ISession session, UseIncubatorsTask useIncubatorsTask)
        {
            _session = session;
            _useIncubatorsTask = useIncubatorsTask;
        }

        public async Task ApplyDistance(double distanceTraveled, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_session.LogicSettings.UseEggIncubators)
                return;

            _distanceTraveled += distanceTraveled;
            if (_distanceTraveled > _checkInterval)
            {
                await _useIncubatorsTask.Execute(_session, cancellationToken);
                _distanceTraveled = 0;
            }
        }
    }
}