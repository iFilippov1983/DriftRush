using RaceManager.Tools;
using System;
using UnityEngine;

namespace RaceManager.Waypoints
{
    public class RaceLine : MonoBehaviour
    {
        private GameObject _segmentPrefab;

        [Tooltip("The distance offset wich will be used to check each line segment")]
        [SerializeField] private float _segmentCheckOffset = 10f;
        [Space]
        [SerializeField] private Color _baseColor = Color.blue;
        [SerializeField] private Color _warningColor = Color.red;
        [SerializeField] private float _segmentFadeSpeed = 1f;
        [SerializeField] private float _segmentColorTransitionSpeed = 1f;
        [Space]
        [SerializeField] private float _segmentSpawnInterval = 1f;

        private float _currentDistance = 0;

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

        public void SpawnSegments(WaypointTrack mainTrack)
        {
            var disArray = mainTrack.Distances;
            while (_currentDistance < disArray[disArray.Length - 1])
            {
                RoutePoint point = mainTrack.GetRoutePoint(_currentDistance);
                GameObject go = Instantiate(SegmentPrefab, point.position, Quaternion.LookRotation(point.direction), transform);

                RaceLineSegment segment = go.GetComponent<RaceLineSegment>();
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

                //OnSpeedChange += segment.CheckSpeed;
                //OnDistanceChange += segment.CheckDistance;
                OnSpeedChange += segment.SpeedCheck;
                OnDistanceChange += segment.DistanceCheck;

                _currentDistance += _segmentSpawnInterval;
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

            segment.OnDestroyAction -= SegmentDestroy;
        }
    }
}
