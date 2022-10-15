using RaceManager.Tools;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Waypoints
{
    public class WaypointTrack : MonoBehaviour
    {
        public bool MainTrack;
        public WaypointList waypointList = new WaypointList();
        public float editorVisualisationSubsteps = 100;
        public float minDistanceToReachWaypoint = 5f;

        [SerializeField] protected bool _smoothRoute = true;
        [SerializeField] protected Color _drawColor = Color.yellow;
        [SerializeField, Range(0.1f, 2f)] private float _nodeSphereSize = 0.25f;
        [SerializeField, ReadOnly] private int _lapsToComplete = 1; 
        protected Color _altColor;
        protected int _numPoints;
        protected Vector3[] _points;
        protected float[] _distances;
        protected float _accumulateDistance;

        //this being here will save GC allocs
        //catmull-rom spline point numbers
        protected int p0n;
        protected int p1n;
        protected int p2n;
        protected int p3n;

        //represents interpolated percentage
        protected float i;
        //catmull-rom spline points
        protected Vector3 P0;
        protected Vector3 P1;
        protected Vector3 P2;
        protected Vector3 P3;

        private GameObject _waypointPrefab;
        private List<Waypoint> _waypoints;

        public float Length { get; protected set; }
        public Transform[] Waypoints => waypointList.items;
        public List<Waypoint> WaypointsList => _waypoints;
        public float AccumulateDistance => _accumulateDistance;
        public Transform CurrentTargetWaypoint => waypointList.items[p2n];
        public Transform PreviouseTargetWaypoint => waypointList.items[p1n];
        public int LapsToComplete => _lapsToComplete;

        private void Awake()
        {
            if (Waypoints.Length > 1)
            {
                CachePositionsAndDistances();
            }
            _numPoints = Waypoints.Length;
        }

        private void Start()
        {
            SetWaypoints();
        }

        private void SetWaypoints()
        {
            if (!MainTrack)
                return;

            if (_distances.Length > 0)
            {
                _waypoints = new List<Waypoint>();
                _waypointPrefab = ResourcesLoader.LoadPrefab(ResourcePath.WaypointPrefab);
                for (int i = 0; i < _distances.Length; i++)
                {
                    RoutePoint point = GetRoutePoint(_distances[i]);
                    GameObject go = Instantiate(_waypointPrefab, point.position, Quaternion.LookRotation(point.direction));
                    go.transform.SetParent(transform, false);

                    var wp = go.GetComponent<Waypoint>();
                    wp.Number = i;
                    _waypoints.Add(wp);

                    if(i == _distances.Length - 2)
                        wp.isFinishLine = true;

                    if(i != 0)
                        _waypoints[i-1].NextWaypoint = wp;
                }

                for (int i = 0; i < _waypoints.Count; i++)
                {
                    foreach (var wp in _waypoints)
                    {
                        if (_waypoints[i].Number == wp.Number)
                            continue;
                        _waypoints[i].Subscribe(wp);
                    }
                }
            }
        }

        public RoutePoint GetRoutePoint(float dist)
        {
            // position and direction
            Vector3 p1 = GetRoutePosition(dist);
            Vector3 p2 = GetRoutePosition(dist + 0.1f);
            Vector3 delta = p2 - p1;
            return new RoutePoint(p1, delta.normalized);
        }

        public virtual Vector3 GetRoutePosition(float dist)
        {
            int point = 0;

            if (Length == 0)
            {
                Length = _distances[_distances.Length - 1];
            }

            dist = Mathf.Repeat(dist, Length);

            while (_distances[point] < dist)
            {
                ++point;
            }


            // get nearest two points,
            p1n = point - 1;
            if (p1n < 0)
                p1n = _points.Length - 1;
            p2n = point;

            // found point numbers, now find interpolation value between the two middle points
            i = Mathf.InverseLerp(_distances[p1n], _distances[p2n], dist);

            if (_smoothRoute)
            {
                // smooth catmull-rom calculation between the two relevant points

                // get indices for the surrounding 2 points, because
                // four points are required by the catmull-rom function
                p0n = point - 2;
                p3n = point + 1;


                if (p0n < 0)
                    p0n = 0;
                if (p3n > _points.Length - 1)
                    p3n = _points.Length - 1;

                P0 = _points[p0n];
                P1 = _points[p1n];
                P2 = _points[p2n];
                P3 = _points[p3n];

                return CatmullRom(P0, P1, P2, P3, i);
            }
            else
            {
                // simple linear lerp between the two points:

                p1n = ((point - 1) + _numPoints) % _numPoints;
                p2n = point;

                return Vector3.Lerp(_points[p1n], _points[p2n], i);
            }
        }

        protected Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float i)
        {
            // comments are no use here... it's the catmull-rom equation.
            // Un-magic this, lord vector!
            return 0.5f *
                   ((2 * p1) + (-p0 + p2) * i + (2 * p0 - 5 * p1 + 4 * p2 - p3) * i * i +
                    (-p0 + 3 * p1 - 3 * p2 + p3) * i * i * i);
        }

        protected virtual void CachePositionsAndDistances()
        {
            // transfer the position of each point and distances between points to arrays for
            // speed of lookup at runtime
            _points = new Vector3[Waypoints.Length];
            _distances = new float[Waypoints.Length];

            _accumulateDistance = 0;
            for (int i = 0; i < _points.Length; ++i)
            {
                Transform t1 = Waypoints[i];
                Transform t2;
                if (i < Waypoints.Length - 1)
                    t2 = Waypoints[i + 1];
                else
                    t2 = Waypoints[Waypoints.Length - 1];
                    
                if (t1 != null && t2 != null)
                {
                    Vector3 p1 = t1.position;
                    Vector3 p2 = t2.position;
                    _points[i] = Waypoints[i].position;
                    _distances[i] = _accumulateDistance;
                    _accumulateDistance += (p1 - p2).magnitude;
                }
            }
        }

        private void OnDrawGizmos()
        {
            DrawGizmos(false);
        }


        private void OnDrawGizmosSelected()
        {
            DrawGizmos(true);
        }

        protected virtual void DrawGizmos(bool selected)
        {
            waypointList.track = this;
            if (Waypoints.Length > 1)
            {
                _numPoints = Waypoints.Length;

                CachePositionsAndDistances();
                Length = _distances[_distances.Length - 1];

                _altColor = new Color(_drawColor.r, _drawColor.g, _drawColor.b, _drawColor.a / 2);
                Gizmos.color = selected ? _drawColor : _altColor;
                Vector3 prev = Waypoints[0].position;
                if (_smoothRoute)
                {
                    for (float dist = 0; dist < Length; dist += Length / editorVisualisationSubsteps)
                    {
                        dist++;
                        if(dist >= Length)
                            dist = Length;
                        Vector3 next = GetRoutePosition(dist);
                        Gizmos.DrawLine(prev, next);
                        prev = next;
                    }
                }
                else
                {
                    for (int n = 0; n < Waypoints.Length - 1; ++n)
                    {
                        Vector3 next = Waypoints[n + 1].position;
                        Gizmos.DrawLine(prev, next);
                        prev = next;
                    }
                }

                Gizmos.color = Color.black;
                foreach (var waypoint in Waypoints)
                    Gizmos.DrawSphere(waypoint.transform.position, _nodeSphereSize);
            }
        }
    }
}
