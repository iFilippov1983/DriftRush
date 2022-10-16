using RaceManager.Tools;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

#pragma warning disable 649
namespace RaceManager.Cars
{
    public class CarAI : MonoBehaviour
    {
        private const float SteerSensitivity = 1f;              //(0-1)How sensitively the AI uses steering input to turn to the desired direction
        private const float BrakeSensitivity = 1f;              //(0-1)How sensitively the AI uses the brake to reach the current desired speed
        private const float AccelSensitivity = 1f;              //(0-1)How sensitively the AI uses the accelerator to reach the current desired speed
                                                                //If angle to target is grater then this value car turns as much as possible

        //[SerializeField, ReadOnly]
        //private CarConfig _carConfig;
        [SerializeField] private Transform _target;             // 'target' the target object to aim for.
        [SerializeField] private bool _isDriving = false;       // whether the AI is currently actively driving or stopped.

        private Car _car;
        private SphereCollider _sphereCollider;
        private float _currentSpeed;
        private float _spherecastRadius;
        private Vector3 _avoidanceVectorLerped;

        private float _randomPerlin;
        private float _avoidOtherCarTime;
        private float _avoidOtherCarFactor;
        private float _avoidPathOffset;

        [Title("Avoidance settings")]
        [SerializeField] private bool _isAvoidingCars = true;
        [SerializeField] private float _castMaxDistance = 12f; //12
        [SerializeField] private float _desireToGetTheWaypoint = 6f; //6
        [SerializeField] private float _avoidanceLerpFactor = 1f; //1

        private float _criticalSteeAngle = 45f;
        private float _accelerationWanderAmount = 0.01f;
        private float _accelerationWanderSpeed = 1f;
        private float _lateralWanderDistance = 0.5f;
        private float _lateralWanderSpeed = 1f;

        public bool isDriving => _isDriving;
        [ReadOnly]
        public bool PlayerDriving = false;
        [ReadOnly]
        public float DesiredSpeed;
        public void StopAvoiding() => _isAvoidingCars = false;
        public Transform Target => _target;

        //private void Start()
        //{
        //    _car = GetComponent<Car>();
        //    _sphereCollider = GetComponent<SphereCollider>();
        //    _spherecastRadius = _sphereCollider.radius;
        //    _spherecastRadius = _sphereCollider.radius * 0.5f;
        //    _randomPerlin = Random.value * 100;

        //    _criticalSteeAngle = _car.CarConfig.MaxSteerAngle;

        //    if (PlayerDriving)
        //        DesiredSpeed = _car.CarConfig.CruiseSpeed;
        //    else
        //    {
        //        DesiredSpeed = _car.CarConfig.MaxSpeed * Random.Range(_car.CarConfig.CruiseSpeedPercentMin, _car.CarConfig.CruiseSpeedPercentMax);
        //        Debug.Log($"{gameObject.name} speed: {DesiredSpeed}");
        //    }
        //}

        public void Initialize()
        {
            _car = GetComponent<Car>();
            _sphereCollider = GetComponent<SphereCollider>();
            _spherecastRadius = _sphereCollider.radius;
            _spherecastRadius = _sphereCollider.radius * 0.5f;
            _randomPerlin = Random.value * 100;

            _criticalSteeAngle = _car.CarConfig.MaxSteerAngle;

            if (PlayerDriving)
                DesiredSpeed = _car.CarConfig.CruiseSpeed;
            else
            {
                DesiredSpeed = _car.CarConfig.MaxSpeed * Random.Range(_car.CarConfig.CruiseSpeedPercentMin, _car.CarConfig.CruiseSpeedPercentMax);
                //Debug.Log($"{gameObject.name} speed: {DesiredSpeed}");
            }
        }

        private void Update()
        {
            MoveCar();
            HandbrakeIfNeeded();
        }

        private void MoveCar()
        {
            if (isDriving)
            {
                _currentSpeed = DesiredSpeed;
                float accel = CalculateAcceleration();
                float steer = CalculateSteering();

                _car.UpdateControls(steer, accel, false);
            }
            else
            {
                _car.UpdateControls(0, 0, true);
            }
        }

        private float CalculateAcceleration()
        {
            float accelBrakeSensitivity = (_currentSpeed < _car.CurrentSpeed)
                                                      ? BrakeSensitivity
                                                      : AccelSensitivity;

            // decide the actual amount of accel/brake input to achieve desired speed.
            float accel = Mathf.Clamp((_currentSpeed - _car.CurrentSpeed) * accelBrakeSensitivity, -1, 1);

            //acceleration and way aline wander for more realistic AI behaviour
            accel *= (1 - _accelerationWanderAmount) +
                     (Mathf.PerlinNoise(Time.time * _accelerationWanderSpeed, _randomPerlin) * _accelerationWanderAmount);

            return accel;
        }

        private float CalculateSteering()
        {
            if (_target == null)
                return 0;

            Vector3 vectorToTarget = _target.position - transform.position;
            vectorToTarget.Normalize();

            if (_isAvoidingCars)
                AvoidAICars(vectorToTarget, out vectorToTarget);

            //Vector3 position = MakeWanderPositionOffsetFromTarget();
            //vectorToTarget = Vector3.Lerp(vectorToTarget, position, _avoidanceLerpFactor * Time.fixedDeltaTime);

            float targetAngle = Vector3.SignedAngle(transform.forward, vectorToTarget, transform.up);
            float steer = targetAngle / _criticalSteeAngle;

            //steer = Mathf.Clamp(steer, -1f, 1f);
            steer = Mathf.Clamp(steer * SteerSensitivity, -1f, 1f) * Mathf.Sign(_car.CurrentSpeed);
            return steer;
        }

        private Vector3 MakeWanderPositionOffsetFromTarget()
        {
            if (_target == null)
                return transform.position;

            Vector3 offsetTargetPos = _target.position;

            // if are we currently taking evasive action to prevent being stuck against another car:
            if (Time.time < _avoidOtherCarTime)
            {
                // slow down if necessary (if we were behind the other car when collision occured)
                DesiredSpeed *= _avoidOtherCarFactor;

                // and veer towards the side of our path-to-target that is away from the other car
                offsetTargetPos += _target.right * _avoidPathOffset;
            }
            else
            {
                // no need for evasive action, we can just wander across the path-to-target in a random way,
                // which can help prevent AI from seeming too uniform and robotic in their driving
                offsetTargetPos += _target.right *
                                   (Mathf.PerlinNoise(Time.time * _lateralWanderSpeed, _randomPerlin) * 2 - 1) *
                                   _lateralWanderDistance;
            }

            return offsetTargetPos;
        }

        private void HandbrakeIfNeeded()
        {
            if (_target == null)
            {
                _car.UpdateControls(0, 0, true);
            }
        }

        private bool IsCarInFrontOfAICar(out Vector3 position, out Vector3 otherCarRightVector)
        {
            _sphereCollider.enabled = false;

            RaycastHit raycastHit;
            Physics.SphereCast(transform.position + transform.forward * 0.5f, _spherecastRadius, transform.forward, out raycastHit, _castMaxDistance, 1 << LayerMask.NameToLayer(Layer.Car));

            _sphereCollider.enabled = true;

            if (raycastHit.collider != null)
            {
                Debug.DrawRay(transform.position, transform.forward * _castMaxDistance, Color.red);
                g_point = raycastHit.point;

                position = raycastHit.collider.transform.position;
                otherCarRightVector = raycastHit.collider.transform.right;

                return true;
            }
            else
            {
                Debug.DrawRay(transform.position, transform.forward * _castMaxDistance, Color.black);
            }

            position = Vector3.zero;
            otherCarRightVector = Vector3.zero;
            return false;
        }

        private Vector3 g_point;
        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(g_point, _spherecastRadius);
        }

        private void AvoidAICars(Vector3 vectorToTarget, out Vector3 newVectorToTarget)
        {
            if (IsCarInFrontOfAICar(out Vector3 otherCarPosition, out Vector3 otherCarRightVector))
            {
                Vector3 avoidanceVector;
                avoidanceVector = Vector3.Reflect((otherCarPosition - transform.position).normalized, otherCarRightVector);

                float distanceTotarget = (_target.position - transform.position).magnitude;

                //As we get closer to waypoint the desire to reach the waypoint increases
                float driveToTargetInfluence = _desireToGetTheWaypoint / distanceTotarget;// / _carController.CurrentSpeed;
                //Ensure that we  limit the value to between 30% and 100%
                driveToTargetInfluence = Mathf.Clamp(driveToTargetInfluence, 0.3f, 1f);

                float avoidanceInfluence = 1f - driveToTargetInfluence;

                _avoidanceVectorLerped = Vector3.Lerp(_avoidanceVectorLerped, avoidanceVector, Time.fixedDeltaTime * _avoidanceLerpFactor / _car.CurrentSpeed);

                newVectorToTarget = vectorToTarget * driveToTargetInfluence + _avoidanceVectorLerped * avoidanceInfluence;
                newVectorToTarget.Normalize();

                Debug.DrawRay(transform.position, avoidanceVector * _castMaxDistance, Color.green);
                Debug.DrawRay(transform.position, newVectorToTarget * _castMaxDistance, Color.yellow);
                return;
            }

            newVectorToTarget = vectorToTarget;
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.rigidbody != null)
            {
                var otherAI = collision.rigidbody.GetComponent<CarAI>();
                if (otherAI != null)
                {
                    if (PlayerDriving)
                    {
                        PushOpponent(collision);
                    }
                    else
                    {
                        HandleCarsJam(otherAI);
                    }
                }
            }
        }

        private void PushOpponent(Collision collision)
        {
            //Debug.Log("Push");
            Vector3 contactPoint = collision.GetContact(0).point;
            Vector3 direction = collision.rigidbody.transform.position - transform.position;
            direction.Normalize();
            Vector3 force = direction * _car.CarConfig.Durability * _car.CurrentSpeed;
            force.y = 0f;

            collision.rigidbody.AddForceAtPosition(force, contactPoint, ForceMode.Impulse);
        }

        private void HandleCarsJam(CarAI otherAI)
        {
            // we'll take evasive action for 1 second
            _avoidOtherCarTime = Time.time + 1;

            float contactAngle = Vector3.Angle(transform.forward, otherAI.transform.position - transform.position);
            // but who's in front?...
            if (contactAngle < 90)
            {
                // the other ai is in front, so it is only good manners that we ought to brake...
                _avoidOtherCarFactor = 0.5f;
            }
            else
            {
                // we're in front! ain't slowing down for anybody...
                _avoidOtherCarFactor = 1;
            }

            // both cars should take evasive action by driving along an offset from the path centre,
            // away from the other car
            var otherCarLocalDelta = transform.InverseTransformPoint(otherAI.transform.position);
            float otherCarAngle = Mathf.Atan2(otherCarLocalDelta.x, otherCarLocalDelta.z);
            _avoidPathOffset = _lateralWanderDistance * -Mathf.Sign(otherCarAngle);
        }

        public void StopDriving() => _isDriving = false;

        public void StopEngine()
        {
            _isDriving = false;
            _target = null;
        }

        public void StartEngine()
        {
            _isDriving = true;
        }
    }
}
