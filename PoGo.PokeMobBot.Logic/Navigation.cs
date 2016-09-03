#region using directives

#region using directives

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using GeoCoordinatePortable;
using PoGo.PokeMobBot.Logic.Utils;
using PoGo.PokeMobBot.Logic.Logging;
using PokemonGo.RocketAPI;
using POGOProtos.Networking.Responses;
using PoGo.PokeMobBot.Logic.State;
using PoGo.PokeMobBot.Logic.Event;

#endregion

// ReSharper disable RedundantAssignment

#endregion

namespace PoGo.PokeMobBot.Logic
{
    public delegate void UpdatePositionDelegate(double lat, double lng);

    public class Navigation
    {
        private const double SpeedDownTo = 10 / 3.6;
        private readonly Client _client;
        #region RandomizeWalkingSpeed
        //RandomizeWalkingSpeedSettings
        private static DateTime timeSinceLastWalkingSpeedRandomization = new DateTime(0);
        private static DateTime projectedNextWalkingSpeedRandomization = new DateTime(0);
        private double currentSpeedInKPH;
        private static Random rand = new Random();
        public double RandomizeWalkingSpeed(ISession session, double walkingSpeedKPH)
        {
            if (timeSinceLastWalkingSpeedRandomization.Ticks == 0 && projectedNextWalkingSpeedRandomization.Ticks == 0)
            {
                timeSinceLastWalkingSpeedRandomization = DateTime.Now;
                projectedNextWalkingSpeedRandomization = timeSinceLastWalkingSpeedRandomization.AddMinutes(session.LogicSettings.MinutesUntilRandomizeWalkingSpeed);
                currentSpeedInKPH = walkingSpeedKPH;
            }
            else if (projectedNextWalkingSpeedRandomization.Ticks < DateTime.Now.Ticks)
            {
                var oldSpeed = currentSpeedInKPH;
                currentSpeedInKPH = rand.NextDouble() *
                    (session.LogicSettings.MaxRandomizeWalkingSpeedInKph - session.LogicSettings.MinRandomizeWalkingSpeedInKph)
                    + session.LogicSettings.MinRandomizeWalkingSpeedInKph;
                timeSinceLastWalkingSpeedRandomization = DateTime.Now;
                projectedNextWalkingSpeedRandomization = timeSinceLastWalkingSpeedRandomization.AddMinutes(session.LogicSettings.MinutesUntilRandomizeWalkingSpeed);
                session.EventDispatcher.Send(new WalkingSpeedRandomizedEvent { OldSpeed = oldSpeed, NewSpeed = currentSpeedInKPH });
            }
            else
            {
                currentSpeedInKPH = walkingSpeedKPH;
            }
            return currentSpeedInKPH / 3.6;
        }
        #endregion

        public double FuzzyFactorBearing()
        {
            double maximum = -8.0f;
            double minimum = 8.0f;
            return rand.NextDouble() * (maximum - minimum) + minimum;
        }

        public double FuzzyFactorSpeed()
        {
            double maximum = -10.0f;
            double minimum = 5.0f;
            return rand.NextDouble() * (maximum - minimum) + minimum;
        }

        public Navigation(Client client, UpdatePositionDelegate updatePos)
        {
            _client = client;
            UpdatePositionEvent = updatePos;
        }

        public Navigation(Client client)
        {
            _client = client;
        }

        public async Task<PlayerUpdateResponse> HumanLikeWalking(ISession session, GeoCoordinate targetLocation,
            double walkingSpeedInKilometersPerHour, Func<Task<bool>> functionExecutedWhileWalking,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var baseSpeedInMetersPerSecond = session.LogicSettings.RandomizeWalkingSpeed ?
                                        RandomizeWalkingSpeed(session, walkingSpeedInKilometersPerHour) :
                                        walkingSpeedInKilometersPerHour / 3.6;

            var sourceLocation = new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude);
            LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation);
            // Logger.Write($"Distance to target location: {distanceToTarget:0.##} meters. Will take {distanceToTarget/speedInMetersPerSecond:0.##} seconds!", LogLevel.Info);

            var nextWaypointBearing = LocationUtils.DegreeBearing(sourceLocation, targetLocation);
            nextWaypointBearing += FuzzyFactorBearing();
            var nextWaypointDistance = baseSpeedInMetersPerSecond;
            var waypoint = LocationUtils.CreateWaypoint(sourceLocation, nextWaypointDistance, nextWaypointBearing);

            var Waypoints = new List<GeoCoordinate>();

            //Initial walking
            var requestSendDateTime = DateTime.Now;
            var result =
                await
                    _client.Player.UpdatePlayerLocation(waypoint.Latitude, waypoint.Longitude,
                        _client.Settings.DefaultAltitude);

            UpdatePositionEvent?.Invoke(waypoint.Latitude, waypoint.Longitude);

            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                var millisecondsUntilGetUpdatePlayerLocationResponse =
                    (DateTime.Now - requestSendDateTime).TotalMilliseconds;

                sourceLocation = new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude);
                var currentDistanceToTarget = LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation);

                if (currentDistanceToTarget < 40)
                {
                    if (baseSpeedInMetersPerSecond > SpeedDownTo)
                    {
                        //Logger.Write("We are within 40 meters of the target. Speeding down to 10 km/h to not pass the target.", LogLevel.Info);
                        baseSpeedInMetersPerSecond = SpeedDownTo;
                    }
                }

                var speedInMetersPerSecond = baseSpeedInMetersPerSecond * (1 + FuzzyFactorSpeed() / 100);
                nextWaypointDistance = Math.Min(currentDistanceToTarget,
                    millisecondsUntilGetUpdatePlayerLocationResponse / 1000 * speedInMetersPerSecond);
                nextWaypointBearing = LocationUtils.DegreeBearing(sourceLocation, targetLocation);
                nextWaypointBearing += FuzzyFactorBearing();
                waypoint = LocationUtils.CreateWaypoint(sourceLocation, nextWaypointDistance, nextWaypointBearing);

                requestSendDateTime = DateTime.Now;
                result =
                    await
                        _client.Player.UpdatePlayerLocation(waypoint.Latitude, waypoint.Longitude,
                            _client.Settings.DefaultAltitude);

                UpdatePositionEvent?.Invoke(waypoint.Latitude, waypoint.Longitude);


                if (functionExecutedWhileWalking != null)
                    await functionExecutedWhileWalking(); // look for pokemon
                await Task.Delay(500, cancellationToken);
            } while (LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation) >= 30);

            return result;
        }

        public async Task<PlayerUpdateResponse> HumanPathWalking(ISession session, GeoCoordinate targetLocation,
            double walkingSpeedInKilometersPerHour, Func<Task<bool>> functionExecutedWhileWalking,
            Func<Task<bool>> functionExecutedWhileWalking2,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            //PlayerUpdateResponse result = null;


            var baseSpeedInMetersPerSecond = session.LogicSettings.RandomizeWalkingSpeed ?
                                        RandomizeWalkingSpeed(session, walkingSpeedInKilometersPerHour) :
                                        walkingSpeedInKilometersPerHour / 3.6;

            var sourceLocation = new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude);
            double distanceToTarget = LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation);
            Logger.Write($"Distance to target location: {distanceToTarget:0.##} meters. Will take {distanceToTarget / baseSpeedInMetersPerSecond:0.##} seconds!", LogLevel.Debug);

            var nextWaypointBearing = LocationUtils.DegreeBearing(sourceLocation, targetLocation);
            nextWaypointBearing += FuzzyFactorBearing();
            var nextWaypointDistance = baseSpeedInMetersPerSecond;
            var waypoint = LocationUtils.CreateWaypoint(sourceLocation, nextWaypointDistance, nextWaypointBearing,
                Convert.ToDouble(sourceLocation.Altitude, CultureInfo.InvariantCulture));

            //Initial walking

            var requestSendDateTime = DateTime.Now;
            var result =
                await
                    _client.Player.UpdatePlayerLocation(waypoint.Latitude, waypoint.Longitude, waypoint.Altitude);

            UpdatePositionEvent?.Invoke(waypoint.Latitude, waypoint.Longitude);

            do
            {
                if (RuntimeSettings.BreakOutOfPathing == true)
                    if (RuntimeSettings.lastPokeStopCoordinate.Latitude.Equals(targetLocation.Latitude) &&
                        RuntimeSettings.lastPokeStopCoordinate.Longitude.Equals(targetLocation.Latitude))
                    {
                        RuntimeSettings.BreakOutOfPathing = true;
                        break;
                    }

                cancellationToken.ThrowIfCancellationRequested();

                var millisecondsUntilGetUpdatePlayerLocationResponse =
                    (DateTime.Now - requestSendDateTime).TotalMilliseconds;

                sourceLocation = new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude);
                var currentDistanceToTarget = LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation);

                //if (currentDistanceToTarget < 40)
                //{
                //    if (speedInMetersPerSecond > SpeedDownTo)
                //    {
                //        //Logger.Write("We are within 40 meters of the target. Speeding down to 10 km/h to not pass the target.", LogLevel.Info);
                //        speedInMetersPerSecond = SpeedDownTo;
                //    }
                //}

                var speedInMetersPerSecond = baseSpeedInMetersPerSecond * (1 + FuzzyFactorSpeed() / 100);
                nextWaypointDistance = Math.Min(currentDistanceToTarget,
                    millisecondsUntilGetUpdatePlayerLocationResponse / 1000 * speedInMetersPerSecond);
                nextWaypointBearing = LocationUtils.DegreeBearing(sourceLocation, targetLocation);
                nextWaypointBearing += FuzzyFactorBearing();
                waypoint = LocationUtils.CreateWaypoint(sourceLocation, nextWaypointDistance, nextWaypointBearing);

                requestSendDateTime = DateTime.Now;
                result =
                    await
                        _client.Player.UpdatePlayerLocation(waypoint.Latitude, waypoint.Longitude,
                            waypoint.Altitude);

                UpdatePositionEvent?.Invoke(waypoint.Latitude, waypoint.Longitude);

                if (functionExecutedWhileWalking != null)
                    await functionExecutedWhileWalking(); // look for pokemon & hit stops
                if (functionExecutedWhileWalking2 != null)
                    await functionExecutedWhileWalking2();

                await Task.Delay(300, cancellationToken);
            } while (LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation) >= 2 && RuntimeSettings.BreakOutOfPathing == false);

            return result;
        }

        public async Task Teleport(GeoCoordinate targetLocation)
        {
            await _client.Player.UpdatePlayerLocation(
                targetLocation.Latitude,
                targetLocation.Longitude,
                _client.Settings.DefaultAltitude);

            UpdatePositionEvent?.Invoke(targetLocation.Latitude, targetLocation.Longitude);
        }

        public event UpdatePositionDelegate UpdatePositionEvent;
        /*
        public async Task<PlayerUpdateResponse> HumanLikeWalking(GeoCoordinate targetLocation,
            double walkingSpeedInKilometersPerHour, Func<Task<bool>> functionExecutedWhileWalking,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var speedInMetersPerSecond = walkingSpeedInKilometersPerHour / 3.6;

            var sourceLocation = new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude);
            LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation);
            // Logger.Write($"Distance to target location: {distanceToTarget:0.##} meters. Will take {distanceToTarget/speedInMetersPerSecond:0.##} seconds!", LogLevel.Info);

            var nextWaypointBearing = LocationUtils.DegreeBearing(sourceLocation, targetLocation);
            var nextWaypointDistance = speedInMetersPerSecond;
            var waypoint = LocationUtils.CreateWaypoint(sourceLocation, nextWaypointDistance, nextWaypointBearing);

            //Initial walking
            var requestSendDateTime = DateTime.Now;
            var result =
                await
                    _client.Player.UpdatePlayerLocation(waypoint.Latitude, waypoint.Longitude,
                        _client.Settings.DefaultAltitude);

            UpdatePositionEvent?.Invoke(waypoint.Latitude, waypoint.Longitude);

            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                var millisecondsUntilGetUpdatePlayerLocationResponse =
                    (DateTime.Now - requestSendDateTime).TotalMilliseconds;

                sourceLocation = new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude);
                var currentDistanceToTarget = LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation);

                if (currentDistanceToTarget < 40)
                {
                    if (speedInMetersPerSecond > SpeedDownTo)
                    {
                        //Logger.Write("We are within 40 meters of the target. Speeding down to 10 km/h to not pass the target.", LogLevel.Info);
                        speedInMetersPerSecond = SpeedDownTo;
                    }
                }

                nextWaypointDistance = Math.Min(currentDistanceToTarget,
                    millisecondsUntilGetUpdatePlayerLocationResponse / 1000 * speedInMetersPerSecond);
                nextWaypointBearing = LocationUtils.DegreeBearing(sourceLocation, targetLocation);
                waypoint = LocationUtils.CreateWaypoint(sourceLocation, nextWaypointDistance, nextWaypointBearing);

                requestSendDateTime = DateTime.Now;
                result =
                    await
                        _client.Player.UpdatePlayerLocation(waypoint.Latitude, waypoint.Longitude,
                            _client.Settings.DefaultAltitude);

                UpdatePositionEvent?.Invoke(waypoint.Latitude, waypoint.Longitude);


                if (functionExecutedWhileWalking != null)
                    await functionExecutedWhileWalking(); // look for pokemon
                await Task.Delay(500, cancellationToken);
            } while (LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation) >= 30);

            return result;        
        }
        */


        //BACKUP OF HUMAN WALKING
    }
}