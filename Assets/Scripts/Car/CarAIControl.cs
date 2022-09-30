using Sirenix.OdinInspector;
using System;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

#pragma warning disable 649
namespace RaceManager.Vehicles
{
    [RequireComponent(typeof (CarController))]
    public class CarAIControl : CarAI
    {
        // "wandering" is used to give the cars a more human, less robotic feel. They can waver slightly
        // in speed and direction while driving towards their target.
        private const float CautiousMaxDistance = 100f;                   // distance at which distance-based cautiousness begins
        private const float CautiousAngularVelocityFactor = 90f;          // how cautious the AI should be when considering its own current angular velocity (i.e. easing off acceleration if spinning!)
        private const float CautiousSpeedFactor = 1f;                     // (0-1)Percentage of max speed to use when being maximally cautious
        private const float CautiousMaxAngle = 180f;                      // angle of approaching corner to treat as warranting maximum caution
        private const float SteerSensitivity = 0.01f;                     // (0-1)How sensitively the AI uses steering input to turn to the desired direction
        private const float BrakeSensitivity = 1f;                        // (0-1)How sensitively the AI uses the brake to reach the current desired speed
        private const float AccelSensitivity = 1f;                        //(0-1)How sensitively the AI uses the accelerator to reach the current desired speed
        private const float AccelWanderSpeed = 0.1f;                      // (0-1)How fast the cars acceleration wandering will fluctuate
        private const float LateralWanderDistance = 5f;                   // how far the car will wander laterally towards its target
        private const float LateralWanderSpeed = 0.2f;                    // (0-1)How fast the lateral wandering will fluctuate
        private const float ReachTargetThreshold = 2;                     // proximity to target to consider we 'reached' it, and stop driving.
        private BrakeCondition BrakeCond = BrakeCondition.NeverBrake;     // what should the AI consider when accelerating/braking?

        [ShowInInspector, ReadOnly]
        private string _id;

        [SerializeField] private AIDriverSettings _driverSettings;
        [SerializeField] private Transform _target;                        // 'target' the target object to aim for.
        
        [SerializeField] private bool _isDriving;                          // whether the AI is currently actively driving or stopped.

        private bool _stopWhenTargetReached;     // should we stop driving when we reach the target?
        private float _randomPerlin;             // A random value for the car to base its wander on (so that AI cars don't all wander in the same pattern)
        private CarController _carController;    // Reference to actual car controller we are controlling
        private float _avoidOtherCarTime;        // time until which to avoid the car we recently collided with
        private float _avoidOtherCarSlowdown;    // how much to slow down due to colliding with another car, whilst avoiding
        private float _avoidPathOffset;          // direction (-1 or 1) in which to offset path to avoid other car, whilst avoiding
        private float _accelWanderAmount = 1f;   //(0.001-1)How much the cars acceleration will wander
        private float _currentSpeed;
        private Rigidbody _rigidbody;

        public bool isDriving => _isDriving;
        [ReadOnly]
        public bool PlayerDriving = false;
        [ReadOnly]
        public float DesiredSpeed;
        public float CruiseSpeed => _driverSettings.CruiseSpeed;

        private void Awake()
        {
            // get the car controller reference
            _carController = GetComponent<CarController>();

            // give the random perlin a random value
            _randomPerlin = Random.value * 100;

            _rigidbody = GetComponent<Rigidbody>();

            _id = MakeId();
        }

        private void Start()
        {
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
            if (_target == null || !_isDriving)
            {
                // Car should not be moving,
                // use handbrake to stop
                _carController.Move(0, 0, -1f, 1f);
            }
            else
            {
                Vector3 fwd = transform.forward;
                if (_rigidbody.velocity.magnitude > _carController.MaxSpeed * 0.1f)
                {
                    fwd = _rigidbody.velocity;
                }

                _currentSpeed = DesiredSpeed;

                // now it's time to decide if we should be slowing down...
                switch (BrakeCond)
                {
                    case BrakeCondition.TargetDirectionDifference:
                        {
                            // the car will brake according to the upcoming change in direction of the target. Useful for route-based AI, slowing for corners.

                            // check out the angle of our target compared to the current direction of the car
                            float approachingCornerAngle = Vector3.Angle(_target.forward, fwd);

                            // also consider the current amount we're turning, multiplied up and then compared in the same way as an upcoming corner angle
                            float spinningAngle = _rigidbody.angularVelocity.magnitude * CautiousAngularVelocityFactor;

                            // if it's different to our current angle, we need to be cautious (i.e. slow down) a certain amount
                            float cautiousnessRequired =
                                Mathf.InverseLerp(0, CautiousMaxAngle, Mathf.Max(spinningAngle, approachingCornerAngle));

                            _currentSpeed = 
                                Mathf.Lerp(_carController.MaxSpeed, _carController.MaxSpeed * CautiousSpeedFactor, cautiousnessRequired);
                            break;
                        }

                    case BrakeCondition.TargetDistance:
                        {
                            // the car will brake as it approaches its target, regardless of the target's direction. Useful if you want the car to
                            // head for a stationary target and come to rest when it arrives there.

                            // check out the distance to target
                            Vector3 delta = _target.position - transform.position;
                            float distanceCautiousFactor = Mathf.InverseLerp(CautiousMaxDistance, 0, delta.magnitude);

                            // also consider the current amount we're turning, multiplied up and then compared in the same way as an upcoming corner angle
                            float spinningAngle = _rigidbody.angularVelocity.magnitude * CautiousAngularVelocityFactor;

                            // if it's different to our current angle, we need to be cautious (i.e. slow down) a certain amount
                            float cautiousnessRequired = Mathf.Max(
                                Mathf.InverseLerp(0, CautiousMaxAngle, spinningAngle), distanceCautiousFactor);
                            _currentSpeed =
                                Mathf.Lerp(_carController.MaxSpeed, _carController.MaxSpeed * CautiousSpeedFactor, cautiousnessRequired);
                            break;
                        }

                    case BrakeCondition.NeverBrake:
                        break;
                }

                // Evasive action due to collision with other cars:

                // our target position starts off as the 'real' target position
                Vector3 offsetTargetPos = _target.position;

                // if are we currently taking evasive action to prevent being stuck against another car:
                if (Time.time < _avoidOtherCarTime)
                {
                    // slow down if necessary (if we were behind the other car when collision occured)
                    _currentSpeed *= _avoidOtherCarSlowdown;

                    // and veer towards the side of our path-to-target that is away from the other car
                    offsetTargetPos += _target.right*_avoidPathOffset;
                }
                else
                {
                    // no need for evasive action, we can just wander across the path-to-target in a random way,
                    // which can help prevent AI from seeming too uniform and robotic in their driving
                    offsetTargetPos += _target.right*
                                       (Mathf.PerlinNoise(Time.time * LateralWanderSpeed, _randomPerlin) * 2 - 1)*
                                       LateralWanderDistance;
                }

                // use different sensitivity depending on whether accelerating or braking:
                float accelBrakeSensitivity = (_currentSpeed < _carController.CurrentSpeed)
                                                  ? BrakeSensitivity
                                                  : AccelSensitivity;

                // decide the actual amount of accel/brake input to achieve desired speed.
                float accel = Mathf.Clamp((_currentSpeed - _carController.CurrentSpeed) * accelBrakeSensitivity, -1, 1);

                // add acceleration 'wander', which also prevents AI from seeming too uniform and robotic in their driving
                // i.e. increasing the accel wander amount can introduce jostling and bumps between AI cars in a race
                accel *= (1 - _accelWanderAmount) +
                         (Mathf.PerlinNoise(Time.time * AccelWanderSpeed, _randomPerlin) * _accelWanderAmount);

                // calculate the local-relative position of the target, to steer towards
                Vector3 localTarget = transform.InverseTransformPoint(offsetTargetPos);

                // work out the local angle towards the target
                float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z)*Mathf.Rad2Deg;

                // get the amount of steering needed to aim the car towards the target
                float steer = Mathf.Clamp(targetAngle * SteerSensitivity, -1, 1) * Mathf.Sign(_carController.CurrentSpeed);

                // feed input to the car controller.
                _carController.Move(steer, accel, accel, 0f);

                // if appropriate, stop driving when we're close enough to the target.
                if (_stopWhenTargetReached && localTarget.magnitude < ReachTargetThreshold)
                {
                    _isDriving = false;
                }
            }
        }

        private void OnCollisionStay(Collision col)
        {
            // detect collision against other cars, so that we can take evasive action
            if (col.rigidbody != null)
            {
                var otherAI = col.rigidbody.GetComponent<CarAIControl>();
                if (otherAI != null)
                {
                    // we'll take evasive action for 1 second
                    _avoidOtherCarTime = Time.time + 1;

                    // but who's in front?...
                    if (Vector3.Angle(transform.forward, otherAI.transform.position - transform.position) < 90)
                    {
                        // the other ai is in front, so it is only good manners that we ought to brake...
                        _avoidOtherCarSlowdown = 1.5f;//0.5f;
                    }
                    else
                    {
                        // we're in front! ain't slowing down for anybody...
                        _avoidOtherCarSlowdown = 1f;
                    }

                    // both cars should take evasive action by driving along an offset from the path centre,
                    // away from the other car
                    var otherCarLocalDelta = transform.InverseTransformPoint(otherAI.transform.position);
                    float otherCarAngle = Mathf.Atan2(otherCarLocalDelta.x, otherCarLocalDelta.z);
                    _avoidPathOffset = LateralWanderDistance * -Mathf.Sign(otherCarAngle);
                }
            }
        }

        public void SetAccelAmount(float value)
        {
            _accelWanderAmount = value;
            Debug.Log($"{value}");
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public override void StopEngine()
        {
            _target = null;
            _isDriving = false;
            $"{gameObject.name} FINISHED".Log(StringConsoleLog.Color.Yellow);
        }

        public override void StartEngine()
        {
            _isDriving = true;
        }

        public override string GetID() => _id;

        private string MakeId()
        {
            StringBuilder builder = new StringBuilder();
            Enumerable
               .Range(65, 26)
                .Select(e => ((char)e).ToString())
                .Concat(Enumerable.Range(97, 26).Select(e => ((char)e).ToString()))
                .Concat(Enumerable.Range(0, 10).Select(e => e.ToString()))
                .OrderBy(e => Guid.NewGuid())
                .Take(11)
                .ToList().ForEach(e => builder.Append(e));
            return builder.ToString();
        }
    }
}
