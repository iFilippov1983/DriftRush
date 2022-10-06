using RaceManager.Cars.Effects;
using System;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Cars
{
    public class CarController : MonoBehaviour
    {
        private const float Downforce = 100f;
        private const float BrakeTorque = 20000f;
        private const float ReverseTorque = 200;
        private const float MaxHandbrakeTorque = float.MaxValue;
        private const float TractionCtrl = 0f; //(0-1) 0 is no traction control, 1 is full interference
        [SerializeField] private float SkidThreshold = 2f;//0.2

        private Car _car;
        //[SerializeField] 
        private CarSettings _carSettings;

        private WheelCollider[] _wheelColliders;
        private GameObject[] _wheelMeshes;
        private Rigidbody _carRb;
        private Vector3 _ñenterOfMassOffset;

        private Quaternion[] _wheelMeshLocalRotations;
        private float _steerAngle;
        private float _oldRotation;
        private float _currentTorque;
        private float _velocityVsForward;

        public SpeedType SpeedType { get; set; } = SpeedType.KPH;
        public float BrakeInput { get; private set; }
        public float MaxSpeed => _carSettings.MaxSpeed;
        public float AccelInput { get; private set; }
        public float CurrentSpeed
        {
            //3.6f - convertion to KPH // 2.23693629f - convertion to MPH
            get
            {
                float factor = SpeedType == SpeedType.KPH ? 3.6f : 2.23693629f;
                float speed = _carRb.velocity.magnitude;// * factor;
                return speed;
            }
        }

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
            _wheelColliders[0].attachedRigidbody.centerOfMass = _ñenterOfMassOffset;
            _currentTorque = _carSettings.FullTorqueOverAllWheels - (TractionCtrl * _carSettings.FullTorqueOverAllWheels);
        }

        public void Move(float steering, float accel, float footbrake, float handbrake)
        {
            _velocityVsForward = Vector3.Dot(transform.forward, _carRb.velocity);

            SetWheelsMeshesPositions();

            //clamp input values
            steering = Mathf.Clamp(steering, -1, 1);
            AccelInput = accel = Mathf.Clamp(accel, 0, 1);
            BrakeInput = footbrake = -1 * Mathf.Clamp(footbrake, -1, 0);
            handbrake = Mathf.Clamp(handbrake, 0, 1);

            SetSteerOnFrontWheels(steering);
            SteerHelper();
            ApplyDrive(accel, footbrake);
            KillOrthogonalVelocity();
            //CapSpeed();
            HandBrake(handbrake);
            AddDownForce();
            TractionControl();
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

            if (Mathf.Abs(lateralVelocity) > SkidThreshold)
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

            //float thrustTorque;
            float thrustTorque = accel * (_currentTorque / 4f);
            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                _wheelColliders[i].motorTorque = thrustTorque;
            }

            for (int i = 0; i < _wheelColliders.Length; i++)
            {
                if (CurrentSpeed > 5 && Vector3.Angle(_carRb.transform.forward, _carRb.velocity) < 50f)
                {
                    _wheelColliders[i].brakeTorque = BrakeTorque * footbrake;
                }
                else if (footbrake > 0)
                {
                    _wheelColliders[i].brakeTorque = 0f;
                    _wheelColliders[i].motorTorque = -ReverseTorque * footbrake;
                }
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
            if (Mathf.Abs(_oldRotation - _carRb.transform.eulerAngles.y) < 10f)
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
            for (int i = 0; i < 4; i++)
            {
                _wheelColliders[i].GetGroundHit(out wheelHit);

                AdjustTorque(wheelHit.forwardSlip);
            }
        }

        private void AdjustTorque(float forwardSlip)
        {
            if (forwardSlip >= SkidThreshold && _currentTorque >= 0)
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

        private void KillOrthogonalVelocity()
        {
            Vector3 forwardVelocity = transform.forward * Vector3.Dot(_carRb.velocity, transform.forward);
            Vector3 rightVelocity = transform.right * Vector3.Dot(_carRb.velocity, transform.right);
            _carRb.velocity = forwardVelocity + rightVelocity * _carSettings.DriftFactor;
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
        public float GetVelocityMagnitude() 
            => _carRb.velocity.magnitude;

        //private void CapSpeed()
        //{
        //    float speed = m_Rigidbody.velocity.magnitude;
        //    switch (_vehicleSettings.SpeedType)
        //    {
        //        case SpeedType.MPH:

        //            speed *= 2.23693629f;
        //            if (speed > _vehicleSettings.SpeedTop)
        //                m_Rigidbody.velocity = (_vehicleSettings.SpeedTop / 2.23693629f) * m_Rigidbody.velocity.normalized;
        //            break;

        //        case SpeedType.KPH:
        //            speed *= 3.6f;
        //            if (speed > _vehicleSettings.SpeedTop)
        //                m_Rigidbody.velocity = (_vehicleSettings.SpeedTop / 3.6f) * m_Rigidbody.velocity.normalized;
        //            break;
        //    }
        //}
    }
}
