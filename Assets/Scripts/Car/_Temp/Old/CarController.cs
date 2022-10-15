using RaceManager.Cars.Effects;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Alt
{
    public class CarController : MonoBehaviour
    {
        private const float KPHFactor = 3.6f;
        private const float MPHFactor = 2.23693629f;

        private const float Downforce = 300f; //100
        
        private const float MaxHandbrakeTorque = float.MaxValue;
        
        private const float TractionCtrl = 0f; //(0-1) 0 is no traction control, 1 is full interference
        [SerializeField] private float _skidThreshold = 2f;//0.2

        private SpeedType _speedType = SpeedType.KPH;

        private Car _car;
        //[SerializeField] 
        private CarSettings _carSettings;

        private WheelCollider[] _wheelColliders;
        private GameObject[] _wheelMeshes;
        private Rigidbody _carRb;

        private Quaternion[] _wheelMeshLocalRotations;
        private float _steerAngle;
        private float _oldRotation;
        private float _currentTorque;
        private float _velocityVsForward;
        private float _brakeTorque = 20000f;
        private float _reverseTorque = 20000f;//200
        private float _rotationDifferenceThreshold = 1f;//10

        public SpeedType SpeedType { get; set; } = SpeedType.KPH;
        public float AccelInput { get; private set; }
        public float BrakeInput { get; private set; }
        public float SteerInput { get; private set; }
        public float MaxVelocityMagnitude => _carSettings.MaxRBVelocityMagnitude;
        public float VelocityMagnitude => _carRb.velocity.magnitude;
        public Vector3 Velocity => _carRb.velocity;
        public void SetSpeedType(SpeedType speedType) => _speedType = speedType;


        //private void OnEnable()
        //{
        //    _car = GetComponent<Car>();
        //    _wheelColliders = _car.WheelColliders;
        //    _wheelMeshes = _car.WheelMeshes;
        //    _wheelEffects = _car.WheelEffects;
        //    _carAudio = _car.CarAudio;

        //    SetWheelMeshesRotation();

        //    _rigidbody = GetComponent<Rigidbody>();
        //    //_currentTorque = _carSettings.FullTorqueOverAllWheels - (TractionCtrl * _carSettings.FullTorqueOverAllWheels);
        //}

        public void Initialize(CarSettings settings)
        {
            _carSettings = settings;

            _carRb = GetComponent<Rigidbody>();
            _car = GetComponent<Car>();

            _wheelColliders = _car.WheelColliders;
            _wheelMeshes = _car.WheelMeshes;

            SetWheelMeshesRotation();
            _currentTorque = _carSettings.FullTorqueOverAllWheels - (TractionCtrl * _carSettings.FullTorqueOverAllWheels);
            _brakeTorque = _carSettings.FullTorqueOverAllWheels / 2;
            _reverseTorque = _carSettings.FullTorqueOverAllWheels / 10;
            _rotationDifferenceThreshold = _carSettings.SteerHelperRange;

            _carRb.centerOfMass = _car.CenterOfMass;
        }

        public void Move(float steering, float accel, float footbrake, float handbrake)
        {
            _carRb.centerOfMass = _car.CenterOfMass; // Doing each frame allows it to be changed in inspector

            SteerInput = steering = Mathf.Clamp(steering, -1, 1);
            AccelInput = accel = Mathf.Clamp(accel, 0, 1);
            BrakeInput = footbrake = -1 * Mathf.Clamp(footbrake, -1, 0);
            handbrake = Mathf.Clamp(handbrake, 0, 1);

            // If rear wheels on ground
            if (_car.WheelsGrounded())
            {
                _carRb.angularVelocity += -transform.up * GetSteeringAngularAcceleration() * Time.fixedDeltaTime;
            }

            SetWheelsMeshesPositions();
            SetSteerOnFrontWheels(steering);
            SteerHelper();
            ApplyDrive(accel, footbrake);
            KillOrthogonalVelocity();
            //WheelsDriftControl();
            CapSpeed();
            HandBrake(handbrake);
            //AddDownForce();
            TractionControl();

           //AddDrift();

            _velocityVsForward = Vector3.Dot(transform.forward, _carRb.velocity);
        }

        private float GetSteeringAngularAcceleration()
        {
            return AccelInput * _carSettings.MaximumSteerAngle * Mathf.PI / 100;
        }

        public void StartMove()
        {
            _currentTorque = _carSettings.FullTorqueOverAllWheels - (TractionCtrl * _carSettings.FullTorqueOverAllWheels);
            Move(0f, 2f, 0f, 0f);
        }

        public bool AreTiresScreeching(out float lateralVelocity, out bool isBraking)
        {
            lateralVelocity = GetLateralVelocity();
            isBraking = false;

            if (AccelInput <= 0 && _velocityVsForward >= 0)
            {
                isBraking = true;
                return true;
            }

            if (Mathf.Abs(lateralVelocity) > _skidThreshold)
                return true;

            return false;
        }

        private void SetWheelMeshesRotation()
        {
            _wheelMeshLocalRotations = new Quaternion[4];
            for (int i = 0; i < _wheelMeshLocalRotations.Length; i++)
            {
                _wheelMeshLocalRotations[i] = _wheelMeshes[i].transform.localRotation;
            }
        }

        private void SetSteerOnFrontWheels(float steering)
        {
            //Assuming that wheels 0 and 1 are the front wheels.
            _steerAngle = steering * _carSettings.MaximumSteerAngle;
            _wheelColliders[0].steerAngle = _steerAngle;
            _wheelColliders[1].steerAngle = _steerAngle;
        }

        private void ApplyDrive(float accel, float footbrake)
        {
                                                                 //to prevent wheels block
            float thrustTorque = accel * (_currentTorque / 4f) + 0.0001f;
            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                _wheelColliders[i].motorTorque = thrustTorque;
            }

            if (footbrake < 0)
                return;
            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                if (VelocityMagnitude > _carSettings.CruiseRBVelocityMagnitude && Vector3.Angle(_carRb.transform.forward, _carRb.velocity) < _carSettings.MaximumSteerAngle)
                {
                    _wheelColliders[i].brakeTorque = _brakeTorque * footbrake;
                }
                else if (footbrake > 0)
                {
                    _wheelColliders[i].brakeTorque = 0f;
                    _wheelColliders[i].motorTorque = -_reverseTorque * footbrake;
                }
                else
                {
                    _wheelColliders[i].brakeTorque = 0;
                }

                //_wheelColliders[i].brakeTorque = 0f;
                //_wheelColliders[i].motorTorque = -_reverseTorque * footbrake;
            }
        }

        private void SteerHelper()
        {
            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                WheelHit wheelhit;
                _wheelColliders[i].GetGroundHit(out wheelhit);
                if (wheelhit.normal == Vector3.zero)
                    return; // wheels arent on the ground so dont realign the rigidbody velocity
            }

            // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
            if (Mathf.Abs(_oldRotation - _carRb.transform.eulerAngles.y) < _rotationDifferenceThreshold)
            {
                var turnadjust = (_carRb.transform.eulerAngles.y - _oldRotation) * _carSettings.SteerHelper;
                Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
                _carRb.velocity = velRotation * _carRb.velocity;
            }
            _oldRotation = _carRb.transform.eulerAngles.y;
        }



        // crude traction control that reduces the power to wheel if the car is wheel spinning too much
        private void TractionControl()
        {
            WheelHit wheelHit;
            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                _wheelColliders[i].GetGroundHit(out wheelHit);

                AdjustTorque(wheelHit.forwardSlip);
            }
        }

        private void AdjustTorque(float forwardSlip)
        {
            if (forwardSlip >= _skidThreshold && _currentTorque >= 0)
            {
                _currentTorque -= 10 * TractionCtrl;
            }
            else
            {
                _currentTorque += 10 * TractionCtrl;
                if (_currentTorque > _carSettings.FullTorqueOverAllWheels)
                {
                    _currentTorque = _carSettings.FullTorqueOverAllWheels;
                }
            }
        }

        private void SetWheelsMeshesPositions()
        {
            for (int i = 0; i < 4; i++)
            {
                Quaternion quat;
                Vector3 position;
                _wheelColliders[i].GetWorldPose(out position, out quat);
                _wheelMeshes[i].transform.position = position;
                _wheelMeshes[i].transform.rotation = quat;
            }
        }

        private void WheelsDriftControl()
        {
            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                Transform tireTransform = _wheelColliders[i].transform;
                float tireMass = _wheelColliders[i].mass;
                float gripFactor = i < 2
                    ? _carSettings.FrontWheelsGripFactor
                    : _carSettings.BackWheelsGripFactor;

                if (_wheelColliders[i].isGrounded)
                {
                    Vector3 steeringDirection = tireTransform.right;
                    Vector3 tireWorldVelocity = _carRb.GetPointVelocity(tireTransform.position);
                    float steeringVelocity = Vector3.Dot(steeringDirection, tireWorldVelocity);
                    float desiredVelChange = -steeringVelocity * gripFactor;
                    float desiredAcceleration = desiredVelChange / Time.fixedDeltaTime;

                    Vector3 force = steeringDirection * tireMass * desiredAcceleration;

                    _carRb.AddForceAtPosition(force, tireTransform.position);
                    //if (gripFactor == 0 && steeringVelocity > _skidThreshold)
                    //{
                    //    AddDriftOnRareWheels();
                    //}
                }
            }
        }

        private void KillOrthogonalVelocity()
        {
            float forwardVelocityDot = Vector3.Dot(_carRb.velocity, transform.forward);
            Vector3 forwardVelocity = transform.forward * forwardVelocityDot;

            float rightVelocityDot = Vector3.Dot(_carRb.velocity, transform.right);
            Vector3 rightVelocity = transform.right * rightVelocityDot;

            _carRb.velocity = forwardVelocity + rightVelocity * _carSettings.DriftFactor;
        }

        private void AddDrift(Vector3 forcePos)
        {
            float driftForce = Vector3.Dot(_carRb.GetRelativePointVelocity(Vector3.left), _carRb.velocity) * 2f;
            Vector3 relativeForce = Vector3.right * driftForce;
            _carRb.AddForceAtPosition(_carRb.GetRelativePointVelocity(relativeForce), forcePos);
        }

        private void AddDriftOnRareWheels()
        {
            AddDrift(_wheelColliders[2].transform.position);
            AddDrift(_wheelColliders[3].transform.position);
        }

        private void HandBrake(float handbrake)
        {
            //Assuming that wheels 2 and 3 are the rear wheels.
            if (handbrake > 0f)
            {
                var hbTorque = handbrake * MaxHandbrakeTorque;
                _wheelColliders[2].brakeTorque = hbTorque;
                _wheelColliders[3].brakeTorque = hbTorque;
            }
        }

        // this is used to add more grip in relation to speed
        private void AddDownForce() 
            => _wheelColliders[0].attachedRigidbody.AddForce(-_carRb.transform.up * Downforce * _wheelColliders[0].attachedRigidbody.velocity.magnitude);
        private float GetLateralVelocity() 
            => Vector3.Dot(transform.right, _carRb.velocity);
        

        private void CapSpeed()
        {
            float speed = _carRb.velocity.magnitude;

            switch (_speedType)
            {
                case SpeedType.MPH:

                    speed *= MPHFactor;
                    if (speed > _carSettings.MaxSpeed)
                        _carRb.velocity = (_carSettings.MaxSpeed / MPHFactor) * _carRb.velocity.normalized;
                    break;

                case SpeedType.KPH:
                    speed *= KPHFactor;
                    if (speed > _carSettings.MaxSpeed)
                        _carRb.velocity = (_carSettings.MaxSpeed / KPHFactor) * _carRb.velocity.normalized;
                    break;
            }
        }
    }
}
