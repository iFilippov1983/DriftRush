using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CarDriver))]
public class CarDriverAI : MonoBehaviour
{
    [SerializeField] private Transform _targetPositionTransform;
    [SerializeField] private CarDriverSettings _settings;
    private Transform _tranformToRespawn;
    private CarDriver _carDriver;
    private Vector3 _targetPosition;
    private bool _hasReachedTargetPosition;

    private Vector3 _lastAngularVelosity;
    private Vector3 _lastPosition;
    private Vector3 _lastStuckPosition;
    private float _stuckTimer;
    private float _forwardAmount;
    private float _turnAmount;

    private bool _haveFinished;
    private bool _onFinalNode;

    private void Awake() 
    {
        _carDriver = GetComponent<CarDriver>();
        _haveFinished = false;
        _onFinalNode = false;
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

    private void FixedUpdate()
    {
        //SetSelfPosition();
    }

    private void SetSelfPosition()
    {
        transform.position = _carDriver.MotorPosition;

        Vector3 angVel = Vector3.Lerp(_carDriver.AngularVelosity, _lastAngularVelosity, 0.1f);
        Vector3 newAngularVector = Mathf.Approximately(angVel.y, 0f) ? Vector3.zero : angVel * _forwardAmount;
        transform.Rotate(0, newAngularVector.y, 0, Space.World);
        _lastAngularVelosity = newAngularVector;
    }

    private void Drive()
    {
        _forwardAmount = 0f;
        _turnAmount = 0f;

        float distanceToTarget = Vector3.Distance(transform.position, _targetPosition);
        if (distanceToTarget > _settings.ReachedTargetDistance && !_haveFinished)
        {
            _hasReachedTargetPosition = false;
            SetAmounts(distanceToTarget);
        }
        else
        {
            HandleStop(distanceToTarget);
        }

        HandleStuckSafety();
        _carDriver.SetInputs(_forwardAmount, _turnAmount);
    }

    private void SetAmounts(float distanceToTarget)
    {
        Vector3 dirToMovePosition = (_targetPosition - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, dirToMovePosition);
        float angleToDir = Vector3.SignedAngle(transform.forward, dirToMovePosition, Vector3.up);

        SetForwardAmount(distanceToTarget);
        SetTurnAmount(angleToDir);
        CheckIfTargetIsBehind(dot, distanceToTarget);

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

    private void SetForwardAmount(float distanceToTarget)
    {
        if (distanceToTarget >= 0)
        {
            _forwardAmount = 1f;
        }

        if (distanceToTarget < _settings.StoppingDistance && _carDriver.Speed > _settings.StoppingSpeedLimit)
        {
            // Within stopping distance and moving forward
            _forwardAmount = -1f;
        }
    }

    private void SetTurnAmount(float angleToDir)
    {
        _turnAmount = angleToDir < 0 ? -1f : 1f;

        if (Mathf.Abs(angleToDir) >= _settings.AngleCritical)
        {
            _turnAmount *= 2f;
        }
    }

    private void CheckIfTargetIsBehind(float dot, float distanceToTarget)
    {
        if (dot < 0 && distanceToTarget < _settings.ReverseDistance)
        {
            _forwardAmount = -1f;
            _turnAmount *= -1f;
        }
    }

    private void HandleStop(float distanceToTarget)
    {
        _hasReachedTargetPosition = true;

        if (_haveFinished)
        {
            GetAside(distanceToTarget);
            return;
        }

        if (_carDriver.Speed > _settings.StoppingSpeedLimitFinal)
        {
            // Hit the brakes!
            _forwardAmount = -1f;
        }
    }

    private async void GetAside(float distanceToTarget)
    {
        if (_onFinalNode)
            return;
        _onFinalNode = true;

        float moveTime = 2f;
        while (moveTime > 0)
        {
            SetAmounts(distanceToTarget);
            moveTime -= Time.deltaTime;
            await Task.Yield();

            if (moveTime <= 0)
            {
                _turnAmount = 0;
                _forwardAmount = 0;
                _carDriver.StopCompletely();
            }
        }
    }

    private void HandleStuckSafety()
    {
        if (!_hasReachedTargetPosition) 
        {
            if (Vector3.Distance(transform.position, _lastStuckPosition) < _settings.StuckDistance) 
            {
                _stuckTimer += Time.deltaTime;

                if (_stuckTimer > _settings.MaxStuckTime && _tranformToRespawn != null) 
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
        _hasReachedTargetPosition = distanceToTarget < _settings.ReachedTargetDistance;
    }

    public void SetTarget(Transform newTragetTansform)
    {
        _tranformToRespawn = _targetPositionTransform;
        _targetPositionTransform = newTragetTansform;

        $"New target: {newTragetTansform.gameObject.name}".Log();
    }

    public void Finish(Transform finalTarget)
    {
        $"FINISHED".Log(StringConsoleLog.Color.Red);
        SetTarget(finalTarget);
        _haveFinished = true;
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
