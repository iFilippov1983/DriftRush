using RaceManager.Cars;
using RaceManager.Root;
using System;

namespace RaceManager.Waypoints
{
    public class RaceLineHandler
    {
        private RaceLine _raceLine;
        private bool _handle;

        public RaceLineHandler(WaypointTrack mainTrack, RaceLine raceLine, bool handle)
        {
            _handle = handle;

            if(!_handle) return;

            _raceLine = raceLine;
            _raceLine?.PrepareSelf(mainTrack);
        }

        public void StartHandling()
        {
            if(!_handle) return;

            _raceLine?.ShowLine?.OnNext();
        }

        public void StopHandling()
        {
            if (_handle)
            {
                _handle = false;
                _raceLine.RaceFinish?.OnNext();
            }
        }

        public void UpdateDataFrom(DriverProfile profile)
        {
            if (!_handle) return;

            _raceLine.SpeedChange?.OnNext(profile.CarCurrentSpeed);
            _raceLine.DistanceChange?.OnNext(profile.DistanceFromStart);
        }
    }
}
