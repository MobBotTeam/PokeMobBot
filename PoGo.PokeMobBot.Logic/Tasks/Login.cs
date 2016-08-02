#region using directives

using System;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;

#endregion

namespace PoGo.PokeMobBot.Logic.Tasks
{
    public interface ILogin
    {
        void DoLogin();
    }

    public class Login : ILogin
    {
        private readonly Client _client;
        private readonly ISettings _settings;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ITranslation _translation;

        public Login(Client client, ISettings settings, IEventDispatcher eventDispatcher, ITranslation translation)
        {
            _client = client;
            _settings = settings;
            _eventDispatcher = eventDispatcher;
            _translation = translation;
        }

        public void DoLogin()
        {
            try
            {
                if (_client.AuthType == AuthType.Ptc)
                {
                    try
                    {
                        _client.Login.DoPtcLogin(_settings.PtcUsername, _settings.PtcPassword)
                            .Wait();
                    }
                    catch (AggregateException ae)
                    {
                        throw ae.Flatten().InnerException;
                    }
                }
                else
                {
                    _client.Login.DoGoogleLogin(_settings.GoogleUsername,
                        _settings.GooglePassword).Wait();
                }
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
            }
            catch (AccountNotVerifiedException)
            {
                _eventDispatcher.Send(new ErrorEvent
                {
                    Message = _translation.GetTranslation(TranslationString.AccountNotVerified)
                });
            }
        }
    }
}