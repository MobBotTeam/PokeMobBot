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

        public InfoState(DisplayPokemonStatsTask displayPokemonStatsTask, FarmState farmState)
        {
            _displayPokemonStatsTask = displayPokemonStatsTask;
            _farmState = farmState;
        }

        public async Task<IState> Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (session.LogicSettings.AmountOfPokemonToDisplayOnStart > 0)
                await _displayPokemonStatsTask.Execute(session);

            return _farmState;
        }
    }
}