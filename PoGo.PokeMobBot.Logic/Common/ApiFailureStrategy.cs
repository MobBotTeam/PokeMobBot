#region using directives

using System;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.State;
using PoGo.PokeMobBot.Logic.Tasks;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Networking.Envelopes;

#endregion

namespace PoGo.PokeMobBot.Logic.Common
{
    public class ApiFailureStrategy : IApiFailureStrategy
    {
        private readonly ISettings _settings;
        private readonly Login _login;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ITranslation _translation;

        private int _retryCount;

        public ApiFailureStrategy(ISettings settings, Login login, IEventDispatcher eventDispatcher, ITranslation translation)
        {
            _settings = settings;
            _login = login;
            _eventDispatcher = eventDispatcher;
            _translation = translation;
        }

        private async void DoLogin()
        {
            try
            {
                await _login.DoLogin();
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten().InnerException;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task<ApiOperation> HandleApiFailure(RequestEnvelope request, ResponseEnvelope response)
        {
            if (_retryCount == 11)
                return ApiOperation.Abort;

            await Task.Delay(500);
            _retryCount++;

            if (_retryCount % 5 == 0)
            {
                try
                {
                    DoLogin();
                }
                catch (PtcOfflineException)
                {
                    _eventDispatcher.Send(new ErrorEvent
                    {
                        Message = _translation.GetTranslation(TranslationString.PtcOffline)
                    });
                    _eventDispatcher.Send(new NoticeEvent
                    {
                        Message = _translation.GetTranslation(TranslationString.TryingAgainIn, 20)
                    });
                    await Task.Delay(20000);
                }
                catch (AccessTokenExpiredException)
                {
                    _eventDispatcher.Send(new ErrorEvent
                    {
                        Message = _translation.GetTranslation(TranslationString.AccessTokenExpired)
                    });
                    _eventDispatcher.Send(new NoticeEvent
                    {
                        Message = _translation.GetTranslation(TranslationString.TryingAgainIn, 2)
                    });
                    await Task.Delay(2000);
                }
                catch (Exception ex) when (ex is InvalidResponseException || ex is TaskCanceledException)
                {
                    _eventDispatcher.Send(new ErrorEvent
                    {
                        Message = _translation.GetTranslation(TranslationString.NianticServerUnstable)
                    });
                    await Task.Delay(1000);
                }
            }

            return ApiOperation.Retry;
        }

        public void HandleApiSuccess(RequestEnvelope request, ResponseEnvelope response)
        {
            _retryCount = 0;
        }
    }
}
