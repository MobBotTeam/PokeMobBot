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
using PokemonGo.RocketAPI.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        //string Async Queues
        Queue<Tuple<string, Color>> logQueue = new Queue<Tuple<string, Color>>();
        Queue<NewMapObject> markerQueue = new Queue<NewMapObject>();
        Dictionary<string, GMapMarker> mapMarkers = new Dictionary<string, GMapMarker>();

        GMapMarker forceMoveMarker;
        GMapMarker playerMarker;

        public MainWindow()
        {
            InitializeComponent();
            InitWindowsComtrolls();
            InitializeMap();
            botWindow = this;

            LogWorker();
            MarkersWorker();

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
             
        void InitBots()
        {            
            Logger.SetLogger(new WpfLogger(LogLevel.Info), subPath);

            foreach (var item in Directory.GetDirectories(subPath))
            {
                if (item != subPath + "\\Logs")
                {
                    initBot(GlobalSettings.Load(item), item);
                }
            }
        } 


        public void ReceiveMsg(string msgType, ISession session, params object[] objData)
        {
            switch (msgType)
            {
                case "log":
                    PushNewConsoleRow(session, (string)objData[0], (Color)objData[1]);
                    break;
                default:
                    break;
            }
        }

        private void PushNewConsoleRow(ISession session, string rowText, Color rowColor)
        {
            if (openedSessions.ContainsKey(session))
            {
                openedSessions[session].log.Add(Tuple.Create(rowText, rowColor));
                if (bot == openedSessions[session])
                {                    
                    logQueue.Enqueue(Tuple.Create(rowText, rowColor));
                }
            }            
        }


        private async void MarkersWorker()
        {
            while (!windowClosing)
            {
                if (markerQueue.Count > 0)
                {
                    var newMapObj = markerQueue.Dequeue();
                    switch (newMapObj.oType)
                    {
                        case "ps":
                            if (!mapMarkers.ContainsKey(newMapObj.uid))
                            {
                                var icn = newMapObj.oName != "lured" ? Properties.Resources.pstop : Properties.Resources.pstop_lured;
                                GMapMarker marker = new GMapMarker(new PointLatLng(newMapObj.lat, newMapObj.lng))
                                {
                                    Shape = icn.ToImage()
                                };
                                pokeMap.Markers.Add(marker);
                                mapMarkers.Add(newMapObj.uid, marker);
                            }
                            break;
                        case "pm_rm":
                            if (mapMarkers.ContainsKey(newMapObj.uid))
                            {
                                pokeMap.Markers.Remove(mapMarkers[newMapObj.uid]);
                            }
                            break;
                        case "pm":
                            if (!mapMarkers.ContainsKey(newMapObj.uid))
                            {
                                CreatePokemonMarker(newMapObj.oName, newMapObj.lat, newMapObj.lng, newMapObj.uid);
                            }
                            break;
                        default:
                            break;
                    }

                }
                await Task.Delay(10);
            }
        }

        public void CreatePokemonMarker(string oName, double lat, double lng, string uid)
        {
            PokemonId pokemon = (PokemonId)Enum.Parse(typeof(PokemonId), oName);

            GMapMarker marker = new GMapMarker(new PointLatLng(lat, lng))
            {
                Shape = pokemon.ToImage(),
                Offset = new Point(-15, -30),
                ZIndex = 10
            };
            pokeMap.Markers.Add(marker);
            mapMarkers.Add(uid, marker);
        }

        private async void LogWorker()
        {
            while (!windowClosing)
            {
                if (logQueue.Count > 0)
                {
                    var t = logQueue.Dequeue();
                    consoleBox.AppendParagraph(t.Item1, t.Item2);
                }
                await Task.Delay(10);
            }
        }
        

        private class BotWindowData
        {
            public List<Tuple<string, Color>> log = new List<Tuple<string, Color>>();
            public Dictionary<string, GMapMarker> mapMarkers = new Dictionary<string, GMapMarker>();
            public StateMachine machine = null;
            public Statistics stats = null;
            public StatisticsAggregator aggregator = null;
            public WpfEventListener listener = null;
            public ClientSettings settings = null;
            public LogicSettings logic = null;

            public Label runTime;
            public Label level;

            private DispatcherTimer timer;
            private TimeSpan ts;

            public double Lat = 55.43213;
            public double Lng = 37.633987;
            public bool gotNewCoord = false;
            public bool moveRequired = false;
            public double _lat, _lng;
            public double _latStep = 0, _lngStep = 0;

            public BotWindowData(StateMachine sm, Statistics st, StatisticsAggregator sa, WpfEventListener wel, ClientSettings cs, LogicSettings l)
            {
                machine = sm;
                stats = st;
                aggregator = sa;
                listener = wel;
                settings = cs;
                logic = l;

                ts = new TimeSpan();
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Tick += delegate(object o, EventArgs args)
                {
                    ts.Add(new TimeSpan(0, 0, 1));
                    runTime.Content = ts.ToString();
                };
            }

            public void timerStart() => timer?.Start();

            public void timerStop() => timer?.Stop();
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

        }

        private void loginBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {

        }

        private void proxyUriBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void useProxyChb_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void proxyLoginBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void proxyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {

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
            initBot(settings, input);
            // Clear InputBox.
            InputTextBox.Text = String.Empty;
        }

        private void initBot(GlobalSettings settings, string profileName = "Unknown")
        {
            var newBot = CreateBowWindowData(settings);

            var session = new Session(newBot.settings, newBot.logic);
            session.Client.ApiFailure = new ApiFailureStrategy(session);

            session.EventDispatcher.EventReceived += evt => newBot.listener.Listen(evt, session);
            session.EventDispatcher.EventReceived += evt => newBot.aggregator.Listen(evt, session);

            session.Navigation.UpdatePositionEvent +=
                (lat, lng) => session.EventDispatcher.Send(new UpdatePositionEvent { Latitude = lat, Longitude = lng });

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

            newBot.level = lbLevel;
            newBot.runTime = lvRuntime;

            botPanel.Children.Add(botGrid);


            bStart.Click += delegate(object o, RoutedEventArgs args)
            {
                newBot.machine.AsyncStart(new VersionCheckState(), session);
                if (session.LogicSettings.UseSnipeLocationServer)
                    SnipePokemonTask.AsyncStart(session);
            };

            rec.MouseLeftButtonDown += delegate (object o, MouseButtonEventArgs args)
            {
                this.curSession = session;
                foreach (var item in botPanel.GetLogicalChildCollection<Rectangle>())
                {
                    if (item != o)
                        item.Fill = new SolidColorBrush(Color.FromArgb(255, 97, 97, 97));
                    else
                        item.Fill = new SolidColorBrush(Color.FromArgb(255, 97, 97, 225));
                }
            };
        }

        private BotWindowData CreateBowWindowData(GlobalSettings _s)
        {
            var stats = new Statistics();
            //stats.DirtyEvent +=
            //    () =>
            //        Console.Title =
            //            stats.GetTemplatedStats(
            //                session.Translation.GetTranslation(TranslationString.StatsTemplateString),
            //                session.Translation.GetTranslation(TranslationString.StatsXpTemplateString));

            return new BotWindowData(new StateMachine(), stats, new StatisticsAggregator(stats), 
                new WpfEventListener(), new ClientSettings(_s), new LogicSettings(_s));

        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            // NoButton Clicked! Let's hide our InputBox.
            InputBox.Visibility = System.Windows.Visibility.Collapsed;

            // Clear InputBox.
            InputTextBox.Text = String.Empty;
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

            if (forceMoveMarker == null)
            {
                forceMoveMarker = new GMapMarker(mapPos)
                {
                    Shape = Properties.Resources.force_move.ToImage(),
                    Offset = new Point(-24, -48),
                    ZIndex = int.MaxValue
                };
                pokeMap.Markers.Add(forceMoveMarker);
            }
            else
            {
                forceMoveMarker.Position = mapPos;
            }
        }
    }
}
