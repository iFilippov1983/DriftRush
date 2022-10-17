using RaceManager.Tools;
using Sirenix.OdinInspector;
using System;
using System.Collections;
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

        [SerializeField] Wheel FrontLeftWheel;
        [SerializeField] Wheel FrontRightWheel;
        [SerializeField] Wheel RearLeftWheel;
        [SerializeField] Wheel RearRightWheel;
        [SerializeField] Transform COM;
        [SerializeField] List<ParticleSystem> BackFireParticles = new List<ParticleSystem>();

        [SerializeField] 
        private CarConfig _carConfig;
        private CarSelfRighting _carSelfRighting;

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
        float AngularVelucityInMaxAngle { get { return _carConfig.AngularVelocityInMaxAngle; } }
        float AngularVelucityInMinAngle { get { return _carConfig.AngularVelocityInMinAngle; } }

        #endregion //Properties of drift Settings

        public string ID => _id;
        public CarConfig CarConfig => _carConfig;
        public CarSelfRighting CarSelfRighting => _carSelfRighting;
        public Wheel[] Wheels { get; private set; }                                     //All wheels, public link.			
        public Action BackFireAction;                                            //Backfire invoked when cut off (You can add a invoke when changing gears).

        float[] AllGearsRatio;                                                           //All gears (Reverce, neutral and all forward).

        Rigidbody _rB;
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
        public float SpeedInDesiredUnits => _carConfig.SpeedType == SpeedType.KPH ? CurrentSpeed * C.KPHFactor : CurrentSpeed * C.MPHFactor;
        public int CarDirection { get { return CurrentSpeed < 1 ? 0 : (VelocityAngle < 90 && VelocityAngle > -90 ? 1 : -1); } }

        float CurrentSteerAngle;
        float CurrentAcceleration;
        float CurrentBrake;

        bool InHandBrake;
        private bool _raceStarted = true;

        int FirstDriveWheel;
        int LastDriveWheel;

//        private void Awake()
//        {
//            _carSelfRighting = GetComponent<CarSelfRighting>();
//            _id = MakeId();
//            RB.centerOfMass = COM.localPosition;

//            Copy wheels in public property
//            Wheels = new Wheel[4] {
//            FrontLeftWheel,
//            FrontRightWheel,
//            RearLeftWheel,
//            RearRightWheel
//        };

//        Set drive wheel.
//            switch (DriveType)
//            {
//                case DriveType.AWD:
//                    FirstDriveWheel = 0;
//                    LastDriveWheel = 3;
//                    break;
//                case DriveType.FWD:
//                    FirstDriveWheel = 0;
//                    LastDriveWheel = 1;
//                    break;
//                case DriveType.RWD:
//                    FirstDriveWheel = 2;
//                    LastDriveWheel = 3;
//                    break;
//            }

//    Divide the motor torque by the count of driving wheels
//            MaxMotorTorque = _carConfig.MaxMotorTorque / (LastDriveWheel - FirstDriveWheel + 1);


//            Calculated gears ratio with main ratio
//            AllGearsRatio = new float[GearsRatio.Length + 2];
//            AllGearsRatio[0] = ReversGearRatio* MainRatio;
//    AllGearsRatio[1] = 0;
//            for (int i = 0; i<GearsRatio.Length; i++)
//            {
//                AllGearsRatio[i + 2] = GearsRatio[i] * MainRatio;
//}

//foreach (var particles in BackFireParticles)
//{
//    BackFireAction += () => particles.Emit(2);
//}
//        }

        public void Initialize(CarConfig carConfig)
        {
            _carConfig = carConfig;
            _carSelfRighting = GetComponent<CarSelfRighting>();
            _id = MakeId();
            RB.centerOfMass = COM.localPosition;

            //Copy wheels in public property
            Wheels = new Wheel[4] {
            FrontLeftWheel,
            FrontRightWheel,
            RearLeftWheel,
            RearRightWheel
            };

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

            foreach (var particles in BackFireParticles)
            {
                BackFireAction += () => particles.Emit(2);
            }
        }

        /// <summary>
        /// Update controls of car, from user control (TODO AI control).
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
                float currentAngularProcent = absAngle / MaxAngularVelocityHelpAngle;

                var currAngle = RB.angularVelocity;

                if (VelocityAngle * CurrentSteerAngle > 0)
                {
                    //Turn to the side opposite to the angle. To change the angular velocity.
                    var angularVelocityMagnitudeHelp = OppositeAngularVelocityHelpPower * CurrentSteerAngle * Time.fixedDeltaTime;
                    currAngle.y += angularVelocityMagnitudeHelp * currentAngularProcent;
                }
                else if (!Mathf.Approximately(CurrentSteerAngle, 0))
                {
                    //Turn to the side positive to the angle. To change the angular velocity.
                    var angularVelocityMagnitudeHelp = PositiveAngularVelocityHelpPower * CurrentSteerAngle * Time.fixedDeltaTime;
                    currAngle.y += angularVelocityMagnitudeHelp * (1 - currentAngularProcent);
                }

                //Clamp and apply of angular velocity.
                var maxMagnitude = ((AngularVelucityInMaxAngle - AngularVelucityInMinAngle) * currentAngularProcent) + AngularVelucityInMinAngle;
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
                    if (Wheels[i].CurrentForwardSleep > MaxForwardSlipToBlockChangeGear)
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

        //[SerializeField] private Wheel _frontLeftWheel;
        //[SerializeField] private Wheel _frontRightWheel;
        //[SerializeField] private Wheel _rearLeftWheel;
        //[SerializeField] private Wheel _rearRightWheel;
        //[SerializeField] private Transform _com;
        //[SerializeField] private List<ParticleSystem> _backFireParticles = new List<ParticleSystem>();

        //private Rigidbody _rB;
        ////[SerializeField]
        //private CarConfig _carConfig;
        //private DriverProfile _driverProfile;

        //#region Properties of car parameters

        //float MaxMotorTorque;
        //float MaxSteerAngle { get { return CarConfig.MaxSteerAngle; } }
        //DriveType DriveType { get { return CarConfig.DriveType; } }
        //bool AutomaticGearBox { get { return CarConfig.AutomaticGearBox; } }
        //AnimationCurve MotorTorqueFromRpmCurve { get { return CarConfig.MotorTorqueFromRpmCurve; } }
        //float MaxRPM { get { return CarConfig.MaxRPM; } }
        //float MinRPM { get { return CarConfig.MinRPM; } }
        //float CutOffRPM { get { return CarConfig.CutOffRPM; } }
        //float CutOffOffsetRPM { get { return CarConfig.CutOffOffsetRPM; } }
        //float RpmToNextGear { get { return CarConfig.RpmToNextGear; } }
        //float RpmToPrevGear { get { return CarConfig.RpmToPrevGear; } }
        //float MaxForwardSlipToBlockChangeGear { get { return CarConfig.MaxForwardSlipToBlockChangeGear; } }
        //float RpmEngineToRpmWheelsLerpSpeed { get { return CarConfig.RpmEngineToRpmWheelsLerpSpeed; } }
        //float[] GearsRatio { get { return CarConfig.GearsRatio; } }
        //float MainRatio { get { return CarConfig.MainRatio; } }
        //float ReversGearRatio { get { return CarConfig.ReversGearRatio; } }
        //float MaxBrakeTorque { get { return CarConfig.MaxBrakeTorque; } }


        //#endregion //Properties of car parameters

        //#region Properties of drift Settings

        //bool EnableSteerAngleMultiplier { get { return CarConfig.EnableSteerAngleMultiplier; } }
        //float MinSteerAngleMultiplier { get { return CarConfig.MinSteerAngleMultiplier; } }
        //float MaxSteerAngleMultiplier { get { return CarConfig.MaxSteerAngleMultiplier; } }
        //float MaxSpeedForMinAngleMultiplier { get { return CarConfig.MaxSpeedForMinAngleMultiplier; } }
        //float SteerAngleChangeSpeed { get { return CarConfig.SteerAngleChangeSpeed; } }
        //float MinSpeedForSteerHelp { get { return CarConfig.MinSpeedForSteerHelp; } }
        //float HelpSteerPower { get { return CarConfig.HelpSteerPower; } }
        //float OppositeAngularVelocityHelpPower { get { return CarConfig.OppositeAngularVelocityHelpPower; } }
        //float PositiveAngularVelocityHelpPower { get { return CarConfig.PositiveAngularVelocityHelpPower; } }
        //float MaxAngularVelocityHelpAngle { get { return CarConfig.MaxAngularVelocityHelpAngle; } }
        //float AngularVelucityInMaxAngle { get { return CarConfig.AngularVelucityInMaxAngle; } }
        //float AngularVelucityInMinAngle { get { return CarConfig.AngularVelucityInMinAngle; } }

        //#endregion //Properties of drift Settings

        //private int _firstDriveWheel;
        //private int _lastDriveWheel;

        //private float[] _allGearsRatio;
        //private float _currentSteerAngle;
        //private float _currentAcceleration;
        //private float _currentBrake;

        //private bool _inHandBrake;
        //private bool _onTrack;

        //public string ID => _id;
        //public CarConfig CarConfig => _carConfig;
        //public Rigidbody RB
        //{
        //    get
        //    {
        //        if (!_rB)
        //            _rB = GetComponent<Rigidbody>();
        //        return _rB;
        //    }
        //}

        //CarSelfRighting _carSelfRighting;
        //public CarSelfRighting CarSelfRighting
        //{
        //    get
        //    {
        //        if (!_carSelfRighting)
        //        {
        //            _carSelfRighting.GetComponent<CarSelfRighting>();
        //            _carSelfRighting.Setup(Wheels);
        //        }

        //        return _carSelfRighting;
        //    }
        //}

        //public float CurrentMaxSlip { get; private set; }//Max slip of all wheels.
        //public int CurrentMaxSlipWheelIndex { get; private set; }//Max slip wheel index.
        //public float CurrentSpeed { get; private set; }//Speed, magnitude of velocity.
        //public float SpeedInHour { get { return CurrentSpeed * C.KPHMult; } }
        //public int CarDirection { get { return CurrentSpeed < 1 ? 0 : (VelocityAngle < 90 && VelocityAngle > -90 ? 1 : -1); } }
        //public float VelocityMagnitude => _rB.velocity.magnitude;
        //public Vector3 Velocity => _rB.velocity;
        //public Wheel[] Wheels { get; private set; }

        ////Backfire invoked when cut off
        //public Action BackFireAction;

        //private void Awake()
        //{
        //    _id = MakeId();
        //    RB.centerOfMass = _com.localPosition;

        //    //Copy wheels in public property
        //    Wheels = new Wheel[4] {
        //          _frontLeftWheel,
        //          _frontRightWheel,
        //          _rearLeftWheel,
        //          _rearRightWheel
        //          };

        //    ////Set drive wheel.
        //    //switch (DriveType)
        //    //{
        //    //    case DriveType.AWD:
        //    //        _firstDriveWheel = 0;
        //    //        _lastDriveWheel = 3;
        //    //        break;
        //    //    case DriveType.FWD:
        //    //        _firstDriveWheel = 0;
        //    //        _lastDriveWheel = 1;
        //    //        break;
        //    //    case DriveType.RWD:
        //    //        _firstDriveWheel = 2;
        //    //        _lastDriveWheel = 3;
        //    //        break;
        //    //}

        //    //Divide the motor torque by the count of driving wheels
        //    //MaxMotorTorque = CarConfig.MaxMotorTorque / (_lastDriveWheel - _firstDriveWheel + 1);


        //    //Calculated gears ratio with main ratio
        //    //_allGearsRatio = new float[GearsRatio.Length + 2];
        //    //_allGearsRatio[0] = ReversGearRatio * MainRatio;
        //    //_allGearsRatio[1] = 0;
        //    //for (int i = 0; i < GearsRatio.Length; i++)
        //    //{
        //    //    _allGearsRatio[i + 2] = GearsRatio[i] * MainRatio;
        //    //}

        //    //foreach (var particles in _backFireParticles)
        //    //{
        //    //    BackFireAction += () => particles.Emit(2);
        //    //}
        //}

        //public void DebugSelf()
        //{
        //    //Debug.Log($"Accel: {_currentAcceleration}; On track: {_onTrack} ");
        //}

        //public void Initialize(CarConfig carConfig, DriverProfile driverProfile)
        //{
        //    _carConfig = carConfig;
        //    _driverProfile = driverProfile;
        //    _onTrack = _driverProfile.CarState.Value == CarState.OnTrack ? true : false;

        //    //_id = MakeId();

        //    //RB.centerOfMass = _com.localPosition;

        //    ////Copy wheels in public property
        //    //Wheels = new Wheel[4] {
        //    //_frontLeftWheel,
        //    //_frontRightWheel,
        //    //_rearLeftWheel,
        //    //_rearRightWheel
        //    //};

        //    //Set drive wheel.
        //    switch (DriveType)
        //    {
        //        case DriveType.AWD:
        //            _firstDriveWheel = 0;
        //            _lastDriveWheel = 3;
        //            break;
        //        case DriveType.FWD:
        //            _firstDriveWheel = 0;
        //            _lastDriveWheel = 1;
        //            break;
        //        case DriveType.RWD:
        //            _firstDriveWheel = 2;
        //            _lastDriveWheel = 3;
        //            break;
        //    }

        //    ////Divide the motor torque by the count of driving wheels
        //    MaxMotorTorque = CarConfig.MaxMotorTorque / (_lastDriveWheel - _firstDriveWheel + 1);


        //    //Calculated gears ratio with main ratio
        //    _allGearsRatio = new float[GearsRatio.Length + 2];
        //    _allGearsRatio[0] = ReversGearRatio * MainRatio;
        //    _allGearsRatio[1] = 0;
        //    for (int i = 0; i < GearsRatio.Length; i++)
        //    {
        //        _allGearsRatio[i + 2] = GearsRatio[i] * MainRatio;
        //    }

        //    foreach (var particles in _backFireParticles)
        //    {
        //        BackFireAction += () => particles.Emit(2);
        //    }
        //}

        ///// <summary>
        ///// Update controls of car, from user control.
        ///// </summary>
        ///// <param name="horizontal">Turn direction</param>
        ///// <param name="vertical">Acceleration</param>
        ///// <param name="brake">Brake</param>
        //public void UpdateControls(float horizontal, float vertical, bool handBrake)
        //{
        //    float targetSteerAngle = horizontal * MaxSteerAngle;

        //    if (EnableSteerAngleMultiplier)
        //    {
        //        targetSteerAngle *= Mathf.Clamp(1 - SpeedInHour / MaxSpeedForMinAngleMultiplier, MinSteerAngleMultiplier, MaxSteerAngleMultiplier);
        //    }

        //    _currentSteerAngle = Mathf.MoveTowards(_currentSteerAngle, targetSteerAngle, Time.deltaTime * SteerAngleChangeSpeed);

        //    _currentAcceleration = vertical;
        //    _inHandBrake = handBrake;
        //    _onTrack = _driverProfile.CarState.Value == CarState.OnTrack ? true : false;
        //}

        //private void Update()
        //{
        //    for (int i = 0; i < Wheels.Length; i++)
        //    {
        //        Wheels[i].UpdateVisual();
        //    }
        //}

        //private void FixedUpdate()
        //{

        //    CurrentSpeed = RB.velocity.magnitude;

        //    UpdateSteerAngleLogic();
        //    UpdateRpmAndTorqueLogic();

        //    //Find max slip and update braking ground logic.
        //    CurrentMaxSlip = Wheels[0].CurrentMaxSlip;
        //    CurrentMaxSlipWheelIndex = 0;

        //    if (_inHandBrake)
        //    {
        //        _rearLeftWheel.WheelCollider.brakeTorque = MaxBrakeTorque;
        //        _rearRightWheel.WheelCollider.brakeTorque = MaxBrakeTorque;
        //        _frontLeftWheel.WheelCollider.brakeTorque = 0;
        //        _frontRightWheel.WheelCollider.brakeTorque = 0;
        //    }

        //    for (int i = 0; i < Wheels.Length; i++)
        //    {
        //        if (!_inHandBrake)
        //        {
        //            Wheels[i].WheelCollider.brakeTorque = _currentBrake;
        //        }

        //        Wheels[i].FixedUpdate();

        //        if (CurrentMaxSlip < Wheels[i].CurrentMaxSlip)
        //        {
        //            CurrentMaxSlip = Wheels[i].CurrentMaxSlip;
        //            CurrentMaxSlipWheelIndex = i;
        //        }
        //    }

        //}

        //#region Steer help logic

        ////Angle between forward point and velocity point.
        //public float VelocityAngle { get; private set; }

        ///// <summary>
        ///// Update all helpers logic.
        ///// </summary>
        //private void UpdateSteerAngleLogic()
        //{
        //    var needHelp = SpeedInHour > MinSpeedForSteerHelp && CarDirection > 0;
        //    float targetAngle = 0;
        //    VelocityAngle = -Vector3.SignedAngle(RB.velocity, transform.TransformDirection(Vector3.forward), Vector3.up);

        //    if (needHelp)
        //    {
        //        //Wheel turning helper.
        //        targetAngle = Mathf.Clamp(VelocityAngle * HelpSteerPower, -MaxSteerAngle, MaxSteerAngle);
        //    }

        //    //Wheel turn limitation.
        //    targetAngle = Mathf.Clamp(targetAngle + _currentSteerAngle, -(MaxSteerAngle + 10), MaxSteerAngle + 10);

        //    //Front wheel turn.
        //    Wheels[0].WheelCollider.steerAngle = targetAngle;
        //    Wheels[1].WheelCollider.steerAngle = targetAngle;

        //    if (needHelp)
        //    {
        //        //Angular velocity helper.
        //        var absAngle = Mathf.Abs(VelocityAngle);

        //        //Get current procent help angle.
        //        float currentAngularProcent = absAngle / MaxAngularVelocityHelpAngle;

        //        var currAngle = RB.angularVelocity;

        //        if (VelocityAngle * _currentSteerAngle > 0)
        //        {
        //            //Turn to the side opposite to the angle. To change the angular velocity.
        //            var angularVelocityMagnitudeHelp = OppositeAngularVelocityHelpPower * _currentSteerAngle * Time.fixedDeltaTime;
        //            currAngle.y += angularVelocityMagnitudeHelp * currentAngularProcent;
        //        }
        //        else if (!Mathf.Approximately(_currentSteerAngle, 0))
        //        {
        //            //Turn to the side positive to the angle. To change the angular velocity.
        //            var angularVelocityMagnitudeHelp = PositiveAngularVelocityHelpPower * _currentSteerAngle * Time.fixedDeltaTime;
        //            currAngle.y += angularVelocityMagnitudeHelp * (1 - currentAngularProcent);
        //        }

        //        //Clamp and apply of angular velocity.
        //        var maxMagnitude = ((AngularVelucityInMaxAngle - AngularVelucityInMinAngle) * currentAngularProcent) + AngularVelucityInMinAngle;
        //        currAngle.y = Mathf.Clamp(currAngle.y, -maxMagnitude, maxMagnitude);
        //        RB.angularVelocity = currAngle;
        //    }
        //}

        //#endregion //Steer help logic

        //#region Rpm and torque logic

        //public int CurrentGear { get; private set; }
        //public int CurrentGearIndex { get { return CurrentGear + 1; } }
        //public float EngineRPM { get; private set; }
        //public float GetMaxRPM { get { return MaxRPM; } }
        //public float GetMinRPM { get { return MinRPM; } }
        //public float GetInCutOffRPM { get { return CutOffRPM - CutOffOffsetRPM; } }

        //float CutOffTimer;
        //bool InCutOff;

        //private void UpdateRpmAndTorqueLogic()
        //{

        //    if (InCutOff)
        //    {
        //        if (CutOffTimer > 0)
        //        {
        //            CutOffTimer -= Time.fixedDeltaTime;
        //            EngineRPM = Mathf.Lerp(EngineRPM, GetInCutOffRPM, RpmEngineToRpmWheelsLerpSpeed * Time.fixedDeltaTime);
        //        }
        //        else
        //        {
        //            InCutOff = false;
        //        }
        //    }

        //    if (!_onTrack)
        //    {
        //        if (InCutOff)
        //            return;

        //        float rpm = _currentAcceleration > 0 ? MaxRPM : MinRPM;
        //        float speed = _currentAcceleration > 0 ? RpmEngineToRpmWheelsLerpSpeed : RpmEngineToRpmWheelsLerpSpeed * 0.2f;
        //        EngineRPM = Mathf.Lerp(EngineRPM, rpm, speed * Time.fixedDeltaTime);
        //        if (EngineRPM >= CutOffRPM)
        //        {
        //            PlayBackfireWithProbability();
        //            InCutOff = true;
        //            CutOffTimer = CarConfig.CutOffTime;
        //        }
        //        return;
        //    }

        //    //Get drive wheel with MinRPM.
        //    float minRPM = 0;
        //    for (int i = _firstDriveWheel + 1; i <= _lastDriveWheel; i++)
        //    {
        //        minRPM += Wheels[i].WheelCollider.rpm;
        //    }

        //    minRPM /= _lastDriveWheel - _firstDriveWheel + 1;

        //    if (!InCutOff)
        //    {
        //        //Calculate the rpm based on rpm of the wheel and current gear ratio.
        //        float targetRPM = ((minRPM + 20) * _allGearsRatio[CurrentGearIndex]).Abs();              //+20 for normal work CutOffRPM
        //        targetRPM = Mathf.Clamp(targetRPM, MinRPM, MaxRPM);
        //        EngineRPM = Mathf.Lerp(EngineRPM, targetRPM, RpmEngineToRpmWheelsLerpSpeed * Time.fixedDeltaTime);
        //    }

        //    if (EngineRPM >= CutOffRPM)
        //    {
        //        PlayBackfireWithProbability();
        //        InCutOff = true;
        //        CutOffTimer = CarConfig.CutOffTime;
        //        return;
        //    }

        //    if (!Mathf.Approximately(_currentAcceleration, 0))
        //    {
        //        //If the direction of the car is the same as Current Acceleration.
        //        if (CarDirection * _currentAcceleration >= 0)
        //        {
        //            _currentBrake = 0;

        //            float motorTorqueFromRpm = MotorTorqueFromRpmCurve.Evaluate(EngineRPM * 0.001f);
        //            var motorTorque = _currentAcceleration * (motorTorqueFromRpm * (MaxMotorTorque * _allGearsRatio[CurrentGearIndex]));
        //            if (Mathf.Abs(minRPM) * _allGearsRatio[CurrentGearIndex] > MaxRPM)
        //            {
        //                motorTorque = 0;
        //            }

        //            //If the rpm of the wheel is less than the max rpm engine * current ratio, then apply the current torque for wheel, else not torque for wheel.
        //            float maxWheelRPM = _allGearsRatio[CurrentGearIndex] * EngineRPM;
        //            for (int i = _firstDriveWheel; i <= _lastDriveWheel; i++)
        //            {
        //                if (Wheels[i].WheelCollider.rpm <= maxWheelRPM)
        //                {
        //                    Wheels[i].WheelCollider.motorTorque = motorTorque;
        //                }
        //                else
        //                {
        //                    Wheels[i].WheelCollider.motorTorque = 0;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            _currentBrake = MaxBrakeTorque;
        //        }
        //    }
        //    else
        //    {
        //        _currentBrake = 0;

        //        for (int i = _firstDriveWheel; i <= _lastDriveWheel; i++)
        //        {
        //            Wheels[i].WheelCollider.motorTorque = 0;
        //        }
        //    }

        //    //Automatic gearbox logic. 
        //    if (AutomaticGearBox)
        //    {

        //        bool forwardIsSlip = false;
        //        for (int i = _firstDriveWheel; i <= _lastDriveWheel; i++)
        //        {
        //            if (Wheels[i].CurrentForwardSleep > MaxForwardSlipToBlockChangeGear)
        //            {
        //                forwardIsSlip = true;
        //                break;
        //            }
        //        }

        //        float prevRatio = 0;
        //        float newRatio = 0;

        //        if (!forwardIsSlip && EngineRPM > RpmToNextGear && CurrentGear >= 0 && CurrentGear < (_allGearsRatio.Length - 2))
        //        {
        //            prevRatio = _allGearsRatio[CurrentGearIndex];
        //            CurrentGear++;
        //            newRatio = _allGearsRatio[CurrentGearIndex];
        //        }
        //        else if (EngineRPM < RpmToPrevGear && CurrentGear > 0 && (EngineRPM <= MinRPM || CurrentGear != 1))
        //        {
        //            prevRatio = _allGearsRatio[CurrentGearIndex];
        //            CurrentGear--;
        //            newRatio = _allGearsRatio[CurrentGearIndex];
        //        }

        //        if (!Mathf.Approximately(prevRatio, 0) && !Mathf.Approximately(newRatio, 0))
        //        {
        //            EngineRPM = Mathf.Lerp(EngineRPM, EngineRPM * (newRatio / prevRatio), RpmEngineToRpmWheelsLerpSpeed * Time.fixedDeltaTime); //EngineRPM * (prevRatio / newRatio);// 
        //        }

        //        if (CarDirection <= 0 && _currentAcceleration < 0)
        //        {
        //            CurrentGear = -1;
        //        }
        //        else if (CurrentGear <= 0 && CarDirection >= 0 && _currentAcceleration > 0)
        //        {
        //            CurrentGear = 1;
        //        }
        //        else if (CarDirection == 0 && _currentAcceleration == 0)
        //        {
        //            CurrentGear = 0;
        //        }
        //    }

        //    //TODO manual gearbox logic.
        //}
        //void PlayBackfireWithProbability()
        //{
        //    PlayBackfireWithProbability(CarConfig.ProbabilityBackfire);
        //}

        //void PlayBackfireWithProbability(float probability)
        //{
        //    if (UnityEngine.Random.Range(0f, 1f) <= probability)
        //    {
        //        BackFireAction.SafeInvoke();
        //    }
        //}

        //#endregion


        //private void OnDrawGizmosSelected()
        //{
        //    var centerPos = transform.position;
        //    var velocity = transform.position + Vector3.ClampMagnitude(RB.velocity, 4);
        //    var forwardPos = transform.TransformPoint(Vector3.forward * 4);

        //    Gizmos.color = Color.green;

        //    Gizmos.DrawWireSphere(centerPos, 0.2f);
        //    Gizmos.DrawLine(centerPos, velocity);
        //    Gizmos.DrawLine(centerPos, forwardPos);

        //    Gizmos.color = Color.yellow;
        //    Gizmos.DrawWireSphere(forwardPos, 0.2f);

        //    Gizmos.color = Color.blue;
        //    Gizmos.DrawWireSphere(velocity, 0.2f);

        //    Gizmos.color = Color.white;
        //}

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
