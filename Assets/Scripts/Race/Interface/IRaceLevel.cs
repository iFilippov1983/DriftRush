using RaceManager.Waypoints;
using UnityEngine;

namespace RaceManager.Race
{
    public interface IRaceLevel
    {
        public RaceLine RaceLine { get; }
        public StartPoint[] StartPoints { get; }
        public WaypointTrack WaypointTrackMain { get; }
        public WaypointTrack WaypointTrackEven { get; }
        public WaypointTrack WaypointTrackOdd { get; }
        public Vector3 FinishCamInitialPosition { get; }
    }
}