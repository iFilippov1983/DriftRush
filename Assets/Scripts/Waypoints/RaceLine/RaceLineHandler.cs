using RaceManager.Cars;
using System;
using System.Collections.Generic;

namespace RaceManager.Waypoints
{
    public class RaceLineHandler : IObserver<DriverProfile>
    {
        private RaceLine _raceLine;

        private float _trackProgress;
        private float _currentSpeed;
        private bool _handle;

        private Dictionary<float, RaceLineSegment> _segments = new Dictionary<float, RaceLineSegment>();

        public RaceLineHandler(WaypointTrack mainTrack, RaceLine raceLine, bool handle)
        {
            _handle = handle;

            if(!_handle)
                return;

            _raceLine = raceLine;
            _raceLine.SpawnSegments(mainTrack);

            MakeSegmentsDictionary();
        }

        #region Public Functions

        public void OnNext(DriverProfile profile)
        {
            if (!_handle)
                return;

            _trackProgress = profile.TrackProgress;
            _currentSpeed = profile.CarCurrentSpeed;

            HandleLine();
        }

        public void OnCompleted() => throw new NotImplementedException();
        public void OnError(Exception error) => throw error;

        #endregion

        #region Private Functions

        private void MakeSegmentsDictionary()
        {
            foreach (var segment in _raceLine.Segments)
            {
                _segments.Add(segment.DistanceFromStart, segment);
            }
        }

        private void HandleLine()
        { 
            
        }

        #endregion
    }
}
