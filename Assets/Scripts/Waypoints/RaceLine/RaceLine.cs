﻿using RaceManager.Tools;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace RaceManager.Waypoints
{
    public class RaceLine : MonoBehaviour
    {
        [Serializable]
        public struct LinePoint
        {
            public float distanceFromStart;
            public float recomendedSpeed;
            public Vector3 position;
            public Vector3 direction;
        }

        private GameObject _segmentPrefab;

        [Tooltip("The distance offset wich will be used to check each line segment")]
        [SerializeField] private float _segmentCheckOffset = 10f;
        [Space]
        [SerializeField] private Color _baseColor = Color.blue;
        [SerializeField] private Color _warningColor = Color.red;
        [SerializeField] private Color _initialColor = Color.white;
        [Space]
        [SerializeField] private float _segmentColorTransitionDuration = 0.25f;
        [SerializeField] private float _segmentAlphaTransitionDuration = 0.2f;
        [Space]
        [SerializeField] private float _segmentSpawnInterval = 1f;
        [SerializeField] private int _segmentsMaxAmount = 50;
        [Space]
        [SerializeField] private Transform _linePointsParent;

        private float _currentDistance = 0;
        private int _currentIndex = 0;

        #region Minor variables

        private LinePoint m_NewLinePoint;

        #endregion

        private List<RaceLineSegment> _segments;
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

        public Subject<float> SpeedChange = new Subject<float>();
        public Subject<float> DistanceChange = new Subject<float>();
        public Subject<Unit> ShowLine = new Subject<Unit>();
        public Subject<Unit> RaceFinish = new Subject<Unit>();

        public void PrepareSelf(WaypointTrack mainTrack)
        {
            _segments = new List<RaceLineSegment>();
            _linePoints = new Dictionary<int, LinePoint>();

            var disArray = mainTrack.Distances;
            while (_currentDistance < disArray[disArray.Length - 1])
            {
                RoutePoint point = mainTrack.GetRoutePoint(_currentDistance);

                GameObject pointGo = new GameObject($"Point [index: {_currentIndex}; dist: {_currentDistance}]");
                pointGo.transform.position = point.position;
                pointGo.transform.rotation = Quaternion.LookRotation(point.direction);
                pointGo.transform.SetParent(_linePointsParent);

                LinePoint linePoint = new LinePoint()
                {
                    distanceFromStart = _currentDistance,
                    recomendedSpeed = GetRecomendedSpeed(mainTrack, _currentDistance),
                    position = point.position,
                    direction = point.direction
                };

                _linePoints.Add(_currentIndex, linePoint);

                if (_segments.Count <= _segmentsMaxAmount)
                {
                    GameObject segmentGo = Instantiate(SegmentPrefab, point.position, Quaternion.LookRotation(point.direction), transform);
                    segmentGo.name = $"Segment [{_currentIndex}]";

                    RaceLineSegment segment = segmentGo.GetComponent<RaceLineSegment>();
                    segment.DistanceFromStart = _currentDistance;
                    segment.CurrentIndex = _currentIndex;

                    segment.Initiallize(new RaceLineSegmentData()
                    {
                        recomendedSpeed = GetRecomendedSpeed(mainTrack, segment.DistanceFromStart),

                        colorTransitionDuration = _segmentColorTransitionDuration,
                        alphaTransitionDuration = _segmentAlphaTransitionDuration,

                        baseColor = _initialColor,
                        warningColor = _initialColor,
                        checkOffset = _segmentCheckOffset
                    });

                    segment.OnVisibilityChange
                        .Where(t => t.isVisible == false)
                        .Subscribe(t => 
                        {
                            MoveSegment(t.segment);
                        })
                        .AddTo(segment);

                    SpeedChange.Subscribe(s => segment.SpeedCheck(s)).AddTo(this);
                    DistanceChange.Subscribe(d => segment.DistanceCheck(d)).AddTo(this);
                    RaceFinish.Subscribe(_ => Destroy(segment.gameObject)).AddTo(this);
                    ShowLine.Subscribe(_ =>
                    {
                        segment.Initiallize(new RaceLineSegmentData()
                        {
                            recomendedSpeed = GetRecomendedSpeed(mainTrack, segment.DistanceFromStart),

                            colorTransitionDuration = _segmentColorTransitionDuration,
                            alphaTransitionDuration = _segmentAlphaTransitionDuration,

                            baseColor = _baseColor,
                            warningColor = _warningColor,
                            checkOffset = _segmentCheckOffset
                        });

                    }).AddTo(this);

                    _segments.Add(segment);
                }

                _currentDistance += _segmentSpawnInterval;
                _currentIndex++;
            }
        }

        private void MoveSegment(RaceLineSegment segment)
        {
            int newIndex = segment.CurrentIndex + _segments.Count;
            m_NewLinePoint = newIndex <= (_linePoints.Count - 1)
                ? _linePoints[newIndex]
                : _linePoints[0];

            segment.CurrentIndex = newIndex;
            segment.DistanceFromStart = m_NewLinePoint.distanceFromStart;
            segment.Transform.position = m_NewLinePoint.position;
            segment.Transform.rotation = Quaternion.LookRotation(m_NewLinePoint.direction);
            segment.RecomendedSpeed = m_NewLinePoint.recomendedSpeed;
        }

        private float GetRecomendedSpeed(WaypointTrack track, float distanceFromStart)
        {
            for (int i = 1; i < track.Distances.Length; i++)
            {
                if (track.Distances[i] > distanceFromStart || Mathf.Approximately(track.Distances[i], distanceFromStart))
                {
                    float speedDif = track.Nodes[i].recomendedSpeed - track.Nodes[i - 1].recomendedSpeed;
                    float distDif = distanceFromStart - track.Distances[i - 1];
                    float distFactor = distDif / (track.Distances[i] - track.Distances[i - 1]);
                    float value = track.Nodes[i - 1].recomendedSpeed + speedDif * distFactor;
                    return value;
                }
            }

            return 0.0f;
        }
    }
}
