#region using directives

using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Tasks;

#endregion

namespace PoGo.PokeMobBot.Logic.State
{
    public class InfoState : IState
    {
        public async Task<IState> Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (session.LogicSettings.AmountOfPokemonToDisplayOnStart > 0)
                await DisplayPokemonStatsTask.Execute(session);
            if(session.LogicSettings.Teleport)
                await session.Client.Player.UpdatePlayerLocation(session.Settings.DefaultLatitude, session.Settings.DefaultLongitude,
                            session.Client.Settings.DefaultAltitude);
            return new FarmState();
        }
    }
}