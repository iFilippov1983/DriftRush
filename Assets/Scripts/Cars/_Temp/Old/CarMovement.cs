using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Alt
{
    public class CarMovement : MonoBehaviour
    {
        private const float KPHFactor = 3.6f;
        private const float MPHFactor = 2.23693629f;

        private const float Downforce = 300f; //100
        private const float BrakeTorque = 20000f;
        private const float MaxHandbrakeTorque = float.MaxValue;
        private const float RotationDifferenceThreshold = 5f;//10
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
        private float ReverseTorque = 20000;//200

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
            ReverseTorque = _carSettings.FullTorqueOverAllWheels / 4;

            _carRb.centerOfMass = _car.CenterOfMass;
        }

        public void Move(float steering, float accel, float footbrake, float handbrake)
        {
            _carRb.centerOfMass = _car.CenterOfMass; // Doing each frame allows it to be changed in inspector

            SteerInput = steering = Mathf.Clamp(steering, -1, 1);
            AccelInput = accel = Mathf.Clamp(accel, 0, 1);
            BrakeInput = footbrake = -1 * Mathf.Clamp(footbrake, -1, 0);
            handbrake = Mathf.Clamp(handbrake, 0, 1);

            SetWheelsMeshesPositions();
            SetSteerOnFrontWheels(steering);
            SteerHelper();
            ApplyDrive(accel, footbrake);
            CapSpeed();
            HandBrake(handbrake);
            AddDownForce();
            TractionControl();


            _velocityVsForward = Vector3.Dot(transform.forward, _carRb.velocity);
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

            if (footbrake <= 0)
                return;
            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                //if (CurrentSpeed > 5 && Vector3.Angle(_carRb.transform.forward, _carRb.velocity) < 50f)
                //{
                //    _wheelColliders[i].brakeTorque = BrakeTorque * footbrake;
                //}
                //else if (footbrake > 0)
                //{
                //    _wheelColliders[i].brakeTorque = 0f;
                //    _wheelColliders[i].motorTorque = -ReverseTorque * footbrake;
                //}
                _wheelColliders[i].brakeTorque = 0f;
                _wheelColliders[i].motorTorque = -ReverseTorque * footbrake;
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
            if (Mathf.Abs(_oldRotation - _carRb.transform.eulerAngles.y) < RotationDifferenceThreshold)
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

