using RaceManager.Waypoints;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Race
{
    public class RaceLevelView : MonoBehaviour, IRaceLevel
    {
        [SerializeField] private RaceLine _raceLine;
        [SerializeField] private StartPoint[] _startPoints;
        [SerializeField] private WaypointTrack _waypointTrackMain;
        [SerializeField] private WaypointTrack _waypointTrackEven;
        [SerializeField] private WaypointTrack _waypointTrackOdd;
        [Space]
        [SerializeField] private Transform _followCamInitialPoint;
        [SerializeField] private Transform _startCamInitialPoint;
        [SerializeField] private Transform _finishCamInitialPoint;

        public RaceLine RaceLine => _raceLine;
        public StartPoint[] StartPoints => _startPoints;
        public WaypointTrack WaypointTrackMain => _waypointTrackMain;
        public WaypointTrack WaypointTrackEven => _waypointTrackEven;
        public WaypointTrack WaypointTrackOdd => _waypointTrackOdd;
        public Vector3 FollowCamInitialPosition => _followCamInitialPoint.position;
        public Vector3 StartCamInitialPosition => _startCamInitialPoint.position;
        public Vector3 FinishCamInitialPosition => _finishCamInitialPoint.position;

        public List<TrackConfiguration> Configurations => throw new System.NotImplementedException();

        public void SetCurrentConfiguration(TrackConfiguration configuration)
        {
            throw new System.NotImplementedException();
        }
    }
}