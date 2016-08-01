#region using directives

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using log4net.Repository.Hierarchy;
using Ninject;
using PoGo.PokeMobBot.Logic;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Logging;
using PoGo.PokeMobBot.Logic.Repository;
using PoGo.PokeMobBot.Logic.State;
using PoGo.PokeMobBot.Logic.Tasks;
using PoGo.PokeMobBot.Logic.Utils;
using PokemonGo.RocketAPI;

#endregion

namespace PoGo.PokeMobBot.CLI
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var culture = CultureInfo.CreateSpecificCulture("en-US");

            CultureInfo.DefaultThreadCurrentCulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;

            var subPath = "";
            if (args.Length > 0)
                subPath = args[0];

            IKernel kernel = new StandardKernel();
            kernel.Bind<Client>().To<Client>().InSingletonScope();
            kernel.Bind<ISettings>().To<ClientSettings>();
            kernel.Bind<ILogicSettings>().To<LogicSettings>();
            kernel.Bind<ISession>().To<Session>().InSingletonScope();
            kernel.Bind<ILogger>().To<ConsoleLogger>().WithConstructorArgument(LogLevel.Info);

            var logger = kernel.Get<ILogger>();

            var globalSettingsRepository = kernel.Get<GlobalSettingsRepository>();

            var settings = globalSettingsRepository.Get(subPath);
            kernel.Bind<GlobalSettings>().ToConstant(settings);


            if (settings == null)
            {
                logger.Write("This is your first start and the bot has generated the default config!", LogLevel.Warning);
                logger.Write("After pressing a key the config folder will open and this commandline will close", LogLevel.Warning);

                //pauses console until keyinput
                Console.ReadKey();

                // opens explorer with location "config"
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = "config",
                    UseShellExecute = true,
                    Verb = "open"
                });
                Environment.Exit(0);
            }
            //var session = new Session(new ClientSettings(settings), new LogicSettings(settings));
            var session = kernel.Get<Session>();
            session.Client.ApiFailure = new ApiFailureStrategy(session);


            /*SimpleSession session = new SimpleSession
            {
                _client = new PokemonGo.RocketAPI.Client(new ClientSettings(settings)),
                _dispatcher = new EventDispatcher(),
                _localizer = new Localizer()
            };

            BotService service = new BotService
            {
                _session = session,
                _loginTask = new Login(session)
            };

            service.Run();
            */

            var machine = new StateMachine();
            var stats = new Statistics();
            stats.DirtyEvent +=
                () =>
                    Console.Title =
                        stats.GetTemplatedStats(
                            session.Translation.GetTranslation(TranslationString.StatsTemplateString),
                            session.Translation.GetTranslation(TranslationString.StatsXpTemplateString));

            var aggregator = new StatisticsAggregator(stats);
            var listener = kernel.Get<ConsoleEventListener>();
            var websocket = kernel.Get<WebSocketInterface>();

            session.EventDispatcher.EventReceived += evt => listener.Listen(evt, session);
            session.EventDispatcher.EventReceived += evt => aggregator.Listen(evt, session);
            session.EventDispatcher.EventReceived += evt => websocket.Listen(evt, session);

            machine.SetFailureState(kernel.Get<LoginState>());

            session.Navigation.UpdatePositionEvent +=
                (lat, lng) => session.EventDispatcher.Send(new UpdatePositionEvent { Latitude = lat, Longitude = lng });

            machine.AsyncStart(kernel.Get<VersionCheckState>(), session);
            if (session.LogicSettings.UseSnipeLocationServer)
            {
                var snipePokemonTask = kernel.Get<SnipePokemonTask>();
                snipePokemonTask.AsyncStart(session);
            }

            //Non-blocking key reader
            //This will allow to process console key presses in another code parts
            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Enter)
                {
                    break;
                }
                Thread.Sleep(5);
            }
        }
    }
}