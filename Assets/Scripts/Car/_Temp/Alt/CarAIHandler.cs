using RaceManager.Tools;
using RaceManager.Waypoints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RaceManager.Alt
{
    public class CarAIHandler : MonoBehaviour
    {
        public enum AIMode { FollowPlayer, FollowWaypoints }

        public AIMode aIMode;
        public bool isAvoidingCars = true;

        private const float SteerAngleThreshold = 45f;
        private const float MinSpeedWhenTurn = 0.05f;

        private Vector3 _targetPosition = Vector3.zero;
        private Transform _targetTransform = null;

        private WaypointTrack _waypointTrack;
        private Transform _currentWaypointTransform;
        private int _currentWaypointIndex;
        private float _distanceToWaypoint;


        private CarMovementController _carController;

        private void Awake()
        {
            _currentWaypointIndex = 0;
            _carController = GetComponent<CarMovementController>();
            _waypointTrack = FindObjectOfType<WaypointTrack>();
        }


        private void FixedUpdate()
        {
            Vector3 inputVector = Vector3.zero;

            switch (aIMode)
            {
                case AIMode.FollowPlayer:
                    FollowPlayer();
                    break;

                case AIMode.FollowWaypoints:
                    FollowWaypointsTrack();
                    break;

            }

            inputVector.x = TurnTowardTarget();
            inputVector.z = ApplyThrottleOrBrake(inputVector.x);

            _carController.SetInputVector(inputVector);
        }

        private void FollowPlayer()
        {
            if (_targetTransform == null)
                _targetTransform = GameObject.FindGameObjectWithTag(Tag.Player).transform;

            if (_targetTransform != null)
                _targetPosition = _targetTransform.position;
        }

        private void FollowWaypointsTrack()
        {
            if (_currentWaypointIndex >= _waypointTrack.WaypointsList.Count)
                return;

            if (_currentWaypointTransform == null)
                _currentWaypointTransform = FindClosestWaypoint();

            if (_currentWaypointTransform != null)
            {
                _targetTransform = _currentWaypointTransform;
                _targetPosition = _currentWaypointTransform.position;

                _distanceToWaypoint = (_targetPosition - transform.position).magnitude;
                if (_distanceToWaypoint <= _waypointTrack.minDistanceToReachWaypoint && _currentWaypointIndex < _waypointTrack.WaypointsList.Count - 1)
                {
                    var currentWaypoint = _waypointTrack.WaypointsList[_currentWaypointIndex];
                    _currentWaypointTransform = currentWaypoint.NextWaypoint.transform;
                    _currentWaypointIndex++;
                }
            }
        }

        private Transform FindClosestWaypoint()
        {
            Waypoint wp = _waypointTrack.WaypointsList
                .OrderBy(w => Vector3.Distance(transform.position, w.transform.position))
                .FirstOrDefault();

            _currentWaypointIndex = _waypointTrack.WaypointsList.IndexOf(wp);

            return wp.transform;
        }

        private float TurnTowardTarget()
        {
            Vector3 vectorTarget = _targetTransform.position - transform.position;
            vectorTarget.Normalize();

            float angleToTarget = Vector3.SignedAngle(transform.forward, vectorTarget, Vector3.up);


            //We want to turn car AMAP if the angle is greater than Threshold angle
            float steerAmount = angleToTarget / SteerAngleThreshold;

            steerAmount = Mathf.Clamp(steerAmount, -1f, 1f);
            steerAmount = Mathf.Lerp(steerAmount * 0.3f, steerAmount, 1 / _distanceToWaypoint);

            return steerAmount;
        }

        private float ApplyThrottleOrBrake(float inputX)
        {
            return (1 + MinSpeedWhenTurn) - Mathf.Abs(inputX) / 1f;
        }

        private void OnDrawGizmos()
        {
            Color color = Gizmos.color;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _targetPosition);
        }
    }
}

