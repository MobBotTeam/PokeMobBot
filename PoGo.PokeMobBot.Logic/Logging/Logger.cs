#region using directives

using System;
using System.IO;
using PoGo.PokeMobBot.Logic.State;

#endregion

namespace PoGo.PokeMobBot.Logic.Logging
{
    //public  class Logger
    //{
    //    private static ILogger _logger;
        
    //    /// <summary>
    //    ///     Set the logger. All future requests to <see cref="Write(string,LogLevel,ConsoleColor)" /> will use that logger, any
    //    ///     old will be
    //    ///     unset.
    //    /// </summary>
    //    /// <param name="logger"></param>
    //    public static void SetLogger(ILogger logger)
    //    {
    //        _logger = logger;
    //        _logger.Write($"Initializing Rocket logger at time {DateTime.Now}...");
    //    }

    //    /// <summary>
    //    ///     Sets Context for the logger
    //    /// </summary>
    //    /// <param name="session">Context</param>
    //    public static void SetLoggerContext(ISession session)
    //    {
    //        _logger?.SetSession(session);
    //    }

    //    /// <summary>
    //    ///     Log a specific message to the logger setup by <see cref="SetLogger(ILogger)" /> .
    //    /// </summary>
    //    /// <param name="message">The message to log.</param>
    //    /// <param name="level">Optional level to log. Default <see cref="LogLevel.Info" />.</param>
    //    /// <param name="color">Optional. Default is automatic color.</param>
    //    public static void Write(string message, LogLevel level = LogLevel.Info, ConsoleColor color = ConsoleColor.Black)
    //    {
    //        _logger?.Write(message, level, color);
    //    }
    //}

    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Pokestop = 3,
        Farming = 4,
        Recycling = 5,
        Berry = 6,
        Caught = 7,
        Transfer = 8,
        Evolve = 9,
        Egg = 10,
        Update = 11,
        Info = 12,
        Debug = 13
    }
}