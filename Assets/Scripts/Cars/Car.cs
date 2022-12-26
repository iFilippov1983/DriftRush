using RaceManager.Tools;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RaceManager.Cars
{
    /// <summary>
    /// Main car controller
    /// </summary>

    [RequireComponent(typeof(Rigidbody))]
	public class Car : MonoBehaviour
	{
        [ShowInInspector, ReadOnly]
        private string _id;

        [ShowInInspector, ReadOnly, FoldoutGroup("Default effects settings")] 
        private const float SlipFoorGenerateParticles = 0.3f;
        [ShowInInspector, ReadOnly, FoldoutGroup("Default effects settings")]
        private readonly Vector3 TrailOffsetLeft = new Vector3(0.1f, 0.05f, 0f);
        [ShowInInspector, ReadOnly, FoldoutGroup("Default effects settings")]
        private readonly Vector3 TrailOffsetRight = new Vector3(-0.1f, 0.05f, 0f);

        [SerializeField] Wheel FrontLeftWheel;
        [SerializeField] Wheel FrontRightWheel;
        [SerializeField] Wheel RearLeftWheel;
        [SerializeField] Wheel RearRightWheel;
        [SerializeField] Transform COM;
        [SerializeField] Transform _cameraLookTarget;
        [SerializeField] Transform _cameraFollowTarget;
        [SerializeField] List<ParticleSystem> BackFireParticles = new List<ParticleSystem>();
        [SerializeField] private CarConfig _carConfig;

        private CarSelfRighting _carSelfRighting;
        private CarBody _carBody;
        private Rigidbody _rB;
        private Wheel[] _wheels;

        /// <summary>
        /// Action performed at the moment of collision event.
        /// </summary>
        public event Action<Car, Collision> CollisionAction;

        /// <summary>
        /// Action performed at the moment of collision stay event.
        /// </summary>
        public event Action<Car, Collision> CollisionStayAction;

        /// <summary>
        /// Backfire invoked when cut off (You can add a invoke when changing gears).
        /// </summary>
        public Action BackFireAction;

        /// <summary>
        /// Action performed when the vehicle is reset.
        /// </summary>
        public Action ResetVehicleAction;

        #region Properties of car parameters

        float MaxMotorTorque;
        float MaxSteerAngle { get { return _carConfig.MaxSteerAngle; } }
        DriveType DriveType { get { return _carConfig.DriveType; } }
        bool AutomaticGearBox { get { return _carConfig.AutomaticGearBox; } }
        AnimationCurve MotorTorqueFromRpmCurve { get { return _carConfig.MotorTorqueFromRpmCurve; } }
        float MaxRPM { get { return _carConfig.MaxRPM; } }
        float MinRPM { get { return _carConfig.MinRPM; } }
        float CutOffRPM { get { return _carConfig.CutOffRPM; } }
        float CutOffOffsetRPM { get { return _carConfig.CutOffOffsetRPM; } }
        float RpmToNextGear { get { return _carConfig.RpmToNextGear; } }
        float RpmToPrevGear { get { return _carConfig.RpmToPrevGear; } }
        float MaxForwardSlipToBlockChangeGear { get { return _carConfig.MaxForwardSlipToBlockChangeGear; } }
        float RpmEngineToRpmWheelsLerpSpeed { get { return _carConfig.RpmEngineToRpmWheelsLerpSpeed; } }
        float[] GearsRatio { get { return _carConfig.GearsRatio; } }
        float MainRatio { get { return _carConfig.MainRatio; } }
        float ReversGearRatio { get { return _carConfig.ReversGearRatio; } }
        float MaxBrakeTorque { get { return _carConfig.MaxBrakeTorque; } }


        #endregion //Properties of car parameters

        #region Properties of drift Settings

        bool EnableSteerAngleMultiplier { get { return _carConfig.EnableSteerAngleMultiplier; } }
        float MinSteerAngleMultiplier { get { return _carConfig.MinSteerAngleMultiplier; } }
        float MaxSteerAngleMultiplier { get { return _carConfig.MaxSteerAngleMultiplier; } }
        float MaxSpeedForMinAngleMultiplier { get { return _carConfig.MaxSpeedForMinAngleMultiplier; } }
        float SteerAngleChangeSpeed { get { return _carConfig.SteerAngleChangeSpeed; } }
        float MinSpeedForSteerHelp { get { return _carConfig.MinSpeedForSteerHelp; } }
        float HelpSteerPower { get { return _carConfig.HelpSteerPower; } }
        float OppositeAngularVelocityHelpPower { get { return _carConfig.OppositeAngularVelocityHelpPower; } }
        float PositiveAngularVelocityHelpPower { get { return _carConfig.PositiveAngularVelocityHelpPower; } }
        float MaxAngularVelocityHelpAngle { get { return _carConfig.MaxAngularVelocityHelpAngle; } }
        float AngularVelocityInMaxAngle { get { return _carConfig.AngularVelocityInMaxAngle; } }
        float AngularVelocityInMinAngle { get { return _carConfig.AngularVelocityInMinAngle; } }

        #endregion //Properties of drift Settings

        public string ID => _id;
        public CarConfig CarConfig => _carConfig;
        public CarSelfRighting CarSelfRighting => _carSelfRighting;
        public Transform CameraLookTarget => _cameraLookTarget;
        public Transform CameraFollowTarget => _cameraFollowTarget;
        

        /// <summary>
        /// All gears (Reverce, neutral and all forward).
        /// </summary>
        float[] AllGearsRatio;

        public Wheel[] Wheels 
        { 
            get
            {
                if (_wheels == null)
                    InitWheels();

                return _wheels;
            }

            private set
            {
                _wheels = value;
            }
        }

        public Rigidbody RB
        {
            get
            {
                if (!_rB)
                {
                    _rB = GetComponent<Rigidbody>();
                }
                return _rB;
            }
        }

        public float CurrentMaxSlip { get; private set; }                       //Max slip of all wheels.
        public int CurrentMaxSlipWheelIndex { get; private set; }               //Max slip wheel index.
        public float CurrentSpeed { get; private set; }                         //Speed, magnitude of velocity.
        public int VehicleDirection { get { return CurrentSpeed < 1 ? 0 : (VelocityAngle.Abs() < 90 ? 1 : -1); } }
        public float SpeedInDesiredUnits => _carConfig.SpeedType == SpeedType.KPH ? CurrentSpeed * C.KPHFactor : CurrentSpeed * C.MPHFactor;
        public int CarDirection { get { return CurrentSpeed < 1 ? 0 : (VelocityAngle < 90 && VelocityAngle > -90 ? 1 : -1); } }
        public bool IsVisible => _carBody.IsVisible;

        float CurrentSteerAngle;
        float CurrentAcceleration;
        float CurrentBrake;

        bool InHandBrake;
        private bool _raceStarted = true;

        int FirstDriveWheel;
        int LastDriveWheel;

        public void Initialize(CarConfig carConfig)
        {
            _carConfig = carConfig;
            _carSelfRighting = GetComponent<CarSelfRighting>();
            _carBody = GetComponent<CarBody>();
            _id = MakeId();

            RB.centerOfMass = COM.localPosition;

            InitWheels();

            //Set drive wheel.
            switch (DriveType)
            {
                case DriveType.AWD:
                    FirstDriveWheel = 0;
                    LastDriveWheel = 3;
                    break;
                case DriveType.FWD:
                    FirstDriveWheel = 0;
                    LastDriveWheel = 1;
                    break;
                case DriveType.RWD:
                    FirstDriveWheel = 2;
                    LastDriveWheel = 3;
                    break;
            }

            //Divide the motor torque by the count of driving wheels
            MaxMotorTorque = _carConfig.MaxMotorTorque / (LastDriveWheel - FirstDriveWheel + 1);


            //Calculated gears ratio with main ratio
            AllGearsRatio = new float[GearsRatio.Length + 2];
            AllGearsRatio[0] = ReversGearRatio * MainRatio;
            AllGearsRatio[1] = 0;
            for (int i = 0; i < GearsRatio.Length; i++)
            {
                AllGearsRatio[i + 2] = GearsRatio[i] * MainRatio;
            }

            _carSelfRighting.OnCarRespawn += CarResetNotification;
        }

        private void InitWheels()
        {
            FrontLeftWheel.SlipForGenerateParticle = SlipFoorGenerateParticles;
            FrontRightWheel.SlipForGenerateParticle = SlipFoorGenerateParticles;
            RearLeftWheel.SlipForGenerateParticle = SlipFoorGenerateParticles;
            RearRightWheel.SlipForGenerateParticle = SlipFoorGenerateParticles;

            FrontLeftWheel.TrailOffset = TrailOffsetLeft;
            FrontRightWheel.TrailOffset = TrailOffsetRight;
            RearLeftWheel.TrailOffset = TrailOffsetLeft;
            RearRightWheel.TrailOffset = TrailOffsetRight;

            var config = FrontLeftWheel.WheelColliderHandler.Config;
            config.ForwardFriction = _carConfig.FWheelsForwardFriction;
            config.SidewaysFriction = _carConfig.FWheelsSidewaysFriction;
            FrontLeftWheel.WheelColliderHandler.Config = config;
            FrontLeftWheel.WheelColliderHandler.UpdateConfig();

            config = FrontRightWheel.WheelColliderHandler.Config;
            config.ForwardFriction = _carConfig.FWheelsForwardFriction;
            config.SidewaysFriction = _carConfig.FWheelsSidewaysFriction;
            FrontRightWheel.WheelColliderHandler.Config = config;
            FrontRightWheel.WheelColliderHandler.UpdateConfig();

            config = RearLeftWheel.WheelColliderHandler.Config;
            config.ForwardFriction = _carConfig.RWheelsForwardFriction;
            config.SidewaysFriction = _carConfig.RWheelsSidewaysFriction;
            RearLeftWheel.WheelColliderHandler.Config = config;
            RearLeftWheel.WheelColliderHandler.UpdateConfig();

            config = RearRightWheel.WheelColliderHandler.Config;
            config.ForwardFriction = _carConfig.RWheelsForwardFriction;
            config.SidewaysFriction = _carConfig.RWheelsSidewaysFriction;
            RearRightWheel.WheelColliderHandler.Config = config;
            RearRightWheel.WheelColliderHandler.UpdateConfig();

            _wheels = new Wheel[4] {
            FrontLeftWheel,
            FrontRightWheel,
            RearLeftWheel,
            RearRightWheel
            };
        }

        /// <summary>
        /// Update controls of car, from user/AI controler.
        /// </summary>
        /// <param name="horizontal">Turn direction</param>
        /// <param name="vertical">Acceleration</param>
        /// <param name="brake">Brake</param>
        public void UpdateControls(float horizontal, float vertical, bool handBrake)
        {
            float targetSteerAngle = horizontal * MaxSteerAngle;

            if (EnableSteerAngleMultiplier)
            {
                targetSteerAngle *= Mathf.Clamp(1 - SpeedInDesiredUnits / MaxSpeedForMinAngleMultiplier, MinSteerAngleMultiplier, MaxSteerAngleMultiplier);
            }

            CurrentSteerAngle = Mathf.MoveTowards(CurrentSteerAngle, targetSteerAngle, Time.deltaTime * SteerAngleChangeSpeed);

            CurrentAcceleration = vertical;
            InHandBrake = handBrake;
        }

        #region Unity Functions

        private void Update()
        {
            for (int i = 0; i < Wheels.Length; i++)
            {
                Wheels[i].UpdateVisual();
            }
        }

        private void FixedUpdate()
        {
            CurrentSpeed = RB.velocity.magnitude;

            UpdateSteerAngleLogic();
            UpdateRpmAndTorqueLogic();

            //Find max slip and update braking ground logic.
            CurrentMaxSlip = Wheels[0].CurrentMaxSlip;
            CurrentMaxSlipWheelIndex = 0;

            if (InHandBrake)
            {
                RearLeftWheel.WheelCollider.brakeTorque = MaxBrakeTorque;
                RearRightWheel.WheelCollider.brakeTorque = MaxBrakeTorque;
                FrontLeftWheel.WheelCollider.brakeTorque = 0;
                FrontRightWheel.WheelCollider.brakeTorque = 0;
            }

            for (int i = 0; i < Wheels.Length; i++)
            {
                if (!InHandBrake)
                {
                    Wheels[i].WheelCollider.brakeTorque = CurrentBrake;
                }

                Wheels[i].FixedUpdate();

                if (CurrentMaxSlip < Wheels[i].CurrentMaxSlip)
                {
                    CurrentMaxSlip = Wheels[i].CurrentMaxSlip;
                    CurrentMaxSlipWheelIndex = i;
                }
            }

        }

        public virtual void OnCollisionEnter(Collision collision)
        {
            CollisionAction.SafeInvoke(this, collision);
        }

        private void OnDrawGizmosSelected()
        {
            var centerPos = transform.position;
            var velocity = transform.position + (Vector3.ClampMagnitude(RB.velocity, 4));
            var forwardPos = transform.TransformPoint(Vector3.forward * 4);

            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(centerPos, 0.2f);
            Gizmos.DrawLine(centerPos, velocity);
            Gizmos.DrawLine(centerPos, forwardPos);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(forwardPos, 0.2f);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(velocity, 0.2f);

            Gizmos.color = Color.white;
        }

        private void OnDestroy()
        {
            _carSelfRighting.OnCarRespawn -= CarResetNotification;
        }

        #endregion

        #region Steer help logic

        //Angle between forward point and velocity point.
        public float VelocityAngle { get; private set; }

        /// <summary>
        /// Update all helpers logic.
        /// </summary>
        void UpdateSteerAngleLogic()
        {
            var needHelp = SpeedInDesiredUnits > MinSpeedForSteerHelp && CarDirection > 0;
            float targetAngle = 0;
            VelocityAngle = -Vector3.SignedAngle(RB.velocity, transform.TransformDirection(Vector3.forward), Vector3.up);

            if (needHelp)
            {
                //Wheel turning helper.
                targetAngle = Mathf.Clamp(VelocityAngle * HelpSteerPower, -MaxSteerAngle, MaxSteerAngle);
            }

            //Wheel turn limitation.
            targetAngle = Mathf.Clamp(targetAngle + CurrentSteerAngle, -(MaxSteerAngle + 10), MaxSteerAngle + 10);

            //Front wheel turn.
            Wheels[0].WheelCollider.steerAngle = targetAngle;
            Wheels[1].WheelCollider.steerAngle = targetAngle;

            if (needHelp)
            {
                //Angular velocity helper.
                var absAngle = Mathf.Abs(VelocityAngle);

                //Get current procent help angle.
                float currentAngularPercent = absAngle / MaxAngularVelocityHelpAngle;

                var currAngle = RB.angularVelocity;

                if (VelocityAngle * CurrentSteerAngle > 0)
                {
                    //Turn to the side opposite to the angle. To change the angular velocity.
                    var angularVelocityMagnitudeHelp = OppositeAngularVelocityHelpPower * CurrentSteerAngle * Time.fixedDeltaTime;
                    currAngle.y += angularVelocityMagnitudeHelp * currentAngularPercent;
                }
                else if (!Mathf.Approximately(CurrentSteerAngle, 0))
                {
                    //Turn to the side positive to the angle. To change the angular velocity.
                    var angularVelocityMagnitudeHelp = PositiveAngularVelocityHelpPower * CurrentSteerAngle * Time.fixedDeltaTime;
                    currAngle.y += angularVelocityMagnitudeHelp * (1 - currentAngularPercent);
                }

                //Clamp and apply of angular velocity.
                var maxMagnitude = ((AngularVelocityInMaxAngle - AngularVelocityInMinAngle) * currentAngularPercent) + AngularVelocityInMinAngle;
                currAngle.y = Mathf.Clamp(currAngle.y, -maxMagnitude, maxMagnitude);
                RB.angularVelocity = currAngle;
            }
        }

        #endregion //Steer help logic

        #region Rpm and torque logic

        public int CurrentGear { get; private set; }
        public int CurrentGearIndex { get { return CurrentGear + 1; } }
        public float EngineRPM { get; private set; }
        public float GetMaxRPM { get { return MaxRPM; } }
        public float GetMinRPM { get { return MinRPM; } }
        public float GetInCutOffRPM { get { return CutOffRPM - CutOffOffsetRPM; } }

        float CutOffTimer;
        bool InCutOff;

        void UpdateRpmAndTorqueLogic()
        {

            if (InCutOff)
            {
                if (CutOffTimer > 0)
                {
                    CutOffTimer -= Time.fixedDeltaTime;
                    EngineRPM = Mathf.Lerp(EngineRPM, GetInCutOffRPM, RpmEngineToRpmWheelsLerpSpeed * Time.fixedDeltaTime);
                }
                else
                {
                    InCutOff = false;
                }
            }

            if (!_raceStarted)
            {
                if (InCutOff)
                    return;

                float rpm = CurrentAcceleration > 0 ? MaxRPM : MinRPM;
                float speed = CurrentAcceleration > 0 ? RpmEngineToRpmWheelsLerpSpeed : RpmEngineToRpmWheelsLerpSpeed * 0.2f;
                EngineRPM = Mathf.Lerp(EngineRPM, rpm, speed * Time.fixedDeltaTime);
                if (EngineRPM >= CutOffRPM)
                {
                    PlayBackfireWithProbability();
                    InCutOff = true;
                    CutOffTimer = _carConfig.CutOffTime;
                }
                return;
            }

            //Get drive wheel with MinRPM.
            float minRPM = 0;
            for (int i = FirstDriveWheel + 1; i <= LastDriveWheel; i++)
            {
                minRPM += Wheels[i].WheelCollider.rpm;
            }

            minRPM /= LastDriveWheel - FirstDriveWheel + 1;

            if (!InCutOff)
            {
                //Calculate the rpm based on rpm of the wheel and current gear ratio.
                float targetRPM = ((minRPM + 20) * AllGearsRatio[CurrentGearIndex]).Abs();              //+20 for normal work CutOffRPM
                targetRPM = Mathf.Clamp(targetRPM, MinRPM, MaxRPM);
                EngineRPM = Mathf.Lerp(EngineRPM, targetRPM, RpmEngineToRpmWheelsLerpSpeed * Time.fixedDeltaTime);
            }

            if (EngineRPM >= CutOffRPM)
            {
                PlayBackfireWithProbability();
                InCutOff = true;
                CutOffTimer = _carConfig.CutOffTime;
                return;
            }

            if (!Mathf.Approximately(CurrentAcceleration, 0))
            {
                //If the direction of the car is the same as Current Acceleration.
                if (CarDirection * CurrentAcceleration >= 0)
                {
                    CurrentBrake = 0;

                    float motorTorqueFromRpm = MotorTorqueFromRpmCurve.Evaluate(EngineRPM * 0.001f);
                    var motorTorque = CurrentAcceleration * (motorTorqueFromRpm * (MaxMotorTorque * AllGearsRatio[CurrentGearIndex]));
                    if (Mathf.Abs(minRPM) * AllGearsRatio[CurrentGearIndex] > MaxRPM)
                    {
                        motorTorque = 0;
                    }

                    //If the rpm of the wheel is less than the max rpm engine * current ratio, then apply the current torque for wheel, else not torque for wheel.
                    float maxWheelRPM = AllGearsRatio[CurrentGearIndex] * EngineRPM;
                    for (int i = FirstDriveWheel; i <= LastDriveWheel; i++)
                    {
                        if (Wheels[i].WheelCollider.rpm <= maxWheelRPM)
                        {
                            Wheels[i].WheelCollider.motorTorque = motorTorque;
                        }
                        else
                        {
                            Wheels[i].WheelCollider.motorTorque = 0;
                        }
                    }
                }
                else
                {
                    CurrentBrake = MaxBrakeTorque;
                }
            }
            else
            {
                CurrentBrake = 0;

                for (int i = FirstDriveWheel; i <= LastDriveWheel; i++)
                {
                    Wheels[i].WheelCollider.motorTorque = 0;
                }
            }

            //Automatic gearbox logic. 
            if (AutomaticGearBox)
            {

                bool forwardIsSlip = false;
                for (int i = FirstDriveWheel; i <= LastDriveWheel; i++)
                {
                    if (Wheels[i].CurrentForwardSlip > MaxForwardSlipToBlockChangeGear)
                    {
                        forwardIsSlip = true;
                        break;
                    }
                }

                float prevRatio = 0;
                float newRatio = 0;

                if (!forwardIsSlip && EngineRPM > RpmToNextGear && CurrentGear >= 0 && CurrentGear < (AllGearsRatio.Length - 2))
                {
                    prevRatio = AllGearsRatio[CurrentGearIndex];
                    CurrentGear++;
                    newRatio = AllGearsRatio[CurrentGearIndex];
                }
                else if (EngineRPM < RpmToPrevGear && CurrentGear > 0 && (EngineRPM <= MinRPM || CurrentGear != 1))
                {
                    prevRatio = AllGearsRatio[CurrentGearIndex];
                    CurrentGear--;
                    newRatio = AllGearsRatio[CurrentGearIndex];
                }

                if (!Mathf.Approximately(prevRatio, 0) && !Mathf.Approximately(newRatio, 0))
                {
                    EngineRPM = Mathf.Lerp(EngineRPM, EngineRPM * (newRatio / prevRatio), RpmEngineToRpmWheelsLerpSpeed * Time.fixedDeltaTime); //EngineRPM * (prevRatio / newRatio);// 
                }

                if (CarDirection <= 0 && CurrentAcceleration < 0)
                {
                    CurrentGear = -1;
                }
                else if (CurrentGear <= 0 && CarDirection >= 0 && CurrentAcceleration > 0)
                {
                    CurrentGear = 1;
                }
                else if (CarDirection == 0 && CurrentAcceleration == 0)
                {
                    CurrentGear = 0;
                }
            }
        }
        void PlayBackfireWithProbability()
        {
            PlayBackfireWithProbability(CarConfig.ProbabilityBackfire);
        }

        void PlayBackfireWithProbability(float probability)
        {
            if (UnityEngine.Random.Range(0f, 1f) <= probability)
            {
                BackFireAction.SafeInvoke();
            }
        }

        #endregion

        private void CarResetNotification() => ResetVehicleAction?.Invoke();

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
