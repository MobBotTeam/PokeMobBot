#region using directives

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PoGo.PokeMobBot.Logic.Common;
using PoGo.PokeMobBot.Logic.Event;
using PoGo.PokeMobBot.Logic.Utils;
using PokemonGo.RocketAPI;

#endregion

namespace PoGo.PokeMobBot.Logic.State
{
    public class PositionCheckState : IState
    {
        private readonly InfoState _infoState;
        private readonly LocationUtils _locationUtils;
        private readonly ISettings _settings;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ITranslation _translation;
        private readonly Client _client;
        private readonly ILogicSettings _logicSettings;

        public PositionCheckState(InfoState infoState, LocationUtils locationUtils, ISettings settings, IEventDispatcher eventDispatcher, ITranslation translation, Client client, ILogicSettings logicSettings)
        {
            _infoState = infoState;
            _locationUtils = locationUtils;
            _settings = settings;
            _eventDispatcher = eventDispatcher;
            _translation = translation;
            _client = client;
            _logicSettings = logicSettings;
        }

        public async Task<IState> Execute(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var coordsPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Configs" +
                             Path.DirectorySeparatorChar + "Coords.ini";
            if (File.Exists(coordsPath))
            {
                var latLngFromFile = LoadPositionFromDisk();
                if (latLngFromFile != null)
                {
                    var distance = _locationUtils.CalculateDistanceInMeters(latLngFromFile.Item1, latLngFromFile.Item2, _settings.DefaultLatitude, _settings.DefaultLongitude);
                    var lastModified = File.Exists(coordsPath) ? (DateTime?)File.GetLastWriteTime(coordsPath) : null;
                    if (lastModified != null)
                    {
                        var hoursSinceModified = (DateTime.Now - lastModified).HasValue
                            ? (double?)((DateTime.Now - lastModified).Value.Minutes / 60.0)
                            : null;
                        if (hoursSinceModified != null && hoursSinceModified != 0)
                        {
                            var kmph = distance / 1000 / (double)hoursSinceModified;
                            if (kmph < 80) // If speed required to get to the default location is < 80km/hr
                            {
                                File.Delete(coordsPath);
                                _eventDispatcher.Send(new WarnEvent
                                {
                                    Message =
                                        _translation.GetTranslation(TranslationString.RealisticTravelDetected)
                                });
                            }
                            else
                            {
                                _eventDispatcher.Send(new WarnEvent
                                {
                                    Message =
                                        _translation.GetTranslation(TranslationString.NotRealisticTravel, kmph)
                                });
                            }
                        }
                        await Task.Delay(200, cancellationToken);
                    }
                }
            }

            _eventDispatcher.Send(new UpdatePositionEvent
            {
                Latitude = _client.CurrentLatitude,
                Longitude = _client.CurrentLongitude
            });

            _eventDispatcher.Send(new WarnEvent
            {
                Message = _translation.GetTranslation(TranslationString.WelcomeWarning, _client.CurrentLatitude, _client.CurrentLongitude),
                RequireInput = _logicSettings.StartupWelcomeDelay
            });

            return _infoState;
        }

        private Tuple<double, double> LoadPositionFromDisk()
        {
            if (
                File.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Configs" +
                            Path.DirectorySeparatorChar + "Coords.ini") &&
                File.ReadAllText(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Configs" +
                                 Path.DirectorySeparatorChar + "Coords.ini").Contains(":"))
            {
                var latlngFromFile =
                    File.ReadAllText(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Configs" +
                                     Path.DirectorySeparatorChar + "Coords.ini");
                var latlng = latlngFromFile.Split(':');
                if (latlng[0].Length != 0 && latlng[1].Length != 0)
                {
                    try
                    {
                        var latitude = Convert.ToDouble(latlng[0]);
                        var longitude = Convert.ToDouble(latlng[1]);

                        if (Math.Abs(latitude) <= 90 && Math.Abs(longitude) <= 180)
                        {
                            return new Tuple<double, double>(latitude, longitude);
                        }
                        _eventDispatcher.Send(new WarnEvent
                        {
                            Message = _translation.GetTranslation(TranslationString.CoordinatesAreInvalid)
                        });
                        return null;
                    }
                    catch (FormatException)
                    {
                        _eventDispatcher.Send(new WarnEvent
                        {
                            Message = _translation.GetTranslation(TranslationString.CoordinatesAreInvalid)
                        });
                        return null;
                    }
                }
            }

            return null;
        }
    }
}