using RaceManager.Cars;
using System.Collections.Generic;
using System.Linq;
using RaceManager.Waypoints;
using System;

namespace RaceManager.Race
{
    public class InRacePositionsHandler : IDisposable
    {
        private List<WaypointsTracker> _waypointsTrackers;

        public void StartHandling(List<WaypointsTracker> waypointsTrackers)
        {
            _waypointsTrackers = waypointsTrackers;

            foreach (WaypointsTracker tracker in _waypointsTrackers)
                tracker.OnPassedWaypoint += OnPassedWaypoint;
        }

        private void OnPassedWaypoint(WaypointsTracker waypointTracker)
        {
            _waypointsTrackers = _waypointsTrackers
                .OrderByDescending(t => t.NumberOfWaypointsPassed)
                .ThenBy(t => t.TimeAtLastWaypoint)
                .ToList();

            int carPosition = _waypointsTrackers.IndexOf(waypointTracker) + 1;
            waypointTracker.SetInRacePosition(carPosition);
        }

        public void Dispose()
        {
            foreach (WaypointsTracker tracker in _waypointsTrackers)
                tracker.OnPassedWaypoint -= OnPassedWaypoint;
        }
    }
}
