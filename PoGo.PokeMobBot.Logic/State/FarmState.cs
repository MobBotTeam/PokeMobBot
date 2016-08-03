#region using directives

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Tasks;
using PokemonGo.RocketAPI.Exceptions;

#endregion

namespace PoGo.PokeMobBot.Logic.State
{
    public class FarmState : IState
    {
        private readonly EvolvePokemonTask _evolvePokemonTask;
        private readonly TransferDuplicatePokemonTask _transferDuplicatePokemonTask;
        private readonly LevelUpPokemonTask _levelUpPokemonTask;
        private readonly RenamePokemonTask _renamePokemonTask;
        private readonly RecycleItemsTask _recycleItemsTask;
        private readonly UseIncubatorsTask _useIncubatorsTask;
        private readonly FarmPokestopsGpxTask _farmPokestopsGpxTask;
        private readonly FarmPokestopsTask _farmPokestopsTask;
        private readonly ILogicSettings _logicSettings;
        private readonly TransferLowStatPokemonTask _lowStatPokemonTask;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ITranslation _translation;

        public FarmState(EvolvePokemonTask evolvePokemonTask, TransferDuplicatePokemonTask transferDuplicatePokemonTask, LevelUpPokemonTask levelUpPokemonTask, RenamePokemonTask renamePokemonTask, RecycleItemsTask recycleItemsTask, UseIncubatorsTask useIncubatorsTask, FarmPokestopsGpxTask farmPokestopsGpxTask, FarmPokestopsTask farmPokestopsTask, ILogicSettings logicSettings, TransferLowStatPokemonTask lowStatPokemonTask, IEventDispatcher eventDispatcher, ITranslation translation)
        {
            _evolvePokemonTask = evolvePokemonTask;
            _transferDuplicatePokemonTask = transferDuplicatePokemonTask;
            _levelUpPokemonTask = levelUpPokemonTask;
            _renamePokemonTask = renamePokemonTask;
            _recycleItemsTask = recycleItemsTask;
            _useIncubatorsTask = useIncubatorsTask;
            _farmPokestopsGpxTask = farmPokestopsGpxTask;
            _farmPokestopsTask = farmPokestopsTask;
            _logicSettings = logicSettings;
            _lowStatPokemonTask = lowStatPokemonTask;
            _eventDispatcher = eventDispatcher;
            _translation = translation;
        }

        public async Task<IState> Execute(CancellationToken cancellationToken)
        {
            try
            {
                if (_logicSettings.EvolveAllPokemonAboveIv || _logicSettings.EvolveAllPokemonWithEnoughCandy)
                {
                    await _evolvePokemonTask.Execute(cancellationToken);
                }

                if (_logicSettings.TransferDuplicatePokemon)
                {
                    await _transferDuplicatePokemonTask.Execute(cancellationToken);
                }
                if (_logicSettings.TransferLowStatPokemon)
                {
                    await _lowStatPokemonTask.Execute(cancellationToken);
                }
                if (_logicSettings.AutomaticallyLevelUpPokemon)
                {
                    await _levelUpPokemonTask.Execute(cancellationToken);
                }
                if (_logicSettings.RenamePokemon)
                {
                    await _renamePokemonTask.Execute(cancellationToken);
                }

                await _recycleItemsTask.Execute(cancellationToken);

                if (_logicSettings.UseEggIncubators)
                {
                    await _useIncubatorsTask.Execute(cancellationToken);
                }

                if (_logicSettings.UseGpxPathing)
                {
                    await _farmPokestopsGpxTask.Execute(cancellationToken);
                }
                else
                {
                    await _farmPokestopsTask.Execute(cancellationToken);
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
                await Task.Delay(20000, cancellationToken);
                throw;
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
                throw;
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

            return this;
        }
    }
}