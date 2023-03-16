using RaceManager.Waypoints;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Race
{
    [Serializable]
    public class TrackConfiguration : MonoBehaviour
    {
        [SerializeField] private StartPoint[] _startPoints;
        [SerializeField] private WaypointTrack _waypointTrackMain;
        [SerializeField] private WaypointTrack _waypointTrackEven;
        [SerializeField] private WaypointTrack _waypointTrackOdd;
        [Space]
        [SerializeField] private Transform _followCamInitialPoint;
        [SerializeField] private Transform _startCamInitialPoint;
        [SerializeField] private Transform _finishCamInitialPoint;
        [Space]
        [GUIColor(0f, 1f, 0f)]
        [SerializeField] private List<GameObject> _activeObjects;
        [GUIColor(1f, 0.6f, 0.4f)]
        [SerializeField] private List<GameObject> _inactiveObjects;

        public StartPoint[] StartPoints => _startPoints;
        public WaypointTrack WaypointTrackMain => _waypointTrackMain;
        public WaypointTrack WaypointTrackEven => _waypointTrackEven;
        public WaypointTrack WaypointTrackOdd => _waypointTrackOdd;
        public Vector3 FollowCamInitialPosition => _followCamInitialPoint.position;
        public Vector3 FinishCamInitialPosition => _finishCamInitialPoint.position;
        public List<GameObject> Actives => _activeObjects;
        public List<GameObject> Inactives => _inactiveObjects;
    }
}