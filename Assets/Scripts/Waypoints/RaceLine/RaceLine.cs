using RaceManager.Tools;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Waypoints
{
    public class RaceLine : MonoBehaviour
    {
        private GameObject _segmentPrefab;

        [SerializeField] private Color _normalColor = Color.blue;
        [SerializeField] private Color _awareColor = Color.yellow;
        [SerializeField] private Color _warningColor = Color.red;
        [SerializeField] private float _segmentSpawnInterval = 1;
        [SerializeField] private float _observeFactor = 15;

        private List<RaceLineSegment> _segments = new List<RaceLineSegment>();

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

        public Color NormalColor => _normalColor;
        public Color AwareColor => _awareColor;
        public Color WarningColor => _warningColor;
        public List<RaceLineSegment> Segments => _segments;
        [ShowInInspector, ReadOnly]
        public float ObserveDistance => _segmentSpawnInterval * _observeFactor;

        [Button]
        public void SpawnSegments(WaypointTrack mainTrack)
        {
            while (_currentDistance < mainTrack.Length)
            {
                RoutePoint point = mainTrack.GetRoutePoint(_currentDistance);
                GameObject go = Instantiate(SegmentPrefab, point.position, Quaternion.LookRotation(point.direction), transform);

                RaceLineSegment segment = go.GetComponent<RaceLineSegment>();
                segment.DistanceFromStart = _currentDistance;
                segment.RecomendedSpeed = GetRecomendedSpeed(mainTrack, segment);
                _segments.Add(segment);

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
                    Debug.Log(i);
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

        private void ClearLine()
        {
            if (_segments.Count < 1)
                return;

            foreach (var segment in _segments)
            {
                _segments.Remove(segment);
                Destroy(segment);
            }
        }

        private void OnDestroy()
        {
            ClearLine();
        }
    }
}
