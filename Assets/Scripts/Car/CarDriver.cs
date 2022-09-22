using UnityEngine;

public class CarDriver : MonoBehaviour 
{
    private const float Min_Turn_Amount = 20f;

    [SerializeField] private Rigidbody _motorRB;
    [SerializeField] private CarSettings _settings;
    private float _speed;
    private float _turnSpeed;

    private float _forwardAmount;
    private float _turnAmount;

    public Vector3 MotorPosition => _motorRB.position;
    public Vector3 AngularVelosity => _motorRB.angularVelocity;
    public float Speed => _speed;
    public float TurnSpeed => _turnSpeed;

    private void Start() 
    {
        _motorRB.transform.parent = null;
    }

    private void FixedUpdate()
    {
        MoveCar();
    }

    private void MoveCar()
    {
        if (_forwardAmount > 0)
        {
            // Accelerating
            _speed += _forwardAmount * _settings.acceleration * Time.deltaTime;
        }

        if (_forwardAmount < 0)
        {
            if (_speed > 0)
            {
                // Braking
                _speed += _forwardAmount * _settings.brakeSpeed * Time.deltaTime;
            }
            else
            {
                // Reversing
                _speed += _forwardAmount * _settings.reverseSpeed * Time.deltaTime;
            }
        }

        if (_forwardAmount == 0)
        {
            // Not accelerating or braking
            if (_speed > 0)
            {
                _speed -= _settings.idleSlowdown * Time.deltaTime;
            }
            if (_speed < 0)
            {
                _speed += _settings.idleSlowdown * Time.deltaTime;
            }
        }

        _speed = Mathf.Clamp(_speed, _settings.speedMin, _settings.speedMax);
        _motorRB.velocity = transform.forward * _speed;// * Time.deltaTime;
        //_motorRB.AddForce(_motorRB.transform.forward * _speed, ForceMode.Acceleration);

        if (_speed < 0)
        {
            // Going backwards, invert wheels
            _turnAmount = _turnAmount * -1f;
        }

        if (_turnAmount > 0 || _turnAmount < 0)
        {
            // Turning
            if ((_turnSpeed > 0 && _turnAmount < 0) || (_turnSpeed < 0 && _turnAmount > 0))
            {
                // Changing turn direction
                _turnSpeed = _turnAmount * Min_Turn_Amount;
            }
            _turnSpeed += _turnAmount * _settings.turnSpeedAcceleration * Time.deltaTime;
        }
        else
        {
            // Not turning
            if (_turnSpeed > 0)
            {
                _turnSpeed -= _settings.turnIdleSlowdown * Time.deltaTime;
            }
            if (_turnSpeed < 0)
            {
                _turnSpeed += _settings.turnIdleSlowdown * Time.deltaTime;
            }
            if (_turnSpeed > -1f && _turnSpeed < +1f)
            {
                // Stop turning
                _turnSpeed = 0f;
            }
        }

        float speedNormalized = _speed / _settings.speedMax;
        float invertSpeedNormalized = Mathf.Clamp(1 - speedNormalized, .75f, 1f);

        _turnSpeed = Mathf.Clamp(_turnSpeed, -_settings.turnSpeedMax, _settings.turnSpeedMax);
        _motorRB.angularVelocity = new Vector3(0, _turnSpeed * (invertSpeedNormalized * 1f) * Mathf.Deg2Rad, 0);

        if (transform.eulerAngles.x > 2 || transform.eulerAngles.x < -2 || transform.eulerAngles.z > 2 || transform.eulerAngles.z < -2)
        {
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
    }

    //private void OnCollisionEnter(Collision collision) {
    //    if (collision.gameObject.layer == GameHandler.SOLID_OBJECTS_LAYER) {
    //        speed = Mathf.Clamp(speed, 0f, 20f);
    //        //turnSpeed = Mathf.Clamp(turnSpeed, 0f, 20f);
    //    }
    //}

    public void SetInputs(float forwardAmount, float turnAmount) 
    {
        _forwardAmount = forwardAmount;
        _turnAmount = turnAmount;
    }

    public void ClearTurnSpeed() 
    {
        _turnSpeed = 0f;
    }

    public void SetTurnSpeedStraightly(float value)
    { 
        _turnSpeed = value;
    }

    public void SetSpeedMax(float speedMax) 
    {
        _settings.speedMax = speedMax;
    }

    public void SetTurnSpeedMax(float turnSpeedMax) 
    {
        _settings.turnSpeedMax = turnSpeedMax;
    }

    public void SetTurnSpeedAcceleration(float turnSpeedAcceleration) 
    {
        _settings.turnSpeedAcceleration = turnSpeedAcceleration;
    }

    public void StopCompletely() 
    {
        _speed = 0f;
        _turnSpeed = 0f;
        _motorRB.velocity = Vector3.zero;
        _motorRB.angularVelocity = Vector3.zero;
    }
}
