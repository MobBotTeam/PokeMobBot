#region using directives

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;

#endregion

namespace PoGo.PokeMobBot.Logic.State
{
    public class LoginState : IState
    {
        private readonly PositionCheckState _positionCheckState;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ITranslation _translation;
        private readonly ISettings _settings;
        private readonly PokemonGo.RocketAPI.Rpc.Login _login;
        private readonly Client _client;

        public LoginState(PositionCheckState positionCheckState, IEventDispatcher eventDispatcher, ITranslation translation, ISettings settings, PokemonGo.RocketAPI.Rpc.Login login, Client client)
        {
            _positionCheckState = positionCheckState;
            _eventDispatcher = eventDispatcher;
            _translation = translation;
            _settings = settings;
            _login = login;
            _client = client;
        }

        public async Task<IState> Execute(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _eventDispatcher.Send(new NoticeEvent
            {
                Message = _translation.GetTranslation(TranslationString.LoggingIn, _settings.AuthType)
            });

            await CheckLogin(cancellationToken);

            try
            {
                await _client.Login.DoLogin();
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
                await Task.Delay(20000, cancellationToken);
                return this;
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
                await Task.Delay(2000, cancellationToken);
                return this;
            }
            catch (InvalidResponseException)
            {
                _eventDispatcher.Send(new ErrorEvent
                {
                    Message = _translation.GetTranslation(TranslationString.NianticServerUnstable)
                });
                return this;
            }
            catch (AccountNotVerifiedException)
            {
                _eventDispatcher.Send(new ErrorEvent
                {
                    Message = _translation.GetTranslation(TranslationString.AccountNotVerified)
                });
                await Task.Delay(2000, cancellationToken);
                Environment.Exit(0);
            }
            catch (GoogleException e)
            {
                if (e.Message.Contains("NeedsBrowser"))
                {
                    _eventDispatcher.Send(new ErrorEvent
                    {
                        Message = _translation.GetTranslation(TranslationString.GoogleTwoFactorAuth)
                    });
                    _eventDispatcher.Send(new ErrorEvent
                    {
                        Message = _translation.GetTranslation(TranslationString.GoogleTwoFactorAuthExplanation)
                    });
                    await Task.Delay(7000, cancellationToken);
                    try
                    {
                        Process.Start("https://security.google.com/settings/security/apppasswords");
                    }
                    catch (Exception)
                    {
                        _eventDispatcher.Send(new ErrorEvent
                        {
                            Message = "https://security.google.com/settings/security/apppasswords"
                        });
                        throw;
                    }
                }
                _eventDispatcher.Send(new ErrorEvent
                {
                    Message = _translation.GetTranslation(TranslationString.GoogleError)
                });
                await Task.Delay(2000, cancellationToken);
                Environment.Exit(0);
            }

            await DownloadProfile();

            return _positionCheckState;
        }

        private async Task CheckLogin(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_settings.AuthType == AuthType.Google && (_settings.GoogleUsername == null || _settings.GooglePassword == null))
            {
                _eventDispatcher.Send(new ErrorEvent
                {
                    Message = _translation.GetTranslation(TranslationString.MissingCredentialsGoogle)
                });
                await Task.Delay(2000, cancellationToken);
                Environment.Exit(0);
            }
            else if (_settings.AuthType == AuthType.Ptc && (_settings.PtcUsername == null || _settings.PtcPassword == null))
            {
                _eventDispatcher.Send(new ErrorEvent
                {
                    Message = _translation.GetTranslation(TranslationString.MissingCredentialsPtc)
                });
                await Task.Delay(2000, cancellationToken);
                Environment.Exit(0);
            }
        }

        public async Task DownloadProfile()
        {
            _eventDispatcher.Send(new ProfileEvent { Profile = await _client.Player.GetPlayer() });
        }
    }
}
