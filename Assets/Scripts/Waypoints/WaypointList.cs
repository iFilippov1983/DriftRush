using System;
using UnityEngine;

namespace RaceManager.Waypoints
{
    [Serializable]
    public class WaypointList
    {
        public WaypointTrack track;
        public WaypointCircuit circuit;
        public Transform[] items = new Transform[0];
    }
}
