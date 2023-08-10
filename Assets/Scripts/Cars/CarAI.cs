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

        [SerializeField] private Transform _target;             // 'target' the target object to aim for.
        [SerializeField] private bool _isDriving = false;       // whether the AI is currently actively driving or stopped.

        private Car _car;
        private SphereCollider _sphereCollider;
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

        private bool _playerDriving = false;
        private bool _isStopping = false;

        public float DesiredSpeed;

        #region Minor variables

        private Transform _itsTransform;

        private Vector3 m_VectorToTargetSteer;
        private Vector3 m_OffsetTargetPosWander;
        private Vector3 m_AvoidanceVector;

        private Vector3 m_PushContactPoint;
        private Vector3 m_PushDirection;
        private Vector3 m_PushForce;

        private Vector3 m_OtherCarLocalDelta;

        private RaycastHit m_RaycastHitFrontCheck;

        private float m_Acceleration;
        private float m_Steering;

        #endregion

        public bool isDriving => _isDriving;
        public bool PlayerDriving
        { 
            get => _playerDriving;
            set
            {
                if (value)
                    DesiredSpeed = _car.CarConfig.CruiseSpeed;
                else
                    DesiredSpeed = _car.CarConfig.MaxSpeed;
                _playerDriving = value;
                //Debug.Log($"{gameObject.name} speed: {DesiredSpeed}");
            }
        }
        public bool IsStopping
        { 
            get => _isStopping;
            set 
            {
                if (value)
                    DesiredSpeed = 0;
                else
                {
                    DesiredSpeed = PlayerDriving
                        ? _car.CarConfig.CruiseSpeed
                        : _car.CarConfig.MaxSpeed;
                }

                _isStopping = value;
            }
        }
        //public Transform Target => _target;
        public Car Car => _car;

        public void StopAvoiding() => _isAvoidingCars = false;

        public void Initialize()
        {
            _car = GetComponent<Car>();
            _sphereCollider = GetComponent<SphereCollider>();
            _spherecastRadius = _sphereCollider.radius * 0.5f;
            _randomPerlin = Random.value * 100;

            _criticalSteeAngle = _car.CarConfig.MaxSteerAngle;
            _itsTransform = transform;

            PlayerDriving = false;
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
                m_Acceleration = IsStopping ? 0 : CalculateAcceleration();
                m_Steering = CalculateSteering();

                _car.UpdateControls(m_Steering, m_Acceleration, IsStopping);
            }
            else
            {
                _car.UpdateControls(0, 0, true);
            }
        }

        private float CalculateAcceleration()
        {
            float acceleration = _car.SpeedInDesiredUnits > DesiredSpeed ? -1 : 1;

            acceleration *= (1 - _accelerationWanderAmount) +
                     (Mathf.PerlinNoise(Time.time * _accelerationWanderSpeed, _randomPerlin) * _accelerationWanderAmount);

            return acceleration;
        }

        private float CalculateSteering()
        {
            if (_target == null)
                return 0;

            m_VectorToTargetSteer = _target.position - _itsTransform.position;
            m_VectorToTargetSteer.Normalize();

            if (_isAvoidingCars)
                AvoidAICars(m_VectorToTargetSteer, out m_VectorToTargetSteer);

            //Vector3 position = MakeWanderPositionOffsetFromTarget();
            //vectorToTarget = Vector3.Lerp(vectorToTarget, position, _avoidanceLerpFactor * Time.fixedDeltaTime);

            float targetAngle = Vector3.SignedAngle(_itsTransform.forward, m_VectorToTargetSteer, _itsTransform.up);
            float steer = targetAngle / _criticalSteeAngle;

            //steer = Mathf.Clamp(steer, -1f, 1f);
            steer = Mathf.Clamp(steer * SteerSensitivity, -1f, 1f) * Mathf.Sign(_car.CurrentSpeed);
            return steer;
        }

        private Vector3 MakeWanderPositionOffsetFromTarget()
        {
            if (_target == null)
                return _itsTransform.position;

            m_OffsetTargetPosWander = _target.position;

            // if are we currently taking evasive action to prevent being stuck against another car:
            if (Time.time < _avoidOtherCarTime)
            {
                // slow down if necessary (if we were behind the other car when collision occured)
                DesiredSpeed *= _avoidOtherCarFactor;

                // and veer towards the side of our path-to-target that is away from the other car
                m_OffsetTargetPosWander += _target.right * _avoidPathOffset;
            }
            else
            {
                // no need for evasive action, we can just wander across the path-to-target in a random way,
                // which can help prevent AI from seeming too uniform and robotic in their driving
                m_OffsetTargetPosWander += _target.right *
                                   (Mathf.PerlinNoise(Time.time * _lateralWanderSpeed, _randomPerlin) * 2 - 1) *
                                   _lateralWanderDistance;
            }

            return m_OffsetTargetPosWander;
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

            Physics.SphereCast(_itsTransform.position + _itsTransform.forward * 0.5f, _spherecastRadius, _itsTransform.forward, out m_RaycastHitFrontCheck, _castMaxDistance, 1 << LayerMask.NameToLayer(Layer.Car));

            _sphereCollider.enabled = true;

            if (m_RaycastHitFrontCheck.collider != null)
            {
                Debug.DrawRay(_itsTransform.position, _itsTransform.forward * _castMaxDistance, Color.red);

                position = m_RaycastHitFrontCheck.collider.transform.position;
                otherCarRightVector = m_RaycastHitFrontCheck.collider.transform.right;

                return true;
            }
            else
            {
                Debug.DrawRay(_itsTransform.position, _itsTransform.forward * _castMaxDistance, Color.black);
            }

            position = Vector3.zero;
            otherCarRightVector = Vector3.zero;
            return false;
        }

        private void AvoidAICars(Vector3 vectorToTarget, out Vector3 newVectorToTarget)
        {
            if (IsCarInFrontOfAICar(out Vector3 otherCarPosition, out Vector3 otherCarRightVector))
            {
                m_AvoidanceVector = Vector3.Reflect((otherCarPosition - _itsTransform.position).normalized, otherCarRightVector);

                float distanceTotarget = (_target.position - _itsTransform.position).magnitude;

                //As we get closer to waypoint the desire to reach the waypoint increases
                float driveToTargetInfluence = _desireToGetTheWaypoint / distanceTotarget;// / _carController.CurrentSpeed;
                //Ensure that we  limit the value to between 30% and 100%
                driveToTargetInfluence = Mathf.Clamp(driveToTargetInfluence, 0.3f, 1f);

                float avoidanceInfluence = 1f - driveToTargetInfluence;

                _avoidanceVectorLerped = Vector3.Lerp(_avoidanceVectorLerped, m_AvoidanceVector, Time.fixedDeltaTime * _avoidanceLerpFactor / _car.CurrentSpeed);

                newVectorToTarget = vectorToTarget * driveToTargetInfluence + _avoidanceVectorLerped * avoidanceInfluence;
                newVectorToTarget.Normalize();

                Debug.DrawRay(_itsTransform.position, m_AvoidanceVector * _castMaxDistance, Color.green);
                Debug.DrawRay(_itsTransform.position, newVectorToTarget * _castMaxDistance, Color.yellow);
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
            m_PushContactPoint = collision.GetContact(0).point;
            m_PushDirection = collision.rigidbody.transform.position - _itsTransform.position;
            m_PushDirection.Normalize();
            m_PushForce = m_PushDirection * _car.CarConfig.Durability * _car.CurrentSpeed;
            m_PushForce.y = 0f;

            collision.rigidbody.AddForceAtPosition(m_PushForce, m_PushContactPoint, ForceMode.Impulse);
        }

        private void HandleCarsJam(CarAI otherAI)
        {
            // we'll take evasive action for 1 second
            _avoidOtherCarTime = Time.time + 1;

            float contactAngle = Vector3.Angle(_itsTransform.forward, otherAI.transform.position - _itsTransform.position);
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
            m_OtherCarLocalDelta = _itsTransform.InverseTransformPoint(otherAI.transform.position);
            float otherCarAngle = Mathf.Atan2(m_OtherCarLocalDelta.x, m_OtherCarLocalDelta.z);
            _avoidPathOffset = _lateralWanderDistance * -Mathf.Sign(otherCarAngle);
        }

        public void StartDriving() => _isDriving = true;
        public void StopDriving() => _isDriving = false;

        public void StopEngine()
        {
            IsStopping = true;
            //_isDriving = false;
            //_target = null;
        }
    }
}
