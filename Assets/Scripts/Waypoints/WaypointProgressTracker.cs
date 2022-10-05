using RaceManager.Cars;
using Sirenix.OdinInspector;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Waypoints
{
    public class WaypointProgressTracker : MonoBehaviour
    {
        [SerializeField] private WaypointTrack _track;                  // A reference to the waypoint-based route we should follow
        [SerializeField] private float _lookAheadForTargetOffset = 5;   // The offset ahead along the route that the we will aim for
        [SerializeField] private float _lookAheadForTargetFactor = .1f; // A multiplier adding distance ahead along the route to aim for, based on current speed
        [SerializeField] private float _lookAheadForSpeedOffset = 10;   // The offset ahead only the route for speed adjustments (applied as the rotation of the waypoint target transform)
        [SerializeField] private float _lookAheadForSpeedFactor = .2f;  // A multiplier adding distance ahead along the route for speed adjustments

        //[ShowInInspector, ReadOnly]
        [SerializeField]
        private Transform _target;
        private float _progressDistance;    // The progress round the route, used in smooth mode.
        private Vector3 _lastPosition;      // Used to calculate current speed (since we may not have a rigidbody component)
        private float _speed;               // current speed of this object (calculated from delta since last frame)

        public RoutePoint ProgressPoint { get; private set; }
        public float Progress => _progressDistance;

        //private void Start()
        //{
        //    // You can manually create a transform and assign it to this component *and* the AI,
        //    // then this component will update it, and the AI can read it.
        //    if (_target == null)
        //    {
        //        _target = new GameObject(name + " Waypoint Target").transform;
        //    }
        //    _progressDistance = 0;
        //}

        public void Initialize(WaypointTrack waypointTrack)
        { 
            _track = waypointTrack;
            if (_target == null)
            {
                _target = new GameObject(name + " Waypoint Target").transform;
            }
            _progressDistance = 0;
        }

        private void Update()
        {
            // determine the position we should currently be aiming for
            // (this is different to the current progress position, it is a a certain amount ahead along the route)
            // we use lerp as a simple way of smoothing out the speed over time.
            if (Time.deltaTime > 0)
            {
                _speed = Mathf.Lerp(_speed, (_lastPosition - transform.position).magnitude / Time.deltaTime, Time.deltaTime);
            }
            _target.position =
                _track.GetRoutePoint(_progressDistance + _lookAheadForTargetOffset + _lookAheadForTargetFactor * _speed).position;
            _target.rotation =
                Quaternion.LookRotation(_track.GetRoutePoint(_progressDistance + _lookAheadForSpeedOffset + _lookAheadForSpeedFactor * _speed).direction);

            // get our current progress along the route
            ProgressPoint = _track.GetRoutePoint(_progressDistance);
            Vector3 progressDelta = ProgressPoint.position - transform.position;
            if (Vector3.Dot(progressDelta, ProgressPoint.direction) < 0)
            {
                _progressDistance += progressDelta.magnitude * 0.5f;
            }

            _lastPosition = transform.position;
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying && _track != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, _target.position);
                Gizmos.DrawWireSphere(_track.GetRoutePosition(_progressDistance), 1);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(_target.position, _target.position + _target.forward);
            }
        }
    }
}
