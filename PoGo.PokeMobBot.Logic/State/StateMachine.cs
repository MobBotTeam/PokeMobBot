#region using directives

using System;
using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PokemonGo.RocketAPI.Exceptions;

#endregion

namespace PoGo.PokeMobBot.Logic.State
{
    public class StateMachine
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ITranslation _translation;
        private IState _initialState;

        public StateMachine(IEventDispatcher eventDispatcher, ITranslation translation)
        {
            _eventDispatcher = eventDispatcher;
            _translation = translation;
        }

        public Task AsyncStart(IState initialState, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() => Start(initialState, cancellationToken), cancellationToken);
        }

        public void SetFailureState(IState state)
        {
            _initialState = state;
        }

        public async Task Start(IState initialState, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = initialState;
            do
            {
                try
                {
                    state = await state.Execute(cancellationToken);
                }
                catch (InvalidResponseException)
                {
                    _eventDispatcher.Send(new ErrorEvent
                    {
                        Message = _translation.GetTranslation(TranslationString.NianticServerUnstable)
                    });
                    state = _initialState;
                }
                catch (OperationCanceledException)
                {
                    _eventDispatcher.Send(new ErrorEvent { Message = _translation.GetTranslation(TranslationString.OperationCanceled) });
                    state = _initialState;
                }
                catch (Exception ex)
                {
                    _eventDispatcher.Send(new ErrorEvent { Message = ex.ToString() });
                    state = _initialState;
                }
            } while (state != null);
        }
    }
}