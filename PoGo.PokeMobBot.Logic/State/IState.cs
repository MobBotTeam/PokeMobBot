#region using directives

using System.Threading;
using System.Threading.Tasks;

#endregion

namespace PoGo.PokeMobBot.Logic.State
{
    public interface IState
    {
        Task<IState> Execute(CancellationToken cancellationToken);
    }
}