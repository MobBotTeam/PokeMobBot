using GMap.NET;
using GMap.NET.WindowsPresentation;
using PoGo.PokeMobBot.Logic;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Logging;
using PoGo.PokeMobBot.Logic.State;
using PoGo.PokeMobBot.Logic.Tasks;
using PoGo.PokeMobBot.Logic.Utils;
using POGOProtos.Enums;
using POGOProtos.Map.Fort;
using POGOProtos.Map.Pokemon;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Catchem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow botWindow;
        private bool windowClosing = false;
        string subPath = "Profiles";

        private Dictionary<ISession, BotWindowData> openedSessions = new Dictionary<ISession, BotWindowData>(); //may be not session... but some uniq obj for every running bot
        private ISession curSession = null;

        private BotWindowData bot
        {
            get
            {
                if (curSession != null)
                {
                    if (openedSessions.ContainsKey(curSession))
                    {
                        return openedSessions[curSession];
                    }
                }
                return null;
            }
        }
        GMapMarker playerMarker;

        bool LoadingUi = false;

        public MainWindow()
        {
            InitializeComponent();
            InitWindowsComtrolls();
            InitializeMap();
            botWindow = this;

            LogWorker();
            MarkersWorker();
            MovePlayer();
            InitBots();
        }

        void InitWindowsComtrolls()
        {
            authBox.ItemsSource = Enum.GetValues(typeof(AuthType));
        }

        private async void InitializeMap()
        {
            pokeMap.Bearing = 0;

            pokeMap.CanDragMap = true;

            pokeMap.DragButton = MouseButton.Left;

            //pokeMap.GrayScleMode = true;

            //pokeMap.MarkersEnabled = true;

            pokeMap.MaxZoom = 18;

            pokeMap.MinZoom = 2;

            pokeMap.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;

            //pokeMap.NegativeMode = false;

            //pokeMap.PolygonsEnabled = true;

            pokeMap.ShowCenter = false;

            //pokeMap.RoutesEnabled = true;

            pokeMap.ShowTileGridLines = false;

            pokeMap.Zoom = 18;

            pokeMap.MapProvider = GMap.NET.MapProviders.GMapProviders.GoogleMap;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;

            GMap.NET.MapProviders.GMapProvider.WebProxy = System.Net.WebRequest.GetSystemWebProxy();
            GMap.NET.MapProviders.GMapProvider.WebProxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

            if (bot != null)
                pokeMap.Position = new GMap.NET.PointLatLng(bot.Lat, bot.Lng);

            await Task.Delay(10);
        }

        internal void InitBots()
        {
            Logger.SetLogger(new WpfLogger(LogLevel.Info), subPath);

            foreach (var item in Directory.GetDirectories(subPath))
            {
                if (item != subPath + "\\Logs")
                {
                    InitBot(GlobalSettings.Load(item), System.IO.Path.GetFileName(item));
                }
            }
        }


        public void ReceiveMsg(string msgType, ISession session, params object[] objData)
        {
            if (session == null) return;
            switch (msgType)
            {
                case "log":
                    PushNewConsoleRow(session, (string)objData[0], (Color)objData[1]);
                    break;
                case "ps":
                    PushNewPokestop(session, (IEnumerable<FortData>)objData[0]);
                    break;
                case "pm":
                    PushNewPokemons(session, (IEnumerable<MapPokemon>)objData[0]);
                    break;
                case "pmw":
                    PushNewWildPokemons(session, (IEnumerable<WildPokemon>)objData[0]);
                    break;
                case "pm_rm":
                    PushRemovePokemon(session, (MapPokemon)objData[0]);
                    break;
                case "p_loc":
                    UpdateCoords(session, objData);
                    break;
                case "forcemove_done":
                    PushRemoveForceMoveMarker(session);
                    break;
            }
        }

        private void UpdateCoords(ISession session, object[] objData)
        {
            try
            {
                if (session != curSession)
                {
                    if (openedSessions.ContainsKey(session))
                    {
                        var botReceiver = openedSessions[session];
                        botReceiver.Lat = botReceiver._lat = (double)objData[0];
                        botReceiver.Lng = botReceiver._lng = (double)objData[1];
                    }
                }
                else
                {
                    bot.moveRequired = true;
                    if (bot._lat == 0 && bot._lng == 0)
                    {
                        bot.Lat = bot._lat = (double)objData[0];
                        bot.Lng = bot._lng = (double)objData[1];
                        Dispatcher.BeginInvoke(new ThreadStart(delegate
                        {
                            pokeMap.Position = new PointLatLng(bot.Lat, bot.Lng);
                        }));                        
                    }
                    else
                    {
                        bot.Lat = (double)objData[0];
                        bot.Lng = (double)objData[1];
                    }

                    if (playerMarker == null)
                    {
                        Dispatcher.BeginInvoke(new ThreadStart(DrawPlayerMarker));                        
                    }
                    else
                    {
                        bot.gotNewCoord = true;
                    }                    
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void DrawPlayerMarker()
        {
            playerMarker = new GMapMarker(new PointLatLng(bot.Lat, bot.Lng))
            {
                Shape = Properties.Resources.trainer.ToImage("Player"),
                Offset = new Point(-14, -40),
                ZIndex = 15
            };
            pokeMap.Markers.Add(playerMarker);

            if (bot.ForceMoveMarker != null)
                pokeMap.Markers.Add(bot.ForceMoveMarker);
        }

        private void PushNewConsoleRow(ISession session, string rowText, Color rowColor)
        {
            if (openedSessions.ContainsKey(session))
            {
                openedSessions[session].logQueue.Enqueue(Tuple.Create(rowText, rowColor));
            }
        }

        private void PushRemoveForceMoveMarker(ISession session)
        {
            if (!openedSessions.ContainsKey(session)) return;
            var tBot = openedSessions[session];
            var nMapObj = new NewMapObject("forcemove_done", "", 0, 0, "");
            tBot.MarkersQueue.Enqueue(nMapObj);
        }

        private void PushRemovePokemon(ISession session, MapPokemon mapPokemon)
        {
            if (!openedSessions.ContainsKey(session)) return;
            var tBot = openedSessions[session];
            var nMapObj = new NewMapObject("pm_rm", mapPokemon.PokemonId.ToString(), mapPokemon.Latitude, mapPokemon.Longitude, mapPokemon.EncounterId.ToString());
            tBot.MarkersQueue.Enqueue(nMapObj);
        }

        private void PushNewPokemons(ISession session, IEnumerable<MapPokemon> pokemons)
        {
            if (!openedSessions.ContainsKey(session)) return;
            foreach (var pokemon in pokemons)
            {
                var tBot = openedSessions[session];
                if (tBot.mapMarkers.ContainsKey(pokemon.EncounterId.ToString()) ||
                    tBot.MarkersQueue.Count(x => x.uid == pokemon.EncounterId.ToString()) != 0) continue;
                var nMapObj = new NewMapObject("pm", pokemon.PokemonId.ToString(), pokemon.Latitude, pokemon.Longitude, pokemon.EncounterId.ToString());
                tBot.MarkersQueue.Enqueue(nMapObj);
            }
        }

        private void PushNewWildPokemons(ISession session, IEnumerable<WildPokemon> pokemons)
        {
            if (!openedSessions.ContainsKey(session)) return;
            foreach (var pokemon in pokemons)
            {
                var tBot = openedSessions[session];
                if (tBot.mapMarkers.ContainsKey(pokemon.EncounterId.ToString()) ||
                    tBot.MarkersQueue.Count(x => x.uid == pokemon.EncounterId.ToString()) != 0) continue;
                var nMapObj = new NewMapObject("pm", pokemon.PokemonData.PokemonId.ToString(), pokemon.Latitude, pokemon.Longitude, pokemon.EncounterId.ToString());
                tBot.MarkersQueue.Enqueue(nMapObj);
            }
        }

        private void PushNewPokestop(ISession session, IEnumerable<FortData> pstops)
        {
            if (!openedSessions.ContainsKey(session)) return;
            var fortDatas = pstops as FortData[] ?? pstops.ToArray();
            for (int i = 0; i < fortDatas.Length; i++)
            {
                try
                {
                    var tBot = openedSessions[session];
                    if (tBot.mapMarkers.ContainsKey(fortDatas[i].Id) || tBot.MarkersQueue.Count(x => x.uid == fortDatas[i].Id) != 0)
                    continue;
                    var lured = fortDatas[i].LureInfo?.LureExpiresTimestampMs > DateTime.UtcNow.ToUnixTime();
                    var nMapObj = new NewMapObject("ps" + (lured ? "_lured" : ""), "PokeStop", fortDatas[i].Latitude,
                        fortDatas[i].Longitude, fortDatas[i].Id);
                    openedSessions[session].MarkersQueue.Enqueue(nMapObj);
                }
                catch
                {
                    i--;
                }
            }
        }

        private async void MovePlayer()
        {
            const int delay = 25;
            while (!windowClosing)
            {
                if (bot != null && playerMarker != null && bot.Started)
                {
                    if (bot.moveRequired)
                    {
                        if (bot.gotNewCoord)
                        {
                            bot._latStep = (bot.Lat - bot._lat) / (2000 / delay);
                            bot._lngStep = (bot.Lng - bot._lng) / (2000 / delay);
                            bot.gotNewCoord = false;
                        }

                        bot._lat += bot._latStep;
                        bot._lng += bot._lngStep;
                        playerMarker.Position = new PointLatLng(bot._lat, bot._lng);
                        if (Math.Abs(bot._lat - bot.Lat) < 0.000000001 && Math.Abs(bot._lng - bot.Lng) < 0.000000001)
                            bot.moveRequired = false;
                        UpdateCoordBoxes();
                    }
                }
                await Task.Delay(delay);
            }
        }


        private async void MarkersWorker()
        {
            while (!windowClosing)
            {
                if (bot?.MarkersQueue.Count > 0)
                {
                    try
                    {
                        var newMapObj = bot.MarkersQueue.Dequeue();
                        switch (newMapObj.oType)
                        {
                            case "ps":
                                if (!bot.mapMarkers.ContainsKey(newMapObj.uid))
                                {
                                    var marker = new GMapMarker(new PointLatLng(newMapObj.lat, newMapObj.lng))
                                    {
                                        Shape = Properties.Resources.pstop.ToImage("PokeStop"),
                                        Offset = new Point(-16, -32),
                                        ZIndex = 5
                                    };
                                    pokeMap.Markers.Add(marker);
                                    bot.mapMarkers.Add(newMapObj.uid, marker);
                                }
                                break;
                            case "ps_lured":
                                if (!bot.mapMarkers.ContainsKey(newMapObj.uid))
                                {
                                    var marker = new GMapMarker(new PointLatLng(newMapObj.lat, newMapObj.lng))
                                    {
                                        Shape = Properties.Resources.pstop_lured.ToImage("Lured PokeStop"),
                                        Offset = new Point(-16, -32),
                                        ZIndex = 5
                                    };
                                    pokeMap.Markers.Add(marker);
                                    bot.mapMarkers.Add(newMapObj.uid, marker);
                                }
                                break;
                            case "pm_rm":
                                if (bot.mapMarkers.ContainsKey(newMapObj.uid))
                                {
                                    pokeMap.Markers.Remove(bot.mapMarkers[newMapObj.uid]);
                                    bot.mapMarkers.Remove(newMapObj.uid);
                                }
                                break;
                            case "forcemove_done":
                                if (bot.ForceMoveMarker != null)
                                {
                                    pokeMap.Markers.Remove(bot.ForceMoveMarker);
                                    bot.ForceMoveMarker = null;
                                }
                                break;
                            case "pm":
                                if (!bot.mapMarkers.ContainsKey(newMapObj.uid))
                                {
                                    CreatePokemonMarker(newMapObj);
                                }
                                break;
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
                await Task.Delay(10);
            }
        }

        private void CreatePokemonMarker(NewMapObject newMapObj)
        {
            PokemonId pokemon = (PokemonId)Enum.Parse(typeof(PokemonId), newMapObj.oName);

            var marker = new GMapMarker(new PointLatLng(newMapObj.lat, newMapObj.lng))
            {
                Shape = pokemon.ToImage(),
                Offset = new Point(-15, -30),
                ZIndex = 10
            };
            pokeMap.Markers.Add(marker);
            bot.mapMarkers.Add(newMapObj.uid, marker);
        }

        private async void LogWorker()
        {
            while (!windowClosing)
            {
                if (bot != null)
                {
                    if (bot.logQueue.Count > 0)
                    {
                        var t = bot.logQueue.Dequeue();
                        bot.log.Add(t);
                        consoleBox.AppendParagraph(t.Item1, t.Item2);
                    }                    
                }
                await Task.Delay(10);
            }
        }


        private class BotWindowData
        {
            public string profileName = string.Empty;
            private CancellationTokenSource cts;
            public CancellationToken cancellationToken
            {
                get
                {
                    return cts.Token;
                }
            }
            internal GMapMarker ForceMoveMarker;
            public List<Tuple<string, Color>> log = new List<Tuple<string, Color>>();
            public Queue<Tuple<string, Color>> logQueue = new Queue<Tuple<string, Color>>();
            public Dictionary<string, GMapMarker> mapMarkers = new Dictionary<string, GMapMarker>();
            public Queue<NewMapObject> MarkersQueue = new Queue<NewMapObject>();
            public StateMachine machine = null;
            public Statistics stats = null;
            public StatisticsAggregator aggregator = null;
            public WpfEventListener listener = null;
            public ClientSettings settings = null;
            public LogicSettings logic = null;
            public GlobalSettings globalSettings = null;

            public Label runTime;
            public Label xpph;
            public bool Started = false;

            private DispatcherTimer timer;
            private TimeSpan ts;

            public double Lat;
            public double Lng;
            public bool gotNewCoord = false;
            public bool moveRequired = false;
            private double _la, _ln;
            public double _lat
            {
                get { return _la; }
                set
                {
                    globalSettings.DefaultLatitude = value;
                    _la = value;
                }
            }
            public double _lng
            {
                get { return _ln; }
                set
                {
                    globalSettings.DefaultLongitude = value;
                    _ln = value;
                }
            }
            public double _latStep = 0, _lngStep = 0;

            public BotWindowData(string name, GlobalSettings gs, StateMachine sm, Statistics st, StatisticsAggregator sa, WpfEventListener wel, ClientSettings cs, LogicSettings l)
            {
                profileName = name;
                settings = new ClientSettings(gs);
                logic = new LogicSettings(gs);
                globalSettings = gs;
                machine = sm;
                stats = st;
                aggregator = sa;
                listener = wel;
                settings = cs;
                logic = l;
                Lat = globalSettings.DefaultLatitude;
                Lng = globalSettings.DefaultLongitude;

                ts = new TimeSpan();
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Tick += delegate (object o, EventArgs args)
                {
                    ts += new TimeSpan(0, 0, 1);
                    runTime.Content = ts.ToString();
                };
                cts = new CancellationTokenSource();
            }

            public void UpdateXppH()
            {
                if (stats == null || ts.TotalHours == 0)
                    xpph.Content = 0;
                else
                    xpph.Content = "Xp/h: " + (stats.TotalExperience / ts.TotalHours).ToString("0.0");
            }

            private void WipeData()
            {
                log = new List<Tuple<string, Color>>();
                mapMarkers = new Dictionary<string, GMapMarker>();
                MarkersQueue = new Queue<NewMapObject>();
                logQueue = new Queue<Tuple<string, Color>>();
            }

            public void Stop()
            {
                timerStop();
                cts.Cancel();
                WipeData();
                ts = new TimeSpan();
                Started = false;
            }

            public void Start()
            {
                timerStart();
                cts.Dispose();
                cts = new CancellationTokenSource();
                Started = true;
            }

            private void timerStart() => timer?.Start();

            private void timerStop() => timer?.Stop();

            internal void EnqueData()
            {
                while (logQueue.Count > 0)
                    log.Add(logQueue.Dequeue());
                foreach (var item in log)                
                    logQueue.Enqueue(item);
                log = new List<Tuple<string, Color>>();
            }
        }

        internal class NewMapObject
        {
            public string oType;
            public string oName;
            public double lat;
            public double lng;
            public string uid;
            public NewMapObject(string _oType, string _oName, double _lat, double _lng, string _uid)
            {
                oType = _oType;
                oName = _oName;
                lat = _lat;
                lng = _lng;
                uid = _uid;
            }
        }

        #region Controll's events
        private void authBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.Auth.AuthType = (AuthType)(sender as ComboBox).SelectedItem;            
        }

        private void loginBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            if (bot.globalSettings.Auth.AuthType == AuthType.Google)
                bot.globalSettings.Auth.GoogleUsername = (sender as TextBox).Text;
            else
                bot.globalSettings.Auth.PtcUsername = (sender as TextBox).Text;
        }

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            if (bot.globalSettings.Auth.AuthType == AuthType.Google)
                bot.globalSettings.Auth.GooglePassword = (sender as PasswordBox).Password;
            else
                bot.globalSettings.Auth.PtcPassword = (sender as PasswordBox).Password;
        }

        private void proxyUriBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.ProxyUri = (sender as TextBox).Text;
        }

        private void useProxyChb_Checked(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.UseProxy = (bool)(sender as CheckBox).IsChecked;
            proxyUriBox.IsEnabled = proxyPasswordBox.IsEnabled = proxyLoginBox.IsEnabled = bot.globalSettings.UseProxy;

        }

        private void proxyLoginBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.ProxyLogin = (sender as TextBox).Text;
        }

        private void proxyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.ProxyPass = (sender as PasswordBox).Password;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            InputBox.Visibility = System.Windows.Visibility.Visible;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            // YesButton Clicked! Let's hide our InputBox and handle the input text.
            InputBox.Visibility = System.Windows.Visibility.Collapsed;

            // Do something with the Input
            String input = InputTextBox.Text;

            var dir = Directory.CreateDirectory(subPath + "\\" + input);
            var settings = GlobalSettings.Load(dir.FullName);
            if (settings == null)
            {
                settings = GlobalSettings.Load(dir.FullName);
            }
            InitBot(settings, input);
            // Clear InputBox.
            InputTextBox.Text = String.Empty;
        }

        private void InitBot(GlobalSettings settings, string profileName = "Unknown")
        {
            var newBot = CreateBowWindowData(settings, profileName);

            var session = new Session(newBot.settings, newBot.logic);
            session.Client.ApiFailure = new ApiFailureStrategy(session);

            

            session.EventDispatcher.EventReceived += evt => newBot.listener.Listen(evt, session);
            session.EventDispatcher.EventReceived += evt => newBot.aggregator.Listen(evt, session);
            session.Navigation.UpdatePositionEvent +=
                (lat, lng) => session.EventDispatcher.Send(new UpdatePositionEvent {Latitude = lat, Longitude = lng});

            newBot.stats.DirtyEvent += () => { StatsOnDirtyEvent(newBot); };

            newBot._lat = settings.DefaultLatitude;
            newBot._lng = settings.DefaultLongitude;

            newBot.machine.SetFailureState(new LoginState());

            openedSessions.Add(session, newBot);

            Grid botGrid = new Grid()
            {
                Height = 120,
                Margin = new Thickness(0, 10, 0, 0)
            };
            Rectangle rec = new Rectangle()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Fill = new SolidColorBrush(Color.FromArgb(255, 97, 97, 97))
            };
            botGrid.Children.Add(rec);

            var r = this.FindResource("flatbutton") as Style;
            Button bStop = new Button()
            {
                Style = r,
                Content = "Stop",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 100,
                Height = 30,
                Margin = new Thickness(10, 80, 0, 0),
                Background = new LinearGradientBrush((Color)ColorConverter.ConvertFromString("#FFEEB29C"), (Color)ColorConverter.ConvertFromString("#FFC05353"), new Point(1, 0.5), new Point(0, 0.05))
            };
            Button bStart = new Button()
            {
                Style = r,
                Content = "Start",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 100,
                Height = 30,
                Margin = new Thickness(136, 80, 0, 0),
                Background = new LinearGradientBrush((Color)ColorConverter.ConvertFromString("#FFB0EE9C"), (Color)ColorConverter.ConvertFromString("#FF53C0B1"), new Point(1, 0.5), new Point(0, 0.05))
            };
            botGrid.Children.Add(bStop);
            botGrid.Children.Add(bStart);

            Label lbProfile = new Label()
            {
                Content = profileName,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF59C3B0")),
                FontSize = 18,
                Height = 38,
            };
            botGrid.Children.Add(lbProfile);

            Label lbLevel = new Label()
            {
                Content = "0",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF59C3B0")),
                FontSize = 14,
                Height = 38,
                Margin = new Thickness(0, 37, 0, 0)
            };
            botGrid.Children.Add(lbLevel);

            Label lvRuntime = new Label()
            {
                Content = "00:00",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF59C3B0")),
                FontSize = 14,
                Height = 38,
                Margin = new Thickness(152, 37, 0, 0)
            };
            botGrid.Children.Add(lvRuntime);

            newBot.xpph = lbLevel;
            newBot.runTime = lvRuntime;

            botPanel.Children.Add(botGrid);


            bStart.Click += delegate (object o, RoutedEventArgs args)
            {
                if (!newBot.Started)
                {
                    session.Client.Player.SetCoordinates(newBot.globalSettings.DefaultLatitude, newBot.globalSettings.DefaultLongitude, newBot.globalSettings.DefaultAltitude);
                    session.Client.Login = new PokemonGo.RocketAPI.Rpc.Login(session.Client);
                    newBot.Start();
                    newBot.machine.AsyncStart(new VersionCheckState(), session, newBot.cancellationToken);
                    if (session.LogicSettings.UseSnipeLocationServer)
                        SnipePokemonTask.AsyncStart(session);                   
                }
            };

            bStop.Click += delegate (object o, RoutedEventArgs args)
            {
                if (curSession == session)
                {
                    ClearPokemonData();
                }
                newBot.Stop();
            };

            rec.MouseLeftButtonDown += delegate (object o, MouseButtonEventArgs args)
            {
                if (bot != null)
                {
                    bot.globalSettings.StoreData(subPath + "\\" + bot.profileName);
                    bot.EnqueData();
                    ClearPokemonData();
                }
                foreach (var marker in newBot.mapMarkers.Values)
                {
                    pokeMap.Markers.Add(marker);
                }
                this.curSession = session;
                if (bot != null)
                {
                    pokeMap.Position = new PointLatLng(bot._lat, bot._lng);
                    DrawPlayerMarker();
                    StatsOnDirtyEvent(bot);
                }
                foreach (var item in botPanel.GetLogicalChildCollection<Rectangle>())
                {
                    item.Fill = !Equals(item, o) ? new SolidColorBrush(Color.FromArgb(255, 97, 97, 97)) : new SolidColorBrush(Color.FromArgb(255, 97, 97, 225));
                }
                RebuildUi();   
            };
        }

        private void StatsOnDirtyEvent(BotWindowData _bot)
        {
            if (_bot == null) throw new ArgumentNullException(nameof(_bot));
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                _bot.UpdateXppH();
            }));
            if (bot == _bot)
            {
                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    Playername.Content = curSession.Profile?.PlayerData?.Username;
                    l_StarDust.Content = bot.stats?.TotalStardust;
                    l_Stardust_farmed.Content = bot.stats?.TotalStardust == 0 ? 0 : bot.stats?.TotalStardust - curSession?.Profile?.PlayerData?.Currencies[1].Amount;
                    l_xp.Content = bot.stats?.ExportStats?.CurrentXp;
                    l_xp_farmed.Content = bot.stats?.TotalExperience;
                    l_coins.Content = curSession.Profile?.PlayerData?.Currencies[0].Amount;
                    l_Pokemons_farmed.Content = bot.stats?.TotalPokemons;
                    l_Pokemons_transfered.Content = bot.stats?.TotalPokemonsTransfered;
                    l_Pokestops_farmed.Content = bot.stats?.TotalPokestops;
                    l_level.Content = bot.stats?.ExportStats?.Level;
                    l_level_nextime.Content = $"{bot.stats?.ExportStats?.HoursUntilLvl.ToString("00")}:{bot.stats?.ExportStats?.MinutesUntilLevel.ToString("00")}";
                }));
            }
        }

        private void ClearPokemonData()
        {
            consoleBox.Document.Blocks.Clear();
            pokeMap.Markers.Clear();
            playerMarker = null;
        }

        private void RebuildUi()
        {
            if (bot == null || LoadingUi) return;

            LoadingUi = true;
            settings_grid.IsEnabled = true;
            if (!tabControl.IsEnabled)
                tabControl.IsEnabled = true;

            authBox.SelectedItem = bot.globalSettings.Auth.AuthType;
            if (bot.globalSettings.Auth.AuthType == AuthType.Google)
            {
                loginBox.Text = bot.globalSettings.Auth.GoogleUsername;
                passwordBox.Password = bot.globalSettings.Auth.GooglePassword;
            }
            else
            {
                loginBox.Text = bot.globalSettings.Auth.PtcUsername;
                passwordBox.Password = bot.globalSettings.Auth.PtcPassword;
            }

            useProxyChb.IsChecked = bot.globalSettings.UseProxy;
            proxyUriBox.Text = bot.globalSettings.ProxyUri;
            proxyLoginBox.Text = bot.globalSettings.ProxyLogin;
            proxyPasswordBox.Password = bot.globalSettings.ProxyPass;


            c_autoLevelPokemons.IsChecked = bot.globalSettings.AutomaticallyLevelUpPokemon;
            c_EvolveAllPokemonAboveIv.IsChecked = bot.globalSettings.EvolveAllPokemonAboveIv;
            c_EvolveAboveIvValue.Text = bot.globalSettings.EvolveAboveIvValue.ToString();
            c_EvolveAllPokemonWithEnoughCandy.IsChecked = bot.globalSettings.EvolveAllPokemonWithEnoughCandy;
            c_KeepMinCp.Text = bot.globalSettings.KeepMinCp.ToString();
            c_KeepMinDuplicatePokemon.Text = bot.globalSettings.KeepMinDuplicatePokemon.ToString();
            c_KeepMinIvPercentage.Text = bot.globalSettings.KeepMinIvPercentage.ToString();
            c_MaxPokeballsPerPokemon.Text = bot.globalSettings.MaxPokeballsPerPokemon.ToString();
            c_KeepPokemonsThatCanEvolve.IsChecked = bot.globalSettings.KeepPokemonsThatCanEvolve;
            c_PrioritizeIvOverCp.IsChecked = bot.globalSettings.PrioritizeIvOverCp;
            c_RenameOnlyAboveIv.IsChecked = bot.globalSettings.RenameOnlyAboveIv;
            c_RenamePokemon.IsChecked = bot.globalSettings.RenamePokemon;
            c_RenameTemplate.Text = bot.globalSettings.RenameTemplate;


            c_altitude.Text = bot.globalSettings.DefaultAltitude.ToString();
            UpdateCoordBoxes();
            c_teleport.IsChecked = bot.globalSettings.Teleport;
            c_UseDiscoveryPathing.IsChecked = bot.globalSettings.UseDiscoveryPathing;
            c_MaxSpawnLocationOffset.Text = bot.globalSettings.MaxSpawnLocationOffset.ToString();
            c_MaxTravelDistanceInMeters.Text = bot.globalSettings.MaxTravelDistanceInMeters.ToString();

            c_MinDelayBetweenSnipes.Text = bot.globalSettings.MinDelayBetweenSnipes.ToString();
            c_MinPokeballsToSnipe.Text = bot.globalSettings.MinPokeballsToSnipe.ToString();
            c_MinPokeballsWhileSnipe.Text = bot.globalSettings.MinPokeballsWhileSnipe.ToString();

            c_TranslationLanguageCode.Text = bot.globalSettings.TranslationLanguageCode;
            c_TotalAmountOfPokebalsToKeep.Text = bot.globalSettings.TotalAmountOfPokeballsToKeep.ToString();
            c_TotalAmountOfPotionsToKeep.Text = bot.globalSettings.TotalAmountOfPotionsToKeep.ToString();
            c_TotalAmountOfRevivesToKeep.Text = bot.globalSettings.TotalAmountOfRevivesToKeep.ToString();
            c_TotalAmountOfBerriesToKeep.Text = bot.globalSettings.TotalAmountOfBerriesToKeep.ToString();
            c_TransferDuplicatePokemon.IsChecked = bot.globalSettings.TransferDuplicatePokemon;
            c_UseEggIncubators.IsChecked = bot.globalSettings.UseEggIncubators;
            c_UseLuckyEggsMinPokemonAmount.Text = bot.globalSettings.UseLuckyEggsMinPokemonAmount.ToString();
            c_UseLuckyEggsWhileEvolving.IsChecked = bot.globalSettings.UseLuckyEggsWhileEvolving;
            c_UseTransferIvForSnipe.IsChecked = bot.globalSettings.UseTransferIvForSnipe;

            c_UseGreatBallAboveIv.Text = bot.logic.UseGreatBallAboveIv.ToString();
            c_UseUltraBallAboveIv.Text = bot.globalSettings.UseUltraBallAboveIv.ToString();

            c_UseUltraBallBelowCatchProbability.Text = bot.globalSettings.UseUltraBallBelowCatchProbability.ToString();
            c_UseGreatBallBelowCatchProbability.Text = bot.globalSettings.UseGreatBallBelowCatchProbability.ToString();
            c_UseMasterBallBelowCatchProbability.Text = bot.globalSettings.UseMasterBallBelowCatchProbability.ToString();

            c_UsePokemonToNotCatchFilter.IsChecked = bot.globalSettings.UsePokemonToNotCatchFilter;
            c_SnipeAtPokestops.IsChecked = bot.globalSettings.SnipeAtPokestops;
            c_SnipeIgnoreUnknownIv.IsChecked = bot.globalSettings.SnipeIgnoreUnknownIv;
            c_StartupWelcomeDelay.IsChecked = bot.globalSettings.StartupWelcomeDelay;
            c_UpgradePokemonIvMinimum.Text = bot.globalSettings.UpgradePokemonIvMinimum.ToString();
            c_UpgradePokemonCpMinimum.Text = bot.globalSettings.UpgradePokemonCpMinimum.ToString();
            c_WalkingSpeedInKilometerPerHour.Text = bot.globalSettings.WalkingSpeedInKilometerPerHour.ToString();

            c_HumanizeThrows.IsChecked = bot.globalSettings.HumanizeThrows;
            c_ThrowAccuracyMin.Text = bot.globalSettings.ThrowAccuracyMin.ToString();
            c_ThrowAccuracyMax.Text = bot.globalSettings.ThrowAccuracyMax.ToString();
            c_ThrowSpinFrequency.Text = bot.globalSettings.ThrowSpinFrequency.ToString();

            c_UseBerryMinCp.Text = bot.globalSettings.UseBerryMinCp.ToString();
            c_UseBerryMinIv.Text = bot.globalSettings.UseBerryMinIv.ToString();
            c_UseBerryBelowCatchProbability.Text = bot.globalSettings.UseBerryBelowCatchProbability.ToString();

            c_RecycleInventoryAtUsagePercentage.Text = bot.globalSettings.RecycleInventoryAtUsagePercentage.ToString();

            LoadingUi = false;
        }

        private void UpdateCoordBoxes()
        {
            c_latitude.Text = bot.globalSettings.DefaultLatitude.ToString();
            c_longtitude.Text = bot.globalSettings.DefaultLongitude.ToString();
        }

        private BotWindowData CreateBowWindowData(GlobalSettings _s, string name)
        {
            var stats = new Statistics();
            //stats.DirtyEvent +=
            //    () =>
            //        Console.Title =
            //            stats.GetTemplatedStats(
            //                session.Translation.GetTranslation(TranslationString.StatsTemplateString),
            //                session.Translation.GetTranslation(TranslationString.StatsXpTemplateString));

            return new BotWindowData(name, _s, new StateMachine(), stats, new StatisticsAggregator(stats),
                new WpfEventListener(), new ClientSettings(_s), new LogicSettings(_s));

        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            // NoButton Clicked! Let's hide our InputBox.
            InputBox.Visibility = System.Windows.Visibility.Collapsed;

            // Clear InputBox.
            InputTextBox.Text = String.Empty;
        }

        private void c_autoLevelPokemons_Checked(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.AutomaticallyLevelUpPokemon = (bool)(sender as CheckBox).IsChecked;
        }

        private void c_EvolveAllPokemonAboveIv_Checked(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.EvolveAllPokemonAboveIv = (bool)(sender as CheckBox).IsChecked;
        }

        private void c_EvolveAllPokemonWithEnoughCandy_Checked(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.EvolveAllPokemonWithEnoughCandy = (bool)(sender as CheckBox).IsChecked;
        }

        private void c_KeepPokemonsThatCanEvolve_Checked(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.KeepPokemonsThatCanEvolve = (bool)(sender as CheckBox).IsChecked;
        }

        private void c_PrioritizeIvOverCp_Checked(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.PrioritizeIvOverCp = (bool)(sender as CheckBox).IsChecked;
        }

        private void c_RenameOnlyAboveIv_Checked(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.RenameOnlyAboveIv = (bool)(sender as CheckBox).IsChecked;
        }

        private void c_RenamePokemon_Checked(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.RenamePokemon = (bool)(sender as CheckBox).IsChecked;
        }

        private void c_teleport_Checked(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.Teleport = (bool)(sender as CheckBox).IsChecked;
        }

        private void c_UseDiscoveryPathing_Checked(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.UseDiscoveryPathing = (bool)(sender as CheckBox).IsChecked;
        }

        private void c_EvolveAboveIvValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            float val = 0;
            if (float.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.EvolveAboveIvValue = val;
        }

        private void c_KeepMinCp_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            int val = 0;
            if (int.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.KeepMinCp = val;
        }

        private void c_KeepMinDuplicatePokemon_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            int val = 0;
            if (int.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.KeepMinDuplicatePokemon = val;
        }

        private void c_KeepMinIvPercentage_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            float val = 0;
            if (float.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.KeepMinIvPercentage = val;
        }

        private void c_MaxPokeballsPerPokemon_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            int val = 0;
            if (int.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.MaxPokeballsPerPokemon = val;
        }

        private static string BeautifyToNum(string text)
        {
            return text.Trim().Replace('.', ',').Replace(" ", "");
        }

        private void c_RenameTemplate_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.RenameTemplate = (sender as TextBox).Text;
        }

        private void c_altitude_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            double val = 0;
            if (double.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.DefaultAltitude = val;
        }

        private void c_latitude_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            double val = 0;
            if (double.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.DefaultLatitude = val;
        }

        private void c_longtitude_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            double val = 0;
            if (double.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.DefaultLongitude = val;
        }

        private void c_MinDelayBetweenSnipes_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            int val = 0;
            if (int.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.MinDelayBetweenSnipes = val;
        }

        private void c_MinPokeballsToSnipe_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            int val = 0;
            if (int.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.MinPokeballsToSnipe = val;
        }

        private void c_MinPokeballsWhileSnipe_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            int val = 0;
            if (int.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.MinPokeballsWhileSnipe = val;
        }

        private void c_MaxSpawnLocationOffset_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            int val = 0;
            if (int.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.MaxSpawnLocationOffset = val;
        }

        private void c_MaxTravelDistanceInMeters_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            int val = 0;
            if (int.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.MaxTravelDistanceInMeters = val;
        }

        private void c_WalkingSpeedInKilometerPerHour_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            double val = 0;
            if (double.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.WalkingSpeedInKilometerPerHour = val;
        }

        private void c_StartupWelcomeDelay_Checked(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.StartupWelcomeDelay = (bool)(sender as CheckBox).IsChecked;
        }

        private void c_SnipeAtPokestops_Checked(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.SnipeAtPokestops = (bool)(sender as CheckBox).IsChecked;
        }

        private void c_SnipeIgnoreUnknownIv_Checked(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.SnipeIgnoreUnknownIv = (bool)(sender as CheckBox).IsChecked;
        }

        private void c_UseTransferIvForSnipe_Checked(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.UseTransferIvForSnipe = (bool)(sender as CheckBox).IsChecked;
        }

        private void c_TotalAmountOfPokebalsToKeep_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            int val = 0;
            if (int.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.TotalAmountOfPokeballsToKeep = val;
        }

        private void c_TotalAmountOfPotionsToKeep_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            int val = 0;
            if (int.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.TotalAmountOfPotionsToKeep = val;
        }

        private void c_TotalAmountOfRevivesToKeep_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            int val = 0;
            if (int.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.TotalAmountOfRevivesToKeep = val;
        }

        private void c_TranslationLanguageCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.TranslationLanguageCode = (sender as TextBox).Text;
        }

        private void c_UpgradePokemonCpMinimum_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            int val = 0;
            if (int.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.UpgradePokemonCpMinimum = val;
        }

        private void c_UpgradePokemonIvMinimum_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            int val = 0;
            if (int.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.UpgradePokemonIvMinimum = val;
        }

        private void c_UsePokemonToNotCatchFilter_Checked(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.UsePokemonToNotCatchFilter = (bool)(sender as CheckBox).IsChecked;
        }
                

        private void c_UseLuckyEggsWhileEvolving_Checked(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.UseLuckyEggsWhileEvolving = (bool)(sender as CheckBox).IsChecked;
        }

        private void c_UseLuckyEggsMinPokemonAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            int val = 0;
            if (int.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.UseLuckyEggsMinPokemonAmount = val;
        }

        private void c_UseEggIncubators_Checked(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.UseEggIncubators = (bool)(sender as CheckBox).IsChecked;
        }

        private void c_TransferDuplicatePokemon_Checked(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.TransferDuplicatePokemon = (bool)(sender as CheckBox).IsChecked;
        }

        private void c_UseMasterBallBelowCatchProbability_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            double val = 0;
            if (double.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.UseMasterBallBelowCatchProbability = val;
        }

        private void c_UseUltraBallBelowCatchProbability_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            double val = 0;
            if (double.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.UseUltraBallBelowCatchProbability = val;
        }

        private void c_UseGreatBallBelowCatchProbability_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            double val = 0;
            if (double.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.UseGreatBallBelowCatchProbability = val;
        }

        private void c_UseUltraBallAboveIv_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            int val = 0;
            if (int.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.UseUltraBallAboveIv = val;
        }

        private void c_UseGreatBallAboveIv_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            int val = 0;
            if (int.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.UseGreatBallAboveIv = val;
        }

        private void c_HumanizeThrows_Checked(object sender, RoutedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            bot.globalSettings.HumanizeThrows = (bool)(sender as CheckBox).IsChecked;
        }


        private void c_ThrowAccuracyMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            double val = 0;
            if (double.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.ThrowAccuracyMin = val;
        }

        private void c_ThrowAccuracyMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            double val = 0;
            if (double.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.ThrowAccuracyMax = val;
        }

        private void c_ThrowSpinFrequency_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            double val = 0;
            if (double.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.ThrowSpinFrequency = val;
        }

        private void c_UseBerryMinCp_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            int val = 0;
            if (int.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.UseBerryMinCp = val;
        }

        private void c_UseBerryMinIv_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            float val = 0;
            if (float.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.UseBerryMinIv = val;
        }

        private void c_UseBerryBelowCatchProbability_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            double val = 0;
            if (double.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.UseBerryBelowCatchProbability = val;
        }

        private void c_TotalAmountOfBerriesToKeep_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            int val = 0;
            if (int.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.TotalAmountOfBerriesToKeep = val;
        }
        private void c_RecycleInventoryAtUsagePercentage_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bot == null || LoadingUi) return;
            double val = 0;
            if (double.TryParse(BeautifyToNum((sender as TextBox).Text), out val))
                bot.globalSettings.RecycleInventoryAtUsagePercentage = val;
        }
        #endregion

        private void pokeMap_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //New point coord
            double lat = 0.0;
            double lng = 0.0;

            var mousePos = e.GetPosition(pokeMap);
            //Getting real coordinates from mouse click
            var mapPos = pokeMap.FromLocalToLatLng((int)mousePos.X, (int)mousePos.Y);
            lat = mapPos.Lat;
            lng = mapPos.Lng;

            if (bot != null)
            {
                if (bot.Started)
                {
                    if (bot.ForceMoveMarker == null)
                    {
                        bot.ForceMoveMarker = new GMapMarker(mapPos)
                        {
                            Shape = Properties.Resources.force_move.ToImage(),
                            Offset = new Point(-24, -48),
                            ZIndex = int.MaxValue
                        };
                        pokeMap.Markers.Add(bot.ForceMoveMarker);
                    }
                    else
                    {
                        bot.ForceMoveMarker.Position = mapPos;
                    }
                    curSession.StartForceMove(lat, lng);
                }
                else
                {
                    bot.Lat = bot._lat = lat;
                    bot.Lng = bot._lng = lng;
                    bot.globalSettings.DefaultLatitude = lat;
                    bot.globalSettings.DefaultLongitude = lng;
                    DrawPlayerMarker();
                    UpdateCoordBoxes();
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            windowClosing = true;
            if (bot == null || LoadingUi) return;
            bot.globalSettings.StoreData(subPath + "\\" + bot.profileName);
            foreach (var _b in openedSessions.Values)
            {
                _b.Stop();
            }
        }

        private void c_AlwaysPrefferLongDistanceEgg_Checked(object sender, RoutedEventArgs e)
        {
            bot.globalSettings.AlwaysPrefferLongDistanceEgg = (bool)(sender as CheckBox).IsChecked;
        }
    }
}
