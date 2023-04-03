using RaceManager.Waypoints;
using System.Collections.Generic;
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
        public List<TrackConfiguration> Configurations { get; }
        public void SetCurrentConfiguration(TrackConfiguration configuration);
    }
}