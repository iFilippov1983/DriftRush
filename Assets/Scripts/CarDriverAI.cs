using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CarDriver))]
public class CarDriverAI : MonoBehaviour
{
    private const float Reached_Target_Distance = 1f;//5
    private const float Stopping_Distance = 1f;//10
    private const float Stopping_Speed_Limit = 80f;
    private const float Stopping_Speed_Limit_Final = 15f;
    private const float Angle_Critical = 45f;
    private const float Reverse_Distance = 5f;
    private const float Stuck_Distance = 0.6f;
    private const float Max_Stuck_Time = 2f;

    [SerializeField] private Transform _targetPositionTransform;
    [SerializeField] private Transform _tranformToRespawn;
    private CarDriver _carDriver;
    private Vector3 _targetPosition;
    private bool _hasReachedTargetPosition;

    private Vector3 _lastPosition;
    private Vector3 _lastStuckPosition;
    private float _stuckTimer;
    private float _forwardAmount;
    private float _turnAmount;

    private bool _haveFinished;

    [ReadOnly]
    public bool manualTurn;

    private void Awake() 
    {
        _carDriver = GetComponent<CarDriver>();
        manualTurn = false;
        _haveFinished = false;
    }

    private void OnDrawGizmos()
    {
        var color = Gizmos.color;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, _targetPosition);
        Gizmos.color = color;
    }

    private void Update() 
    {
        SetTargetPosition(_targetPositionTransform.position);
        Drive();

        _lastPosition = transform.position;
    }

    private void Drive()
    {
        _forwardAmount = 0f;
        _turnAmount = 0f;

        float distanceToTarget = Vector3.Distance(transform.position, _targetPosition);
        if (distanceToTarget > Reached_Target_Distance)
        {
            // Still too far, keep going
            SetAmounts(distanceToTarget);
        }
        else
        {
            HandleStop();
        }

        HandleStuckSafety();
        _carDriver.SetInputs(_forwardAmount, _turnAmount);
    }

    private void SetAmounts(float distanceToTarget)
    {
        Vector3 dirToMovePosition = (_targetPosition - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, dirToMovePosition);

        _hasReachedTargetPosition = false;
        _forwardAmount = 1f;

        if (distanceToTarget < Stopping_Distance && _carDriver.GetSpeed() > Stopping_Speed_Limit)
        {
            // Within stopping distance and moving forward
            _forwardAmount = -1f;
        }

        ///-------------------

        float angleToDir = Vector3.SignedAngle(transform.forward, dirToMovePosition, Vector3.up);

        if (angleToDir < 0)
        {
            _turnAmount = -1f;
        }
        else
        {
            _turnAmount = 1f;
        }

        if (angleToDir >= Angle_Critical)
        {
            _turnAmount *= 2f;
        }

        if (manualTurn)
        {
            _turnAmount = 0.01f;
        }

        if (dot < 0)
        {
            // Target is behind
            if (distanceToTarget < Reverse_Distance)
            {
                _forwardAmount = -1f;
                _turnAmount *= -1f;
            }
        }

        #region Old
        //------------------

        //if (manualTurn)
        //{
        //    turnAmount = 0.5f + Time.deltaTime;
        //    //_carDriver.ClearTurnSpeed();
        //    _carDriver.SetTurnSpeedStraightly(turnAmount);
        //}
        //else
        //{
        //    float angleToDir = Vector3.SignedAngle(transform.forward, dirToMovePosition, Vector3.up);
        //    if (angleToDir < 0)
        //    {
        //        turnAmount = -1f;
        //    }
        //    else
        //    {
        //        turnAmount = 1f;
        //    }

        //    if (dot < 0)
        //    {
        //        // Target is behind
        //        if (distanceToTarget > Reverse_Distance)
        //        {
        //            // Too far to reverse
        //        }
        //        else
        //        {
        //            forwardAmount = -1f;
        //            turnAmount *= -1f;
        //        }
        //    }
        //}
        #endregion
    }

    private void HandleStop()
    {
        // Close enough
        _hasReachedTargetPosition = true;
        if (_carDriver.GetSpeed() > Stopping_Speed_Limit_Final)
        {
            // Hit the brakes!
            _forwardAmount = -1f;
        }
    }

    private void HandleStuckSafety()
    {
        if (!_hasReachedTargetPosition) 
        {
            if (Vector3.Distance(transform.position, _lastStuckPosition) < Stuck_Distance) 
            {
                _stuckTimer += Time.deltaTime;

                if (_stuckTimer > Max_Stuck_Time) 
                {
                    transform.position = _tranformToRespawn.position;
                    transform.rotation = _tranformToRespawn.rotation;
                    transform.rotation *= new Quaternion(0, -1f, 0f, 0f);
                    _stuckTimer = 0;
                    _lastStuckPosition = _lastPosition;
                }
            } 
            else 
            {
                _stuckTimer = 0;
                _lastStuckPosition = _lastPosition;
            }
        }
    }

    private void SetTargetPosition(Vector3 targetPosition) 
    {
        _targetPosition = targetPosition;

        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        _hasReachedTargetPosition = distanceToTarget < Reached_Target_Distance;
    }

    public void SetTarget(Transform tragetTansform)
    {
        _tranformToRespawn = _targetPositionTransform;
        _targetPositionTransform = tragetTansform;
    }

    public Action Finish()
    {
        return () => _haveFinished = true;
    }

    //public bool GetHasReachedMoveToPosition()
    //{
    //    return _hasReachedTargetPosition;
    //}

    //public void SetReachedTargetDistance(float reachedTargetDistance)
    //{
    //    Reached_Target_Distance = reachedTargetDistance;
    //}

    //public void SetReverseDistance(float reverseDistance)
    //{
    //    Reverse_Distance = reverseDistance;
    //}
}
