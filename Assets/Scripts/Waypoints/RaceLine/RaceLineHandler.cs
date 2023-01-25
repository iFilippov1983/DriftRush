using RaceManager.Cars;
using System;

namespace RaceManager.Waypoints
{
    public class RaceLineHandler : IObserver<DriverProfile>
    {
        private RaceLine _raceLine;
        private bool _handle;

        public RaceLineHandler(WaypointTrack mainTrack, RaceLine raceLine, bool handle)
        {
            _handle = handle;

            if(!_handle) return;

            _raceLine = raceLine;
            _raceLine.SpawnSegments(mainTrack);
        }

        public void OnNext(DriverProfile profile)
        {
            if (!_handle) return;

            _raceLine.OnSpeedChange?.Invoke(profile.CarCurrentSpeed);
            _raceLine.OnDistanceChange?.Invoke(profile.DistanceFromStart);
        }

        public void OnCompleted() => throw new NotImplementedException();
        public void OnError(Exception error) => throw error;
    }
}
