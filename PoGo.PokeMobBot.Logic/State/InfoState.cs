#region using directives

using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Tasks;
using System;

#endregion

namespace PoGo.PokeMobBot.Logic.State
{
    public class InfoState : IState
    {
        public async Task<IState> Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            //Routing.GetRoute(new GeoCoordinatePortable.GeoCoordinate(35.6895, 139.6917), new GeoCoordinatePortable.GeoCoordinate(35.9000, 139.9000));
            //Console.ReadLine();

            //if (session.LogicSettings.AmountOfPokemonToDisplayOnStart > 0)
            if ((session.LogicSettings.AmountOfPokemonToDisplayOnStartCp + session.LogicSettings.AmountOfPokemonToDisplayOnStartIv) > 0)
                await DisplayPokemonStatsTask.Execute(session);
            await session.MapCache.UpdateMapDatas(session);
            return new FarmState();
        }
    }
}