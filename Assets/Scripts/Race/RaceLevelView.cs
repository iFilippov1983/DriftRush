using RaceManager.Waypoints;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Race
{
    public class RaceLevelView : MonoBehaviour
    {
        [SerializeField] private StartPoint[] _startPoints;
        [SerializeField] private WaypointTrack _waypointTrack;
        [SerializeField] private Finish _finishObject;

        public StartPoint[] StartPoints => _startPoints;
        public WaypointTrack WaypointTrack => _waypointTrack;
        public Finish Finish => _finishObject;
    }
}