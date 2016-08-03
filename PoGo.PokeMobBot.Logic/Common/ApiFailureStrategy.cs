﻿#region using directives

using System;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.State;
using PokemonGo.RocketAPI.Common;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Extensions;

#endregion

namespace PoGo.PokeMobBot.Logic.Common
{
    public class ApiFailureStrategy : IApiFailureStrategy
    {
        private readonly ISession _session;
        private int _retryCount;

        public ApiFailureStrategy(ISession session)
        {
            _session = session;
        }

        public async Task<ApiOperation> HandleApiFailure()
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
                catch (Exception ex) when (ex is PtcOfflineException || ex is AccessTokenExpiredException)
                {
                    _session.EventDispatcher.Send(new ErrorEvent
                    {
                        Message = _session.Translation.GetTranslation(TranslationString.PtcOffline)
                    });
                    _session.EventDispatcher.Send(new NoticeEvent
                    {
                        Message = _session.Translation.GetTranslation(TranslationString.TryingAgainIn, 20)
                    });
                    await Task.Delay(20000);
                }
                catch (InvalidResponseException)
                {
                    _session.EventDispatcher.Send(new ErrorEvent
                    {
                        Message = _session.Translation.GetTranslation(TranslationString.NianticServerUnstable)
                    });
                    await Task.Delay(1000);
                }
            }

            return ApiOperation.Retry;
        }

        public void HandleApiSuccess()
        {
            _retryCount = 0;
        }

        private async void DoLogin()
        {
            switch (_session.Settings.AuthType)
            {
                case AuthType.Ptc:
                    try
                    {
                        await
                            _session.Client.Login.DoPtcLogin(_session.Settings.PtcUsername,
                                _session.Settings.PtcPassword);
                    }
                    catch (AggregateException ae)
                    {
                        throw ae.Flatten().InnerException;
                    }
                    break;
                case AuthType.Google:
                    await
                        _session.Client.Login.DoGoogleLogin(_session.Settings.GoogleUsername,
                            _session.Settings.GooglePassword);
                    break;
                default:
                    _session.EventDispatcher.Send(new ErrorEvent
                    {
                        Message = _session.Translation.GetTranslation(TranslationString.WrongAuthType)
                    });
                    break;
            }
        }
    }
}
