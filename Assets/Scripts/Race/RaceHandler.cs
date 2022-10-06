using Cinemachine;
using RaceManager.Root;
using RaceManager.Cars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RaceManager.Waypoints;

namespace RaceManager.Race
{
    internal class RaceHandler : Singleton<RaceHandler>
    {
        private Driver _playerDriver;
        private List<Driver> _drivers;

        private List<WaypointProgressTracker> _waypointsTrackers;

        public void StartHandle()
        {
            _drivers = RaceInitializer.Instance.Drivers;
            _playerDriver = _drivers.Find(d => d.DriverType == DriverType.Player);

            WaypointProgressTracker[] waypointsTrackesArray = FindObjectsOfType<WaypointProgressTracker>();
            _waypointsTrackers = new List<WaypointProgressTracker>(waypointsTrackesArray);

            foreach (WaypointProgressTracker tracker in _waypointsTrackers)
                tracker.OnPassedWaypoint += OnPassedWaypoint;
        }

        private void OnPassedWaypoint(WaypointProgressTracker waypointTracker)
        {
            _waypointsTrackers = _waypointsTrackers
                .OrderByDescending(t => t.NumberOfWaypointsPassed)
                .ThenBy(t => t.TimeAtLastWaypoint)
                .ToList();

            int carPosition = _waypointsTrackers.IndexOf(waypointTracker) + 1;
            waypointTracker.SetInRacePosition(carPosition);
        }

        private void OnDestroy()
        {
            foreach (WaypointProgressTracker tracker in _waypointsTrackers)
                tracker.OnPassedWaypoint -= OnPassedWaypoint;
        }
    }
}
