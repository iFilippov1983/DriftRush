using RaceManager.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Waypoints
{
    public class RaceLine : MonoBehaviour
    {
        [Serializable]
        public struct LinePoint
        {
            public float distanceFromStart;
            public Vector3 position;
            public Vector3 direction;
        }

        private GameObject _segmentPrefab;

        [Tooltip("The distance offset wich will be used to check each line segment")]
        [SerializeField] private float _segmentCheckOffset = 10f;
        [Space]
        [SerializeField] private Color _baseColor = Color.blue;
        [SerializeField] private Color _warningColor = Color.red;
        [SerializeField] private float _segmentFadeSpeed = 15f;
        [SerializeField] private float _segmentColorTransitionSpeed = 15f;
        [Space]
        [SerializeField] private float _segmentSpawnInterval = 1f;
        [SerializeField] private int _segmentsMaxAmount = 50;

        private float _currentDistance = 0;
        private int _currentIndex = 0;

        private Queue<RaceLineSegment> _segments;
        private Dictionary<int, LinePoint> _linePoints;

        private GameObject SegmentPrefab
        {
            get
            {
                if (_segmentPrefab == null)
                    _segmentPrefab = ResourcesLoader.LoadPrefab(ResourcePath.RaceLineSegmentPrefab);

                return _segmentPrefab;
            }
        }

        public Action<float> OnSpeedChange;
        public Action<float> OnDistanceChange;
        public Action OnRaceFinish;

        public void PrepareSelf(WaypointTrack mainTrack)
        {
            _segments = new Queue<RaceLineSegment>();
            _linePoints = new Dictionary<int, LinePoint>();

            var disArray = mainTrack.Distances;
            while (_currentDistance < disArray[disArray.Length - 1])
            {
                RoutePoint point = mainTrack.GetRoutePoint(_currentDistance);

                GameObject pointGo = Instantiate
                    (
                    new GameObject($"Point [index: {_currentIndex}; dist: {_currentDistance}]"),
                    point.position,
                    Quaternion.LookRotation(point.direction),
                    transform
                    );

                LinePoint linePoint = new LinePoint()
                {
                    distanceFromStart = _currentDistance,
                    position = point.position,
                    direction = point.direction
                };

                _linePoints.Add(_currentIndex, linePoint);

                if (_segments.Count <= _segmentsMaxAmount)
                {
                    GameObject segmentGo = Instantiate(SegmentPrefab, point.position, Quaternion.LookRotation(point.direction), transform);

                    RaceLineSegment segment = segmentGo.GetComponent<RaceLineSegment>();
                    segment.DistanceFromStart = _currentDistance;

                    segment.Initiallize(new RaceLineSegmentData()
                    {
                        recomendedSpeed = GetRecomendedSpeed(mainTrack, segment),
                        fadeSpeed = _segmentFadeSpeed,
                        colorTransitionSpeed = _segmentColorTransitionSpeed,
                        baseColor = _baseColor,
                        warningColor = _warningColor,
                        checkOffset = _segmentCheckOffset
                    });

                    segment.OnDestroyAction += SegmentDestroy;

                    //OnSpeedChange += segment.CheckSpeed; //Coroutine
                    //OnDistanceChange += segment.CheckDistance; //Coroutine
                    OnSpeedChange += segment.SpeedCheck; //Task
                    OnDistanceChange += segment.DistanceCheck; //Task
                    OnRaceFinish += () => Destroy(segment.gameObject);
                }

                _currentDistance += _segmentSpawnInterval;
                _currentIndex++;
            }
        }

        private float GetRecomendedSpeed(WaypointTrack track, RaceLineSegment segment)
        {
            TrackNode prev;
            TrackNode next;
            var distances = track.Distances;
            var waypoints = track.Waypoints;
            for (int i = 1; i < distances.Length; i++)
            {
                if (distances[i] > segment.DistanceFromStart || Mathf.Approximately(distances[i], segment.DistanceFromStart))
                {
                    prev = waypoints[i - 1].GetComponent<TrackNode>();
                    next = waypoints[i].GetComponent<TrackNode>();
                    float speedDif = next.recomendedSpeed - prev.recomendedSpeed;
                    float distDif = segment.DistanceFromStart - distances[i - 1];
                    float distFactor = distDif / (distances[i] - distances[i - 1]);
                    float value = prev.recomendedSpeed + speedDif * distFactor;
                    return value;
                }
            }

            return 0.0f;
        }

        private void SegmentDestroy(RaceLineSegment segment)
        {
            //OnSpeedChange -= segment.CheckSpeed;
            //OnDistanceChange -= segment.CheckDistance;
            OnSpeedChange -= segment.SpeedCheck;
            OnDistanceChange -= segment.DistanceCheck; 
            OnRaceFinish -= () => Destroy(segment.gameObject);

            segment.OnDestroyAction -= SegmentDestroy;
        }
    }
}
