using Newtonsoft.Json;
using RaceManager.Tools;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace RaceManager.Cars
{
	[Serializable]
    public class CarConfig 
	{
		public CarName CarName;

		[Header("MAIN SETTINGS")]

		[Title("Drive settings")]
		public SpeedType SpeedType = SpeedType.KPH;
		public DriveType DriveType = DriveType.AWD;
		[Space]

		//==========================================
		[Title("Speed")]

		public float MaxSpeed = 200;

		[Space]
		[Tooltip("Cruise Speed Settings (for Player)")]
		public float CruiseSpeed = 20f;

		//[Tooltip("Represents Max percentage range of Speed AI will use")]
		//[Range(0.01f, 1f)]
		//public float CruiseSpeedPercentMax = 0.8f;

		//[Tooltip("Represents Min percentage range of Speed AI will use")]
		//[Range(0.01f, 1f)]
		//public float CruiseSpeedPercentMin = 0.7f;

		//==========================================
		[Title("Handling")]

		[Range(1f, 90f)]
		public float MaxSteerAngle = 42;

		[Range(0.001f, 1f)]
		[Tooltip("The power of turning the wheels in the direction of the drift")]
		public float HelpSteerPower = 0.2f; //The power of turning the wheels in the direction of the drift.

		public float SteerAngleChangeSpeed = 100f; //Wheel turn speed. Default = 60

		//==========================================
        [Title("Acceleration")]

        public float MaxMotorTorque = 800; //Max motor torque engine (Without GearBox multiplier).

        [Range(0.05f, CutOffRPMFactor - RPMTONextGearVsCutOffDif)]
        [Tooltip("Higher the percentage faster a car accelerates")]
        public float RPMToNextGearPercent = 0.9f;

        public float MaxBrakeTorque = 2000;

		//==========================================
		[Title("Friction")]

        [Range(0f, 1f)]
        public float FWheelsForwardFriction = 0.4f;

        [Range(0f, 1f)]
        public float FWheelsSidewaysFriction = 0.4f;

        [Range(0f, 1f)]
        public float RWheelsForwardFriction = 0.4f;

        [Range(0f, 1f)]
        public float RWheelsSidewaysFriction = 0.4f;

        //==========================================
        [Title("Durability")]

		[Tooltip("Defines how hard Player will push opponents")]
		public float Durability = 50f;
		[Space(20)]

        //==========================================
        [Header("SECONDARY SETTINGS")]

		[Title("Engine")]

		[Range(0, 1)]
		[Tooltip("Probability backfire: 0 - off backfire, 1 always on backfire")]
		public float ProbabilityBackfire = 0.1f; //Probability backfire: 0 - off backfire, 1 always on backfire. Default = 0.2f

		[Title("Drift and Helper")]

		[Tooltip("Maximum rear wheel slip for shifting gearbox")]
		public float MaxForwardSlipToBlockChangeGear = 0.25f; //Maximum rear wheel slip for shifting gearbox. Default = 0.25

		[Tooltip("Lerp Speed change of RPM")]
		public float RpmEngineToRpmWheelsLerpSpeed = 15; //Lerp Speed change of RPM. Default = 15

		public bool EnableSteerAngleMultiplier = true;

		[Tooltip("Min steer angle multiplayer to limit understeer at high speeds")]
		public float MinSteerAngleMultiplier = 0.05f; //Min steer angle multiplayer to limit understeer at high speeds. 

		[Tooltip("Max steer angle multiplayer to limit understeer at high speeds")]
		public float MaxSteerAngleMultiplier = 1f; //Max steer angle multiplayer to limit understeer at high speeds.

		[Tooltip("The maximum speed at which there will be a minimum steering angle multiplier")]
		public float MaxSpeedForMinAngleMultiplier = 250; //The maximum speed at which there will be a minimum steering angle multiplier.
		[Space]

		[Tooltip("Min speed at which helpers are enabled")]
		public float MinSpeedForSteerHelp = 1f; //Min speed at which helpers are enabled. Default = 20

        [Range(1f, 90f)]
        [Tooltip("The angle at which the assistant works 100%")]
        public float MaxAngularVelocityHelpAngle = 45f; //The angle at which the assistant works 100%.

        [Tooltip("The power of the helper to turn the rigidbody in the direction of the control turn")]
		public float OppositeAngularVelocityHelpPower = 0.05f; //The power of the helper to turn the rigidbody in the direction of the control turn.

		[Tooltip("The power of the helper to positive turn the rigidbody in the direction of the control turn")]
		public float PositiveAngularVelocityHelpPower = 0f; //The power of the helper to positive turn the rigidbody in the direction of the control turn.

		[Tooltip("Min angular velocity, reached at max drift angles.")]
		public float AngularVelocityInMaxAngle = 0.5f; //Min angular velocity, reached at max drift angles.

		[Tooltip("Max angular velocity, reached at min drift angles")]
		public float AngularVelocityInMinAngle = 2f; //Max angular velocity, reached at min drift angles.
		[Space(20)]

		[Header("Read only settings and automatically calculated properties")]
		[ReadOnly] public bool AutomaticGearBox = true;
		[ReadOnly] public AnimationCurve MotorTorqueFromRpmCurve = new AnimationCurve(_kf); //Curve motor torque (Y(0-1) motor torque, X(0-7) motor RPM).
		[ReadOnly] public float[] GearsRatio = { 3.38f, 2.36f, 1.67f, 1.312f, 1f }; //Forward gears ratio.
		[ReadOnly] public float MainRatio = 3.56f;
		[ReadOnly] public float ReversGearRatio = 4;
		[ReadOnly] public float MaxRPM = 7000; //Default = 7000. With this value, the maximum speed is guaranteed to be reached.

		[ShowInInspector, ReadOnly]
		public float MinRPM => MaxRPM * MinMaxRPMFactor; //Default = 700

		[ShowInInspector, ReadOnly]
		public float RpmToNextGear => MaxRPM * RPMToNextGearPercent; //The speed at which there is an increase in gearbox. Default = 6500

		[ShowInInspector, ReadOnly]
		public float RpmToPrevGear => RpmToNextGear * PrevToNextGearFactor; //The speed at which there is an decrease in gearbox. Default = 4500

		[ShowInInspector, ReadOnly]
		public float CutOffRPM => MaxRPM * CutOffRPMFactor; //The RPM at which the cutoff is triggered.

		[ShowInInspector, ReadOnly]
		public float CutOffOffsetRPM => MinRPM * CutOffOffsetRPMFactor;

		[ShowInInspector, ReadOnly]
		public float CutOffTime = 0.1f;

		[ShowInInspector, ReadOnly]
		public float MaxSpeedVelocityMagnitude
		{
			get
			{
				float value = 0f;
				switch (SpeedType)
				{
					case SpeedType.MPH:
						value = MaxSpeed / C.MPHFactor;
						break;
					case SpeedType.KPH:
						value = MaxSpeed / C.KPHFactor;
						break;
				}

				return value;
			}
		}

		[ShowInInspector, ReadOnly]
		public float CruiseSpeedVelocityMagnitude
		{
			get
			{
				float value = 0f;
				switch (SpeedType)
				{
					case SpeedType.MPH:
						value = CruiseSpeed / C.MPHFactor;
						break;
					case SpeedType.KPH:
						value = CruiseSpeed / C.KPHFactor;
						break;
				}

				return value;
			}
		}

		public float MaxRPMToNextGearPercent => CutOffRPMFactor - RPMTONextGearVsCutOffDif;

		[JsonIgnore]
		private readonly static Keyframe[] _kf = new Keyframe[]
		{
			new Keyframe(0f, 0.3f),
			new Keyframe(3.54f, 0.869f, 0.1f, 0.1f),
			new Keyframe(6.4f, 1f),
			new Keyframe(8f, 0.2f)
		};

		private const float MinMaxRPMFactor = 0.1f;
		private const float CutOffRPMFactor = 0.995f;
		private const float CutOffOffsetRPMFactor = 0.71f;
		private const float PrevToNextGearFactor = 0.69f;
		private const float RPMTONextGearVsCutOffDif = 0.025f;

		[Button]
		private void ResetToDefaultValues()
		{
			SpeedType = SpeedType.KPH;
			DriveType = DriveType.AWD;

			MaxSpeed = 200;
			CruiseSpeed = 20f;
			//CruiseSpeedPercentMax = 0.8f;
			//CruiseSpeedPercentMin = 0.7f;
			RPMToNextGearPercent = 0.9f;
			MaxSteerAngle = 42;
			HelpSteerPower = 0.2f;  //0.2
			MaxAngularVelocityHelpAngle = 45f; //45
			SteerAngleChangeSpeed = 60f; //Wheel turn speed. Default = 60
			Durability = 50f;
			MaxMotorTorque = 800; //Max motor torque engine (Without GearBox multiplier).
			ProbabilityBackfire = 0.1f; //Probability backfire: 0 - off backfire, 1 always on backfire. Default = 0.1f
			MaxBrakeTorque = 2000;
			MaxForwardSlipToBlockChangeGear = 0.25f; //Maximum rear wheel slip for shifting gearbox. Default = 0.25
			RpmEngineToRpmWheelsLerpSpeed = 15; //Lerp Speed change of RPM. Default = 15
			EnableSteerAngleMultiplier = true;
			MinSteerAngleMultiplier = 0.05f; //Min steer angle multiplayer to limit understeer at high speeds. 
			MaxSteerAngleMultiplier = 1f; //Max steer angle multiplayer to limit understeer at high speeds.
			MaxSpeedForMinAngleMultiplier = 250; //The maximum speed at which there will be a minimum steering angle multiplier.
			MinSpeedForSteerHelp = 1f; //Min speed at which helpers are enabled. Default = 1
			OppositeAngularVelocityHelpPower = 0.05f; //The power of the helper to turn the rigidbody in the direction of the control turn.
			PositiveAngularVelocityHelpPower = 0f; //The power of the helper to positive turn the rigidbody in the direction of the control turn.
			AngularVelocityInMaxAngle = 0.5f; //Min angular velocity, reached at max drift angles.
			AngularVelocityInMinAngle = 2f; //Max angular velocity, reached at min drift angles.
		}
	}
}
