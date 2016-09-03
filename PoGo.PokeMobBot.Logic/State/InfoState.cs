#region using directives

using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Tasks;

#endregion

namespace PoGo.PokeMobBot.Logic.State
{
    public class InfoState : IState
    {
        private readonly DisplayPokemonStatsTask _displayPokemonStatsTask;
        private readonly FarmState _farmState;
        private readonly ILogicSettings _logicSettings;

        public InfoState(DisplayPokemonStatsTask displayPokemonStatsTask, FarmState farmState, ILogicSettings logicSettings)
        {
            _displayPokemonStatsTask = displayPokemonStatsTask;
            _farmState = farmState;
            _logicSettings = logicSettings;
        }

        public async Task<IState> Execute(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_logicSettings.AmountOfPokemonToDisplayOnStart > 0)
                await _displayPokemonStatsTask.Execute();

            return _farmState;
        }
    }
}