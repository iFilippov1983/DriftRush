using RaceManager.Vehicles.Effects;
using System;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Vehicles
{
    public class CarController : MonoBehaviour
    {
        private const float Downforce = 100f;
        private const float BrakeTorque = 20000f;
        private const float ReverseTorque = 200;
        private const float MaxHandbrakeTorque = float.MaxValue;
        private const int NoOfGears = 5;
        private const float RevRangeBoundary = 1f;
        private const float TractionCtrl = 0f; //(0-1) 0 is no traction control, 1 is full interference
        private const float SlipLimit = 0.1f;

        [SerializeField] private VehicleSettings _vehicleSettings;
        [SerializeField] private WheelCollider[] m_WheelColliders = new WheelCollider[4];
        [SerializeField] private GameObject[] m_WheelMeshes = new GameObject[4];
        [SerializeField] private WheelEffects[] m_WheelEffects = new WheelEffects[4];
        [SerializeField] private Vector3 _ñentreOfMassOffset;

        private CarDriveType _carDriveType = CarDriveType.FourWheelDrive;
        //private SpeedType _speedType = SpeedType.KPH;

        private Quaternion[] _wheelMeshLocalRotations;
        private float _steerAngle;
        private int _gearNum;
        private float _gearFactor;
        private float _oldRotation;
        private float _currentTorque;
        private Rigidbody _rigidbody;

        public bool Skidding { get; private set; }
        public float BrakeInput { get; private set; }
        public float CurrentSteerAngle => _steerAngle;
        public float CurrentSpeed => _rigidbody.velocity.magnitude * 3.6f;//convertion to KPH // * 2.23693629f//convertion to MPH
        public float MaxSpeed => _vehicleSettings.MaxSpeed;
        public float Acceleration => _vehicleSettings.AccelerationFactor;
        public float Revs { get; private set; }
        public float AccelInput { get; private set; }

        private void Start()
        {
            _wheelMeshLocalRotations = new Quaternion[4];
            for (int i = 0; i < 4; i++)
            {
                _wheelMeshLocalRotations[i] = m_WheelMeshes[i].transform.localRotation;
            }
            m_WheelColliders[0].attachedRigidbody.centerOfMass = _ñentreOfMassOffset;

            //MaxHandbrakeTorque = float.MaxValue;

            _rigidbody = GetComponent<Rigidbody>();
            _currentTorque = _vehicleSettings.FullTorqueOverAllWheels - (TractionCtrl * _vehicleSettings.FullTorqueOverAllWheels);
        }

        public void SetVehicleSettings(VehicleSettings vehicleSettings) => _vehicleSettings = vehicleSettings;

        public void Move(float steering, float accel, float footbrake, float handbrake)
        {
            for (int i = 0; i < 4; i++)
            {
                Quaternion quat;
                Vector3 position;
                m_WheelColliders[i].GetWorldPose(out position, out quat);
                m_WheelMeshes[i].transform.position = position;
                m_WheelMeshes[i].transform.rotation = quat;
            }

            //clamp input values
            steering = Mathf.Clamp(steering, -1, 1);
            AccelInput = accel = Mathf.Clamp(accel, 0, 1);
            BrakeInput = footbrake = -1 * Mathf.Clamp(footbrake, -1, 0);
            handbrake = Mathf.Clamp(handbrake, 0, 1);

            //Set the steer on the front wheels.
            //Assuming that wheels 0 and 1 are the front wheels.
            _steerAngle = steering * _vehicleSettings.MaximumSteerAngle;
            m_WheelColliders[0].steerAngle = _steerAngle;
            m_WheelColliders[1].steerAngle = _steerAngle;

            SteerHelper();
            ApplyDrive(accel, footbrake);
            //CapSpeed();

            //Set the handbrake.
            //Assuming that wheels 2 and 3 are the rear wheels.
            if (handbrake > 0f)
            {
                var hbTorque = handbrake * MaxHandbrakeTorque;
                m_WheelColliders[2].brakeTorque = hbTorque;
                m_WheelColliders[3].brakeTorque = hbTorque;
            }


            CalculateRevs();
            GearChanging();

            AddDownForce();
            CheckForWheelSpin();
            CheckSlip();
            TractionControl();
        }

        private void GearChanging()
        {
            float f = Mathf.Abs(CurrentSpeed/MaxSpeed);
            float upgearlimit = (1/(float) NoOfGears)*(_gearNum + 1);
            float downgearlimit = (1/(float)NoOfGears) *_gearNum;

            if (_gearNum > 0 && f < downgearlimit)
            {
                _gearNum--;
            }

            if (f > upgearlimit && (_gearNum < (NoOfGears - 1)))
            {
                _gearNum++;
            }
        }

        // simple function to add a curved bias towards 1 for a value in the 0-1 range
        private static float CurveFactor(float factor)
        {
            return 1 - (1 - factor)*(1 - factor);
        }

        // unclamped version of Lerp, to allow value to exceed the from-to range
        private static float ULerp(float from, float to, float value)
        {
            return (1.0f - value)*from + value*to;
        }

        private void CalculateGearFactor()
        {
            float f = (1/(float)NoOfGears);
            // gear factor is a normalised representation of the current speed within the current gear's range of speeds.
            // We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
            var targetGearFactor = Mathf.InverseLerp(f*_gearNum, f*(_gearNum + 1), Mathf.Abs(CurrentSpeed/MaxSpeed));
            _gearFactor = Mathf.Lerp(_gearFactor, targetGearFactor, Time.deltaTime*5f);
        }

        private void CalculateRevs()
        {
            // calculate engine revs (for display / sound)
            // (this is done in retrospect - revs are not used in force/power calculations)
            CalculateGearFactor();
            var gearNumFactor = _gearNum / (float)NoOfGears;
            var revsRangeMin = ULerp(0f, RevRangeBoundary, CurveFactor(gearNumFactor));
            var revsRangeMax = ULerp(RevRangeBoundary, 1f, gearNumFactor);
            Revs = ULerp(revsRangeMin, revsRangeMax, _gearFactor);
        }

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

        private void ApplyDrive(float accel, float footbrake)
        {

            float thrustTorque;
            switch (_carDriveType)
            {
                case CarDriveType.FourWheelDrive:
                    thrustTorque = accel * (_currentTorque / 4f);
                    for (int i = 0; i < 4; i++)
                    {
                        m_WheelColliders[i].motorTorque = thrustTorque;
                    }
                    break;

                case CarDriveType.FrontWheelDrive:
                    thrustTorque = accel * (_currentTorque / 2f);
                    m_WheelColliders[0].motorTorque = m_WheelColliders[1].motorTorque = thrustTorque;
                    break;

                case CarDriveType.RearWheelDrive:
                    thrustTorque = accel * (_currentTorque / 2f);
                    m_WheelColliders[2].motorTorque = m_WheelColliders[3].motorTorque = thrustTorque;
                    break;

            }

            for (int i = 0; i < 4; i++)
            {
                if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, _rigidbody.velocity) < 50f)
                {
                    m_WheelColliders[i].brakeTorque = BrakeTorque * footbrake;
                }
                else if (footbrake > 0)
                {
                    m_WheelColliders[i].brakeTorque = 0f;
                    m_WheelColliders[i].motorTorque = -ReverseTorque * footbrake;
                }
            }
        }

        private void SteerHelper()
        {
            for (int i = 0; i < 4; i++)
            {
                WheelHit wheelhit;
                m_WheelColliders[i].GetGroundHit(out wheelhit);
                if (wheelhit.normal == Vector3.zero)
                    return; // wheels arent on the ground so dont realign the rigidbody velocity
            }

            // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
            if (Mathf.Abs(_oldRotation - transform.eulerAngles.y) < 10f)
            {
                var turnadjust = (transform.eulerAngles.y - _oldRotation) * _vehicleSettings.SteerHelper;
                Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
                _rigidbody.velocity = velRotation * _rigidbody.velocity;
            }
            _oldRotation = transform.eulerAngles.y;
        }

        // this is used to add more grip in relation to speed
        private void AddDownForce()
        {
            m_WheelColliders[0].attachedRigidbody.AddForce
                (-transform.up * Downforce * m_WheelColliders[0].attachedRigidbody.velocity.magnitude);
        }

        // checks if the wheels are spinning and is so does three things
        // 1) emits particles
        // 2) plays tiure skidding sounds
        // 3) leaves skidmarks on the ground
        // these effects are controlled through the WheelEffects class
        private void CheckForWheelSpin()
        {
            // loop through all wheels
            for (int i = 0; i < 4; i++)
            {
                WheelHit wheelHit;
                m_WheelColliders[i].GetGroundHit(out wheelHit);

                // is the tire slipping above the given threshhold
                if (Mathf.Abs(wheelHit.forwardSlip) >= SlipLimit || Mathf.Abs(wheelHit.sidewaysSlip) >= SlipLimit)
                {
                    m_WheelEffects[i].EmitTyreSmoke(1);

                    // avoiding all four tires screeching at the same time
                    // if they do it can lead to some strange audio artefacts
                    if (!AnySkidSoundPlaying())
                    {
                        m_WheelEffects[i].PlayAudio();
                    }
                    continue;
                }

                // if it wasnt slipping stop all the audio
                if (m_WheelEffects[i].PlayingAudio)
                {
                    m_WheelEffects[i].StopAudio();
                }
                // end the trail generation
                m_WheelEffects[i].EndSkidTrail();
            }
        }
        private void CheckSlip()
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 steeringDir = m_WheelColliders[i].transform.right;
                Vector3 tireWorldVel = _rigidbody.GetPointVelocity(m_WheelColliders[i].transform.position);
                float steeringVel = Vector3.Dot(steeringDir, tireWorldVel);

                if (steeringVel > SlipLimit)
                {
                    m_WheelEffects[i].EmitTyreSmoke(1);

                    if (!AnySkidSoundPlaying())
                    {
                        m_WheelEffects[i].PlayAudio();
                    }
                    continue;
                }

                if (m_WheelEffects[i].PlayingAudio)
                {
                    m_WheelEffects[i].StopAudio();
                }

                m_WheelEffects[i].EndSkidTrail();
            }
        }

        // crude traction control that reduces the power to wheel if the car is wheel spinning too much
        private void TractionControl()
        {
            WheelHit wheelHit;
            switch (_carDriveType)
            {
                case CarDriveType.FourWheelDrive:
                    // loop through all wheels
                    for (int i = 0; i < 4; i++)
                    {
                        m_WheelColliders[i].GetGroundHit(out wheelHit);

                        AdjustTorque(wheelHit.forwardSlip);
                    }
                    break;

                case CarDriveType.RearWheelDrive:
                    m_WheelColliders[2].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);

                    m_WheelColliders[3].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);
                    break;

                case CarDriveType.FrontWheelDrive:
                    m_WheelColliders[0].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);

                    m_WheelColliders[1].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);
                    break;
            }
        }

        private void AdjustTorque(float forwardSlip)
        {
            if (forwardSlip >= SlipLimit && _currentTorque >= 0)
            {
                _currentTorque -= 10 * TractionCtrl;
            }
            else
            {
                _currentTorque += 10 * TractionCtrl;
                if (_currentTorque > _vehicleSettings.FullTorqueOverAllWheels)
                {
                    _currentTorque = _vehicleSettings.FullTorqueOverAllWheels;
                }
            }
        }

        private bool AnySkidSoundPlaying()
        {
            for (int i = 0; i < 4; i++)
            {
                if (m_WheelEffects[i].PlayingAudio)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
