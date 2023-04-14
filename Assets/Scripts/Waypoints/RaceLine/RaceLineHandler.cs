using RaceManager.Cars;
using RaceManager.Root;
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
            _raceLine.PrepareSelf(mainTrack);
        }

        public void StartHandling()
        {
            if(!_handle) return;

            _raceLine.ShowLine?.OnNext();
        }

        public void StopHandling()
        {
            _handle = false;
            _raceLine.RaceFinish?.OnNext();
        }

        public void OnNext(DriverProfile profile)
        {
            if (!_handle) return;

            _raceLine.SpeedChange?.OnNext(profile.CarCurrentSpeed);
            _raceLine.DistanceChange?.OnNext(profile.DistanceFromStart);
        }

        public void OnCompleted() => throw new NotImplementedException();
        public void OnError(Exception error) => throw error;
    }
}
