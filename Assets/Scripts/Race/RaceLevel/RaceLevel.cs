using RaceManager.Waypoints;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Race
{
    public class RaceLevel : MonoBehaviour, IRaceLevel
    {
        [SerializeField] private RaceLine _raceLine;
        [SerializeField] private List<TrackConfiguration> _trackConfigurations = new List<TrackConfiguration>();

        [ShowInInspector, ReadOnly]
        private TrackConfiguration _configuration;

        public RaceLine RaceLine => _raceLine;
        public StartPoint[] StartPoints => _configuration.StartPoints;
        public WaypointTrack WaypointTrackMain => _configuration.WaypointTrackMain;
        public WaypointTrack WaypointTrackEven => _configuration.WaypointTrackEven;
        public WaypointTrack WaypointTrackOdd => _configuration.WaypointTrackOdd;
        public Vector3 FinishCamInitialPosition => _configuration.FinishCamInitialPosition;
        public List<TrackConfiguration> Configurations => _trackConfigurations;

        public void SetCurrentConfiguration(TrackConfiguration configuration) => _configuration = configuration;
    }
}