﻿using PoGo.PokeMobBot.Logic.Logging;
using PoGo.PokeMobBot.Logic.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PokeBot
{
    public class WpfLogger : ILogger
    {
        private readonly LogLevel _maxLogLevel;
        private ISession _session;

        static string strError = "ERROR";
        static string strAttention = "ATTENTION";
        static string strInfo = "INFO";
        static string strPokestop = "POKESTOP";
        static string strFarming = "FARMING";
        static string strRecycling = "RECYCLING";
        static string strPKMN = "PKMN";
        static string strTransfered = "TRANSFERED";
        static string strEvolved = "EVOLVED";
        static string strBerry = "BERRY";
        static string strEgg = "EGG";
        static string strDebug = "DEBUG";
        static string strUpdate = "UPDATE";

        public void SetSession(ISession session)
        {
            _session = session;

            if (_session != null)
            {
                strError = _session.Translation.GetTranslation(PoGo.PokeMobBot.Logic.Common.TranslationString.LogEntryError);
                strAttention = _session.Translation.GetTranslation(PoGo.PokeMobBot.Logic.Common.TranslationString.LogEntryAttention);
                strInfo = _session.Translation.GetTranslation(PoGo.PokeMobBot.Logic.Common.TranslationString.LogEntryInfo);
                strPokestop = _session.Translation.GetTranslation(PoGo.PokeMobBot.Logic.Common.TranslationString.LogEntryPokestop);
                strFarming = _session.Translation.GetTranslation(PoGo.PokeMobBot.Logic.Common.TranslationString.LogEntryFarming);
                strRecycling = _session.Translation.GetTranslation(PoGo.PokeMobBot.Logic.Common.TranslationString.LogEntryRecycling);
                strPKMN = _session.Translation.GetTranslation(PoGo.PokeMobBot.Logic.Common.TranslationString.LogEntryPkmn);
                strTransfered = _session.Translation.GetTranslation(PoGo.PokeMobBot.Logic.Common.TranslationString.LogEntryTransfered);
                strEvolved = _session.Translation.GetTranslation(PoGo.PokeMobBot.Logic.Common.TranslationString.LogEntryEvolved);
                strBerry = _session.Translation.GetTranslation(PoGo.PokeMobBot.Logic.Common.TranslationString.LogEntryBerry);
                strEgg = _session.Translation.GetTranslation(PoGo.PokeMobBot.Logic.Common.TranslationString.LogEntryEgg);
                strDebug = _session.Translation.GetTranslation(PoGo.PokeMobBot.Logic.Common.TranslationString.LogEntryDebug);
                strUpdate = _session.Translation.GetTranslation(PoGo.PokeMobBot.Logic.Common.TranslationString.LogEntryUpdate);
            }
        }

        /// <summary>
        ///     To create a ConsoleLogger, we must define a maximum log level.
        ///     All levels above won't be logged.
        /// </summary>
        /// <param name="maxLogLevel"></param>
        public WpfLogger(LogLevel maxLogLevel)
        {
            _maxLogLevel = maxLogLevel;
        }

        public void Write(string message, LogLevel level = LogLevel.Info, ConsoleColor color = ConsoleColor.Black)
        {
            //Remember to change to a font that supports your language, otherwise it'll still show as ???
            Console.OutputEncoding = Encoding.UTF8;
            if (level > _maxLogLevel)
                return;
            
            switch (level)
            {
                case LogLevel.Error:
                    SendWindowMsg("log", new object[] { $"[{DateTime.Now.ToString("HH:mm:ss")}] ({strError}) {message}", Color.FromRgb(255, 0, 0) });
                    break;
                case LogLevel.Warning:
                    SendWindowMsg("log", new object[] { $"[{DateTime.Now.ToString("HH:mm:ss")}] ({strAttention}) {message}", Color.FromRgb(255, 0, 0) });
                    break;
                case LogLevel.Info:
                    SendWindowMsg("log", new object[] { $"[{DateTime.Now.ToString("HH:mm:ss")}] ({strInfo}) {message}", Color.FromRgb(255, 0, 0) });
                    break;
                case LogLevel.Pokestop:
                    SendWindowMsg("log", new object[] { $"[{DateTime.Now.ToString("HH:mm:ss")}] ({strPokestop}) {message}", Color.FromRgb(255, 0, 0) });
                    break;
                case LogLevel.Farming:
                    SendWindowMsg("log", new object[] { $"[{DateTime.Now.ToString("HH:mm:ss")}] ({strFarming}) {message}", Color.FromRgb(255, 0, 0) });
                    break;
                case LogLevel.Recycling:
                    SendWindowMsg("log", new object[] { $"[{DateTime.Now.ToString("HH:mm:ss")}] ({strRecycling}) {message}", Color.FromRgb(255, 0, 0) });
                    break;
                case LogLevel.Caught:
                    SendWindowMsg("log", new object[] { $"[{DateTime.Now.ToString("HH:mm:ss")}] ({strPKMN}) {message}", Color.FromRgb(0, 255, 0) });
                    break;
                case LogLevel.Transfer:
                    SendWindowMsg("log", new object[] { $"[{DateTime.Now.ToString("HH:mm:ss")}] ({strTransfered}) {message}", Color.FromRgb(0, 255, 0) });
                    break;
                case LogLevel.Evolve:
                    SendWindowMsg("log", new object[] { $"[{DateTime.Now.ToString("HH:mm:ss")}] ({strEvolved}) {message}", Color.FromRgb(0, 255, 0) });
                    break;
                case LogLevel.Berry:
                    SendWindowMsg("log", new object[] { $"[{DateTime.Now.ToString("HH:mm:ss")}] ({strBerry}) {message}", Color.FromRgb(0, 255, 0) });
                    break;
                case LogLevel.Egg:
                    SendWindowMsg("log", new object[] { $"[{DateTime.Now.ToString("HH:mm:ss")}] ({strEgg}) {message}", Color.FromRgb(0, 255, 0) });
                    break;
                case LogLevel.Debug:
                    SendWindowMsg("log", new object[] { $"[{DateTime.Now.ToString("HH:mm:ss")}] ({strDebug}) {message}", Color.FromRgb(0, 255, 0) });
                    break;
                case LogLevel.Update:
                    SendWindowMsg("log", new object[] { $"[{DateTime.Now.ToString("HH:mm:ss")}] ({strUpdate}) {message}", Color.FromRgb(0, 255, 0) });
                    break;
                default:
                    SendWindowMsg("log", new object[] { $"[{DateTime.Now.ToString("HH:mm:ss")}] ({strError}) {message}", Color.FromRgb(255, 255, 255) });
                    break;
            }
        }

        public void PushObjectInfo(string infoType, params object[] objData)
        {
            SendWindowMsg(infoType, objData);
        }

        public void SendWindowMsg(string msgType, params object[] objData) => MainWindow.botWindow.ReceiveMsg(msgType, _session, objData);
    }
}