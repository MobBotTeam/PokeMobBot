#region using directives

using System;
using System.Globalization;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Logging;
using PoGo.PokeMobBot.Logic.State;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;

#endregion

namespace PoGo.PokeMobBot.CLI
{
    public class ConsoleEventListener
    {
        private readonly ILogger _logger;
        private readonly ITranslation _translation;

        public ConsoleEventListener(ILogger logger, ITranslation translation)
        {
            _logger = logger;
            _translation = translation;
        }

        public void HandleEvent(ProfileEvent evt)
        {
            _logger.Write(_translation.GetTranslation(TranslationString.EventProfileLogin,
                evt.Profile.PlayerData.Username ?? ""));
        }

        public void HandleEvent(ErrorEvent evt)
        {
            _logger.Write(evt.ToString(), LogLevel.Error);
        }

        public void HandleEvent(NoticeEvent evt)
        {
            _logger.Write(evt.ToString());
        }

        public void HandleEvent(DebugEvent evt)
        {
            _logger.Write(evt.ToString(), LogLevel.Debug);
        }

        public void HandleEvent(WarnEvent evt)
        {
            _logger.Write(evt.ToString(), LogLevel.Warning);

            if (evt.RequireInput)
            {
                _logger.Write(_translation.GetTranslation(TranslationString.RequireInputText));
                Console.ReadKey();
            }
        }

        public void HandleEvent(UseLuckyEggEvent evt)
        {
            _logger.Write(_translation.GetTranslation(TranslationString.EventUsedLuckyEgg, evt.Count),
                LogLevel.Egg);
        }

        public void HandleEvent(UseLuckyEggMinPokemonEvent evt)
        {
            _logger.Write(_translation.GetTranslation(TranslationString.EventUseLuckyEggMinPokemonCheck, evt.Diff, evt.CurrCount, evt.MinPokemon), LogLevel.Info);
        }

        public void HandleEvent(PokemonEvolveEvent evt)
        {
            _logger.Write(evt.Result == EvolvePokemonResponse.Types.Result.Success
                ? _translation.GetTranslation(TranslationString.EventPokemonEvolvedSuccess, _translation.GetPokemonName(evt.Id), evt.Exp)
                : _translation.GetTranslation(TranslationString.EventPokemonEvolvedFailed, _translation.GetPokemonName(evt.Id), evt.Result,
                    evt.Id),
                LogLevel.Evolve);
        }

        public void HandleEvent(TransferPokemonEvent evt)
        {
            _logger.Write(
                _translation.GetTranslation(TranslationString.EventPokemonTransferred, _translation.GetPokemonName(evt.Id), evt.Cp,
                    evt.Perfection.ToString("0.00"), evt.BestCp, evt.BestPerfection.ToString("0.00"), evt.FamilyCandies),
                LogLevel.Transfer);
        }

        public void HandleEvent(ItemRecycledEvent evt)
        {
            _logger.Write(_translation.GetTranslation(TranslationString.EventItemRecycled, evt.Count, evt.Id),
                LogLevel.Recycling);
        }

        public void HandleEvent(EggIncubatorStatusEvent evt)
        {
            _logger.Write(evt.WasAddedNow
                ? _translation.GetTranslation(TranslationString.IncubatorPuttingEgg, evt.KmRemaining)
                : _translation.GetTranslation(TranslationString.IncubatorStatusUpdate, evt.KmRemaining),
                LogLevel.Egg);
        }

        public void HandleEvent(EggHatchedEvent evt)
        {
            _logger.Write(_translation.GetTranslation(TranslationString.IncubatorEggHatched,
                _translation.GetPokemonName(evt.PokemonId), evt.Level, evt.Cp, evt.MaxCp, evt.Perfection),
                LogLevel.Egg);
        }

        public void HandleEvent(FortUsedEvent evt)
        {
            var itemString = evt.InventoryFull
                ? _translation.GetTranslation(TranslationString.InvFullPokestopLooting)
                : evt.Items;
            _logger.Write(
                _translation.GetTranslation(TranslationString.EventFortUsed, evt.Name, evt.Exp, evt.Gems,
                    itemString),
                LogLevel.Pokestop);
        }

        public void HandleEvent(FortFailedEvent evt)
        {
            _logger.Write(
                _translation.GetTranslation(TranslationString.EventFortFailed, evt.Name, evt.Try, evt.Max),
                LogLevel.Pokestop, ConsoleColor.DarkRed);
        }

        public void HandleEvent(FortTargetEvent evt)
        {
            _logger.Write(
                _translation.GetTranslation(TranslationString.EventFortTargeted, evt.Name,
                    Math.Round(evt.Distance)),
                LogLevel.Info, ConsoleColor.DarkRed);
        }

        public void HandleEvent(PokemonCaptureEvent evt)
        {
            Func<ItemId, string> returnRealBallName = a =>
            {
                switch (a)
                {
                    case ItemId.ItemPokeBall:
                        return _translation.GetTranslation(TranslationString.Pokeball);
                    case ItemId.ItemGreatBall:
                        return _translation.GetTranslation(TranslationString.GreatPokeball);
                    case ItemId.ItemUltraBall:
                        return _translation.GetTranslation(TranslationString.UltraPokeball);
                    case ItemId.ItemMasterBall:
                        return _translation.GetTranslation(TranslationString.MasterPokeball);
                    default:
                        return _translation.GetTranslation(TranslationString.CommonWordUnknown);
                }
            };

            var catchType = evt.CatchType;

            string strStatus;
            switch (evt.Status)
            {
                case CatchPokemonResponse.Types.CatchStatus.CatchError:
                    strStatus = _translation.GetTranslation(TranslationString.CatchStatusError);
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchEscape:
                    strStatus = _translation.GetTranslation(TranslationString.CatchStatusEscape);
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchFlee:
                    strStatus = _translation.GetTranslation(TranslationString.CatchStatusFlee);
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchMissed:
                    strStatus = _translation.GetTranslation(TranslationString.CatchStatusMissed);
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchSuccess:
                    strStatus = _translation.GetTranslation(TranslationString.CatchStatusSuccess);
                    break;
                default:
                    strStatus = evt.Status.ToString();
                    break;
            }

            var catchStatus = evt.Attempt > 1
                ? _translation.GetTranslation(TranslationString.CatchStatusAttempt, strStatus, evt.Attempt)
                : _translation.GetTranslation(TranslationString.CatchStatus, strStatus);

            var familyCandies = evt.FamilyCandies > 0
                ? _translation.GetTranslation(TranslationString.Candies, evt.FamilyCandies)
                : "";

            _logger.Write(
                _translation.GetTranslation(TranslationString.EventPokemonCapture, catchStatus, catchType, _translation.GetPokemonName(evt.Id),
                    evt.Level, evt.Cp, evt.MaxCp, evt.Perfection.ToString("0.00"), evt.Probability,
                    evt.Distance.ToString("F2"),
                    returnRealBallName(evt.Pokeball), evt.BallAmount, familyCandies), LogLevel.Caught);
        }

        public void HandleEvent(NoPokeballEvent evt)
        {
            _logger.Write(_translation.GetTranslation(TranslationString.EventNoPokeballs, _translation.GetPokemonName(evt.Id), evt.Cp),
                LogLevel.Caught);
        }

        public void HandleEvent(UseBerryEvent evt)
        {
            _logger.Write(_translation.GetTranslation(TranslationString.UseBerry, evt.Count),
                LogLevel.Berry);
        }

        public void HandleEvent(SnipeScanEvent evt)
        {
            _logger.Write(evt.PokemonId == PokemonId.Missingno
                ? _translation.GetTranslation(TranslationString.SnipeScan,
                    $"{evt.Bounds.Latitude},{evt.Bounds.Longitude}")
                : _translation.GetTranslation(TranslationString.SnipeScanEx, _translation.GetPokemonName(evt.PokemonId),
                    evt.Iv > 0 ? evt.Iv.ToString(CultureInfo.InvariantCulture) : "unknown",
                    $"{evt.Bounds.Latitude},{evt.Bounds.Longitude}"));
        }

        public void HandleEvent(DisplayHighestsPokemonEvent evt)
        {
            string strHeader;
            //PokemonData | CP | IV | Level | MOVE1 | MOVE2
            switch (evt.SortedBy)
            {
                case "Level":
                    strHeader = _translation.GetTranslation(TranslationString.DisplayHighestsLevelHeader);
                    break;
                case "IV":
                    strHeader = _translation.GetTranslation(TranslationString.DisplayHighestsPerfectHeader);
                    break;
                case "CP":
                    strHeader = _translation.GetTranslation(TranslationString.DisplayHighestsCpHeader);
                    break;
                case "MOVE1":
                    strHeader = _translation.GetTranslation(TranslationString.DisplayHighestMove1Header);
                    break;
                case "MOVE2":
                    strHeader = _translation.GetTranslation(TranslationString.DisplayHighestMove2Header);
                    break;
                default:
                    strHeader = _translation.GetTranslation(TranslationString.DisplayHighestsHeader);
                    break;
            }
            var strPerfect = _translation.GetTranslation(TranslationString.CommonWordPerfect);
            var strName = _translation.GetTranslation(TranslationString.CommonWordName).ToUpper();

            _logger.Write($"====== {strHeader} ======", LogLevel.Info, ConsoleColor.Yellow);
            foreach (var pokemon in evt.PokemonList)
                _logger.Write(
                    $"# CP {pokemon.Item1.Cp.ToString().PadLeft(4, ' ')}/{pokemon.Item2.ToString().PadLeft(4, ' ')} | ({pokemon.Item3.ToString("0.00")}% {strPerfect})\t| Lvl {pokemon.Item4.ToString("00")}\t {strName}: {_translation.GetPokemonName(pokemon.Item1.PokemonId).PadRight(10, ' ')}\t MOVE1: {pokemon.Item5.ToString().PadRight(20, ' ')} MOVE2: {pokemon.Item6}",
                    LogLevel.Info, ConsoleColor.Yellow);
        }

        public void HandleEvent(UpdateEvent evt)
        {
            _logger.Write(evt.ToString(), LogLevel.Update);
        }

        public void Listen(IEvent evt)
        {
            dynamic eve = evt;

            try
            {
                HandleEvent(eve);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }
    }
}