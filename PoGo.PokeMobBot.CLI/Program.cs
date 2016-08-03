#region using directives

using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
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
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Networking.Responses;

#endregion

namespace PoGo.PokeMobBot.CLI
{
    internal class Program
    {
        // http://stackoverflow.com/questions/2586612/how-to-keep-a-net-console-app-running Save some CPU Cycles, speed things up
        static ManualResetEvent _quitEvent = new ManualResetEvent(false);

        private static void Main(string[] args)
        {
            Console.CancelKeyPress += (sender, eArgs) =>
            {
                _quitEvent.Set();
                eArgs.Cancel = true;
            };

            var culture = CultureInfo.CreateSpecificCulture("en-US");

            CultureInfo.DefaultThreadCurrentCulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;

            var subPath = "";
            if (args.Length > 0)
                subPath = args[0];

#if DEBUG
            LogLevel logLevel = LogLevel.Debug;
#else
            LogLevel logLevel = LogLevel.Info;
#endif

            IKernel kernel = new StandardKernel();
            kernel.Bind<ISettings>().To<ClientSettings>().InSingletonScope();
            kernel.Bind<IApiFailureStrategy>().To<ApiFailureStrategy>().InSingletonScope();
            kernel.Bind<Inventory>().To<Inventory>().InSingletonScope();
            kernel.Bind<GetPlayerResponse>().To<GetPlayerResponse>().InSingletonScope();
            kernel.Bind<ILogicSettings>().To<LogicSettings>().InSingletonScope();
            kernel.Bind<Navigation>().To<Navigation>().InSingletonScope();
            kernel.Bind<ITranslation>().To<Translation>().InSingletonScope();
            kernel.Bind<IEventDispatcher>().To<EventDispatcher>().InSingletonScope();
            kernel.Bind<ILogger>().To<ConsoleLogger>().WithConstructorArgument(logLevel);
            kernel.Bind<FarmPokestopsTask>().To<FarmPokestopsTask>().InSingletonScope();

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

            // very very dirty hacks...
            var client = new Client(kernel.Get<ISettings>(), null);
            kernel.Bind<Client>().ToConstant(client);
            client.ApiFailure = kernel.Get<IApiFailureStrategy>();


            var translation = kernel.Get<ITranslation>();
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

            var machine = kernel.Get<StateMachine>();
            var stats = kernel.Get<Statistics>();
            stats.DirtyEvent +=
                () =>
                    Console.Title =
                        stats.GetTemplatedStats(
                            translation.GetTranslation(TranslationString.StatsTemplateString),
                            translation.GetTranslation(TranslationString.StatsXpTemplateString));

            var aggregator = kernel.Get<StatisticsAggregator>();
            var listener = kernel.Get<ConsoleEventListener>();
            var websocket = kernel.Get<WebSocketInterface>();
            var eventDispatcher = kernel.Get<IEventDispatcher>();
            var navigation = kernel.Get<Navigation>();
            var logicSettings = kernel.Get<ILogicSettings>();

            eventDispatcher.EventReceived += evt => listener.Listen(evt);
            eventDispatcher.EventReceived += evt => aggregator.Listen(evt);
            eventDispatcher.EventReceived += evt => websocket.Listen(evt);

            machine.SetFailureState(kernel.Get<LoginState>());

            navigation.UpdatePositionEvent +=
                (lat, lng) => eventDispatcher.Send(new UpdatePositionEvent { Latitude = lat, Longitude = lng });

            machine.AsyncStart(kernel.Get<VersionCheckState>());

            if (logicSettings.UseSnipeLocationServer)
            {
                var snipePokemonTask = kernel.Get<SnipePokemonTask>();
                snipePokemonTask.AsyncStart();
            }

            _quitEvent.WaitOne();
        }
    }
}