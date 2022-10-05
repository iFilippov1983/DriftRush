using RaceManager.Tools;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

#pragma warning disable 649
namespace RaceManager.Cars
{
    public class CarAIControl : MonoBehaviour
    {
        private const float SteerSensitivity = 0.01f;                     //(0-1)How sensitively the AI uses steering input to turn to the desired direction
        private const float BrakeSensitivity = 1f;                        //(0-1)How sensitively the AI uses the brake to reach the current desired speed
        private const float AccelSensitivity = 1f;                        //(0-1)How sensitively the AI uses the accelerator to reach the current desired speed

        //[SerializeField]
        private DriverSettings _driverSettings;
        [SerializeField] private Transform _target;                        // 'target' the target object to aim for.
        [SerializeField] private bool _isAvoidingCars = true;
        [SerializeField] private bool _isDriving = false;                          // whether the AI is currently actively driving or stopped.

        private CarController _carController; 
        private SphereCollider _sphereCollider;
        private float _currentSpeed;
        private float _spherecastRadius;
        private Vector3 _avoidanceVectorLerped;

        private const float SpherecastMaxDistance = 20f; //12
        private const float DesireToGetTheWaypoint = 6f; //6
        private const float AvoidanceLerpFactor = 1f;

        public bool isDriving => _isDriving;
        [ReadOnly]
        public bool PlayerDriving = false;
        [ReadOnly]
        public float DesiredSpeed;
        public float CruiseSpeed => _driverSettings.CruiseSpeed;

        //private void OnEnable()
        //{
        //    _carController = GetComponent<CarController>();
        //    _sphereCollider = GetComponent<SphereCollider>();
        //    _spherecastRadius = _sphereCollider.radius;

        //    if (PlayerDriving)
        //        DesiredSpeed = _driverSettings.CruiseSpeed;
        //    else
        //    {
        //        DesiredSpeed = _carController.MaxSpeed * Random.Range(_driverSettings.CruiseSpeedPercentMin, _driverSettings.CruiseSpeedPercentMax);
        //        Debug.Log($"{gameObject.name} speed: {DesiredSpeed}");
        //    }


        //    StartEngine();
        //}

        public void Initialize(DriverSettings driverSettings)
        {
            _driverSettings = driverSettings;
            _carController = GetComponent<CarController>();
            _sphereCollider = GetComponent<SphereCollider>();
            _spherecastRadius = _sphereCollider.radius;


            if (PlayerDriving)
                DesiredSpeed = _driverSettings.CruiseSpeed;
            else
            {
                DesiredSpeed = _carController.MaxSpeed * Random.Range(_driverSettings.CruiseSpeedPercentMin, _driverSettings.CruiseSpeedPercentMax);
                Debug.Log($"{gameObject.name} speed: {DesiredSpeed}");
            }
        }

        private void FixedUpdate()
        {
            MoveCar();
            HandbrakeIfNeeded();
        }

        private void MoveCar()
        {
            if (!_isDriving)
            {
                _carController.Move(0, 0, 0f, 0f);
            }
            else
            {
                _currentSpeed = DesiredSpeed;
                float accelBrakeSensitivity = (_currentSpeed < _carController.CurrentSpeed)
                                                  ? BrakeSensitivity
                                                  : AccelSensitivity;

                // decide the actual amount of accel/brake input to achieve desired speed.
                float accel = Mathf.Clamp((_currentSpeed - _carController.CurrentSpeed) * accelBrakeSensitivity, -1, 1);

                // calculate the local-relative position of the target, to steer towards
                Vector3 localTarget = transform.InverseTransformPoint(_target.position);
                Vector3 vectorToTarget = _target.position - transform.position;
                vectorToTarget.Normalize();

                // work out the local angle towards the target
                float targetAngle;
                if (_isAvoidingCars)
                {
                    AvoidAICars(vectorToTarget, out vectorToTarget);
                    targetAngle = Vector3.SignedAngle(transform.forward, vectorToTarget, transform.up);
                    //targetAngle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.z) * Mathf.Rad2Deg;
                }
                else
                {
                    targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
                }

                // get the amount of steering needed to aim the car towards the target
                float steer = Mathf.Clamp(targetAngle * SteerSensitivity, -1, 1) * Mathf.Sign(_carController.CurrentSpeed);

                // feed input to the car controller.
                _carController.Move(steer, accel, accel, 0f);
            }
        }

        private void HandbrakeIfNeeded()
        {
            if (_target == null)
            {
                // Car should not be moving, use handbrake to stop
                _carController.Move(0, 0, -1f, 1f);
            }
        }

        private bool IsCarInFrontOfAICar(out Vector3 position, out Vector3 otherCarRightVector)
        {
            _sphereCollider.enabled = false;

            RaycastHit raycastHit;
            Physics.SphereCast(transform.position + transform.forward * 0.5f, _spherecastRadius, transform.forward, out raycastHit, SpherecastMaxDistance, 1 << LayerMask.NameToLayer(Layer.Car));

            _sphereCollider.enabled = true;

            if (raycastHit.collider != null)
            {
                Debug.DrawRay(transform.position, transform.forward * SpherecastMaxDistance, Color.red);

                position = raycastHit.collider.transform.position;
                otherCarRightVector = raycastHit.collider.transform.right;

                return true;
            }
            else
            {
                Debug.DrawRay(transform.position, transform.forward * SpherecastMaxDistance, Color.black);
            }

            position = Vector3.zero;
            otherCarRightVector = Vector3.zero;
            return false;
        }

        private void AvoidAICars(Vector3 vectorToTarget, out Vector3 newVectorToTarget)
        {
            if (IsCarInFrontOfAICar(out Vector3 otherCarPosition, out Vector3 otherCarRightVector))
            { 
                Vector3 avoidanceVector;
                avoidanceVector = Vector3.Reflect((otherCarPosition - transform.position).normalized, otherCarRightVector);

                float distanceTotarget = (_target.position - transform.position).magnitude;

                //As we get closer to waypoint the desire to reach the waypoint increases
                float driveToTargetInfluence = DesireToGetTheWaypoint / distanceTotarget;
                //Ensure that we  limit the value to between 30% and 100%
                driveToTargetInfluence = Mathf.Clamp(driveToTargetInfluence, 0.3f, 1f);

                float avoidanceInfluence = 1f - driveToTargetInfluence;

                _avoidanceVectorLerped = Vector3.Lerp(_avoidanceVectorLerped, avoidanceVector, Time.fixedDeltaTime * AvoidanceLerpFactor);

                newVectorToTarget = vectorToTarget * driveToTargetInfluence + _avoidanceVectorLerped * avoidanceInfluence;
                newVectorToTarget.Normalize();

                Debug.DrawRay(transform.position, avoidanceVector * SpherecastMaxDistance, Color.green);
                Debug.DrawRay(transform.position, newVectorToTarget * SpherecastMaxDistance, Color.yellow);
                return;
            }

            newVectorToTarget = vectorToTarget;
        }

        public void StopEngine()
        {
            _isDriving = false;
        }

        public void StartEngine()
        {
            _isDriving = true;
            _carController.StartMove();
        }
    }
}
