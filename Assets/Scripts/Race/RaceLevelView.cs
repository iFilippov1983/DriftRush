using RaceManager.Waypoints;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Race
{
    public class RaceLevelView : MonoBehaviour
    {
        [SerializeField] private StartPoint[] _startPoints;
        [SerializeField] private WaypointTrack _waypointTrackMain;
        [SerializeField] private WaypointTrack _waypointTrackEven;
        [SerializeField] private WaypointTrack _waypointTrackOdd;

        public StartPoint[] StartPoints => _startPoints;
        public WaypointTrack WaypointTrackMain => _waypointTrackMain;
        public WaypointTrack WaypointTrackEven => _waypointTrackEven;
        public WaypointTrack WaypointTrackOdd => _waypointTrackOdd;
    }
}