#region using directives

using System;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.State;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Rpc;

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

        public async Task<ApiOperation> HandleApiFailure()
        {
            if (_retryCount == 11)
                return ApiOperation.Abort;

            await Task.Delay(500);
            _retryCount++;

            if (_retryCount % 5 == 0)
            {
                DoLogin();
            }

            return ApiOperation.Retry;
        }

        public void HandleApiSuccess()
        {
            _retryCount = 0;
        }

        private async void DoLogin()
        {
            switch (_settings.AuthType)
            {
                case AuthType.Ptc:
                    try
                    {
                        await
                            _login.DoPtcLogin(_settings.PtcUsername, _settings.PtcPassword);
                    }
                    catch (AggregateException ae)
                    {
                        throw ae.Flatten().InnerException;
                    }
                    break;
                case AuthType.Google:
                    await
                        _login.DoGoogleLogin(_settings.GoogleUsername, _settings.GooglePassword);
                    break;
                default:
                    _eventDispatcher.Send(new ErrorEvent
                    {
                        Message = _translation.GetTranslation(TranslationString.WrongAuthType)
                    });
                    break;
            }
        }
    }
}