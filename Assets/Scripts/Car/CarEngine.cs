using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngine : MonoBehaviour, IObservable<CarEngine.MoveValues>, IDisposable 
{
    public struct MoveValues
    {
        public float Speed;
        public float TurnSpeed;
        public Vector3 MotorVelosity;
        public Vector3 MotorAngularVelosity;
    }

    private const float Min_Turn_Amount = 20f;
    private const string FinishLayer = "Finish";

    [SerializeField] private Rigidbody _motorRB;
    private List<IObserver<MoveValues>> _observersList = new List<IObserver<MoveValues>>();

    private float _speedMax = 10f;
    private float _speedCruising = 8f;
    private float _speedMin = -10f;
    private float _acceleration = 10f;
    private float _reverseSpeed = 5f;

    private float _brakeSpeed = 10f;
    private float _idleSlowdown = 15f;

    private float _turnSpeedMax = 20f;
    private float _turnSpeedAcceleration = 10f;
    private float _turnIdleSlowdown = 15f;
    private float _skidDrag = 1f;

    private float _speed;
    private float _speedMaxCurrent;
    private float _skidThresholdSpeed;
    private float _turnSpeed;

    private float _forwardAmount;
    private float _turnAmount;

    public float Speed => _speed;
    public float TurnSpeed => _turnSpeed;

    private void FixedUpdate()
    {
        MoveForward();
        Turn();
        Skidding();
        NotifyObservers();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void MoveForward()
    {
        if (_forwardAmount > 0)
        {
            // Accelerating
            _speed += _forwardAmount * _acceleration * Time.deltaTime;
        }

        if (_forwardAmount < 0)
        {
            if (_speed > 0)
            {
                // Braking
                _speed += _forwardAmount * _brakeSpeed * Time.deltaTime;
            }
            else
            {
                // Reversing
                _speed += _forwardAmount * _reverseSpeed * Time.deltaTime;
            }
        }

        if (_forwardAmount == 0)
        {
            // Not accelerating or braking
            if (_speed > 0)
            {
                _speed -= _idleSlowdown * Time.deltaTime;
            }
            if (_speed < 0)
            {
                _speed += _idleSlowdown * Time.deltaTime;
            }
        }

        _speed = Mathf.Clamp(_speed, _speedMin, _speedMaxCurrent);
        //_motorRB.velocity = transform.forward * _speed;
        _motorRB.AddForce(_motorRB.transform.forward * _speed, ForceMode.Acceleration);
    }

    private void Turn()
    {
        if (_speed < 0)
        {
            // Going backwards, invert wheels
            _turnAmount *= -1f;
        }

        if (_turnAmount > 0 || _turnAmount < 0)
        {
            // Turning
            if ((_turnSpeed > 0 && _turnAmount < 0) || (_turnSpeed < 0 && _turnAmount > 0))
            {
                // Changing turn direction
                _turnSpeed = _turnAmount * Min_Turn_Amount;
            }
            _turnSpeed += _turnAmount * _turnSpeedAcceleration * Time.deltaTime;
        }
        else
        {
            // Not turning
            if (_turnSpeed > 0)
            {
                _turnSpeed -= _turnIdleSlowdown * Time.deltaTime;
            }
            if (_turnSpeed < 0)
            {
                _turnSpeed += _turnIdleSlowdown * Time.deltaTime;
            }
            if (_turnSpeed > -1f && _turnSpeed < +1f)
            {
                // Stop turning
                _turnSpeed = 0f;
            }
        }

        float speedNormalized = _speed / _speedMaxCurrent;
        float invertSpeedNormalized = Mathf.Clamp(1 - speedNormalized, .75f, 1f);

        _turnSpeed = Mathf.Clamp(_turnSpeed, -_turnSpeedMax, _turnSpeedMax);
        _motorRB.angularVelocity = new Vector3(0, _turnSpeed * (invertSpeedNormalized * 1f) * Mathf.Deg2Rad, 0);

        //Stable in plain
        if (transform.eulerAngles.x > 2 || transform.eulerAngles.x < -2 || transform.eulerAngles.z > 2 || transform.eulerAngles.z < -2)
        {
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
    }

    private void Skidding()
    {
        _skidThresholdSpeed = (_speedCruising + _speedMax) / 2;
        bool turning = !Mathf.Approximately(_turnSpeed, 0f);
        if (_speed > _skidThresholdSpeed && turning)
        {
            "SKID".Log(StringConsoleLog.Color.Blue);
            _motorRB.AddForce(Vector3.right * _skidDrag, ForceMode.Acceleration);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(FinishLayer))
        {
            StopSlowly();
        }
    }

    public void SetInputs(float forwardAmount, float turnAmount) 
    {
        _forwardAmount = forwardAmount;
        _turnAmount = turnAmount;
    }

    public void SetValues(CarSettings carSettings)
    {
        _speedMax = carSettings.speedMax;
        _speedCruising = carSettings.speedCruising;
        _speedMin = carSettings.speedMin;
        _acceleration = carSettings.acceleration;
        _reverseSpeed = carSettings.reverseSpeed;
        _brakeSpeed = carSettings.brakeSpeed;
        _idleSlowdown = carSettings.idleSlowdown;
        _turnSpeedMax = carSettings.turnSpeedMax;
        _turnSpeedAcceleration = carSettings.turnSpeedAcceleration;
        _turnIdleSlowdown = carSettings.turnIdleSlowdown;
        _skidDrag = carSettings.skidDrag;
    }

    public void StopCompletely() 
    {
        _speed = 0f;
        _turnSpeed = 0f;
    }

    public void StopSlowly()
    {
        _speed = Mathf.Clamp(_speed, 0f, 20f);
        _turnSpeed = Mathf.Clamp(_turnSpeed, 0f, 20f);
    }

    public void StartCruise()
    {
        "Start cruise".Log(StringConsoleLog.Color.Yellow);
        StopAllCoroutines();
        StartCoroutine(InterpolateSpeed(_speedMax, _speedCruising));
    }

    public void StartAccelerate()
    {
        "Start acceleration".Log(StringConsoleLog.Color.Green);
        StopAllCoroutines();
        StartCoroutine(InterpolateSpeed(_speedCruising, _speedMax));
    }

    private IEnumerator InterpolateSpeed(float from, float to)
    {
        float persentage = 0f;
        while (persentage < 1)
        {
            persentage += Time.deltaTime;
            _speedMaxCurrent = Mathf.Lerp(from, to, persentage);
            yield return null;
        }
    }


    private void NotifyObservers()
    { 
        MoveValues moveValues = new MoveValues();
        moveValues.Speed = _speed;
        moveValues.TurnSpeed = _turnSpeed;
        moveValues.MotorVelosity = _motorRB.velocity;
        moveValues.MotorAngularVelosity = _motorRB.angularVelocity;

        foreach(var o in _observersList)
            o.OnNext(moveValues);
    }

    public IDisposable Subscribe(IObserver<MoveValues> observer)
    {
        _observersList.Add(observer);
        return this;
    }

    public void Dispose()
    {
        _observersList.Clear();
    }
}
