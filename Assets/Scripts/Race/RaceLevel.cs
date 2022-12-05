using RaceManager.Waypoints;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Race
{
    public class RaceLevel : MonoBehaviour, IRaceLevel
    {
        [SerializeField] private List<TrackConfiguration> _trackConfigurations = new List<TrackConfiguration>();

        private TrackConfiguration _configuration;

        public StartPoint[] StartPoints => _configuration.StartPoints;
        public WaypointTrack WaypointTrackMain => _configuration.WaypointTrackMain;
        public WaypointTrack WaypointTrackEven => _configuration.WaypointTrackEven;
        public WaypointTrack WaypointTrackOdd => _configuration.WaypointTrackOdd;
        public Vector3 FollowCamInitialPosition => _configuration.FollowCamInitialPosition;
        public Vector3 StartCamInitialPosition => _configuration.StartCamInitialPosition;
        public Vector3 FinishCamInitialPosition => _configuration.FinishCamInitialPosition;
        public List<TrackConfiguration> Configurations => _trackConfigurations;

        public void SetCurrentConfiguration(TrackConfiguration configuration) => _configuration = configuration;
    }
}