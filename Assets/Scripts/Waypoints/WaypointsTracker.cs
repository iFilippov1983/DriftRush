using RaceManager.Cars;
using RaceManager.Tools;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace RaceManager.Waypoints
{
    public class WaypointsTracker : MonoBehaviour
    {
        private DriverProfile _driverProfile;
        [SerializeField, ReadOnly]
        private WaypointTrack _waypointTrack;                  // A reference to the waypoint-based route we should follow
        [SerializeField] private float _lookAheadForTargetOffset = 15;   // The offset ahead along the route that the we will aim for
        [SerializeField] private float _lookAheadForTargetFactor = .1f; // A multiplier adding distance ahead along the route to aim for, based on current speed
        [SerializeField] private float _lookAheadForSpeedOffset = 15;   // The offset ahead only the route for speed adjustments (applied as the rotation of the waypoint target transform)
        [SerializeField] private float _lookAheadForSpeedFactor = .1f;  // A multiplier adding distance ahead along the route for speed adjustments

        private int _passedWaypointNumber = 0;
        private float _timeAtLastPassedWaypoint = 0;
        private int _numberOfPassedWaypoints = 0;
        private int _lapsCompleted = 0;
        private int _lapsToComplete;
        private int _carPosition;

        private bool _raceFinished;

        [SerializeField, ReadOnly]
        private Transform _target;
        private float _progressDistance;    // The progress round the route, used in smooth mode.
        private Vector3 _lastPosition;      // Used to calculate current speed (since we may not have a rigidbody component)
        private float _speed;               // current speed of this object (calculated from delta since last frame)

        private float _cashedDistance;
        private Vector3 _cashedPosition;

        public RoutePoint ProgressPoint { get; private set; }
        public float Progress => _progressDistance / _waypointTrack.AccumulateDistance;
        public float DistanceFromStart => _progressDistance;
        public int CarPosition => _carPosition;
        public int NumberOfWaypointsPassed => _numberOfPassedWaypoints;
        public float TimeAtLastWaypoint => _timeAtLastPassedWaypoint;

        public Action<WaypointsTracker> OnPassedWaypoint;

        private void Start()
        {
            // You can manually create a transform and assign it to this component *and* the AI,
            // then this component will update it, and the AI can read it.
            if (_target == null)
            {
                _target = new GameObject(name + " Waypoint Target").transform;
            }
            _progressDistance = 0;
            //_lapsToComplete = _waypointTrack.LapsToComplete;
        }

        public void Initialize(WaypointTrack waypointTrack, DriverProfile driverProfile)
        {
            _waypointTrack = waypointTrack;
            _driverProfile = driverProfile;
            _lapsToComplete = _waypointTrack.LapsToComplete;
            _progressDistance = 0;

            //if (_target == null)
            //{
            //    _target = new GameObject(name + " Waypoint Target").transform;
            //}
        }

        public void SetInRacePosition(int position) => _carPosition = position;

        public void ResetTargetToCashedValues()
        {
            _lastPosition = _cashedPosition;
            _progressDistance = _cashedDistance;
        }

        private void Update()
        {
            TrackWaypoints();
        }

        private void TrackWaypoints()
        {
            // determine the position we should currently be aiming for
            // (this is different to the current progress position, it is a a certain amount ahead along the route)
            // we use lerp as a simple way of smoothing out the speed over time.
            if (Time.deltaTime > 0)
            {
                _speed = Mathf.Lerp(_speed, (_lastPosition - transform.position).magnitude / Time.deltaTime, Time.deltaTime);
            }
            _target.position =
                _waypointTrack.GetRoutePoint(_progressDistance + _lookAheadForTargetOffset + _lookAheadForTargetFactor * _speed).position;
            _target.rotation =
                Quaternion.LookRotation(_waypointTrack.GetRoutePoint(_progressDistance + _lookAheadForSpeedOffset + _lookAheadForSpeedFactor * _speed).direction);

            // get our current progress along the route
            ProgressPoint = _waypointTrack.GetRoutePoint(_progressDistance);
            Vector3 progressDelta = ProgressPoint.position - transform.position;
            if (Vector3.Dot(progressDelta, ProgressPoint.direction) < 0)
            {
                _progressDistance += progressDelta.magnitude * 0.5f;
            }

            _lastPosition = transform.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            //if (_raceFinished)
            //    return;

            if (other.CompareTag(Tag.Waypoint))
            {
                Waypoint waypoint = other.GetComponent<Waypoint>();

                if (_passedWaypointNumber + 1 == waypoint.Number)
                {
                    _passedWaypointNumber = waypoint.Number;
                    _numberOfPassedWaypoints++;
                    _timeAtLastPassedWaypoint = Time.time;

                    if (waypoint.isFinishLine)
                    {
                        _passedWaypointNumber = 0;
                        _lapsCompleted++;

                        if (_lapsCompleted >= _lapsToComplete)
                        {
                            //_raceFinished = true;
                            _driverProfile.CarState.Value = CarState.Finished;
                        }
                    }

                    OnPassedWaypoint?.Invoke(this);
                }

                if (waypoint.isCheckpoint)
                { 
                    _cashedDistance = _progressDistance;
                    _cashedPosition = _lastPosition;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying && _waypointTrack != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, _target.position);
                Gizmos.DrawWireSphere(_waypointTrack.GetRoutePosition(_progressDistance), 1);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(_target.position, _target.position + _target.forward);
            }
        }
    }

}
