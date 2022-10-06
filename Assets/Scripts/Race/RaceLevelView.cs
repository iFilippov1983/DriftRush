using RaceManager.Waypoints;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Race
{
    public class RaceLevelView : MonoBehaviour
    {
        [SerializeField] private StartPoint[] _startPoints;
        [SerializeField] private WaypointTrack _waypointTrack;

        public StartPoint[] StartPoints => _startPoints;
        public WaypointTrack WaypointTrack => _waypointTrack;
    }
}