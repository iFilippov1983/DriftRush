using RaceManager.Tools;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Waypoints
{
    public class RaceLine : MonoBehaviour
    {
        private GameObject _segmentPrefab;

        [Tooltip("The distance offset wich will be used to check each line segment")]
        [SerializeField] private float _checkOffset = 10f;
        [Space]
        [SerializeField] private Color _baseColor = Color.blue;
        [SerializeField] private Color _warningColor = Color.red;
        [SerializeField] private float _segmentFadeSpeed = 1f;
        [SerializeField] private float _segmentColorTransitionSpeed = 1f;
        [Space]
        [SerializeField] private float _segmentSpawnInterval = 1f;
        [SerializeField] private float _observeFactor = 15f;

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

        [ShowInInspector, ReadOnly]
        private float ObserveDistance => _segmentSpawnInterval * _observeFactor;

        public Action<float> OnSpeedChange;
        public Action<float> OnDistanceChange;

        [Button]
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
                    checkOffset = _checkOffset                    
                });

                segment.OnDestroyAction += OnSegmentDestroy;

                OnSpeedChange += segment.CheckSpeed;
                OnDistanceChange += segment.CheckDistance;

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

        private void OnSegmentDestroy(RaceLineSegment segment)
        {
            OnSpeedChange -= segment.CheckSpeed;
            OnDistanceChange -= segment.CheckDistance;

            segment.OnDestroyAction -= OnSegmentDestroy;
        }
    }
}
