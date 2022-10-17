using RaceManager.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RaceManager.Cars
{
	[System.Serializable]
	public class CarConfig
	{
		public CarProfile CarProfile;

		[Header("Main settings")]

		[Title("Drive settings")]
		public SpeedType SpeedType = SpeedType.KPH;
		public DriveType DriveType = DriveType.AWD;
		[Space]

		[Title("Speed")]
		public float MaxSpeed = 200;

		[Space]
		[Tooltip("Cruise Speed Settings (for Player)")]
		public float CruiseSpeed = 20f;
		
		[Tooltip("Represents percentage range of Max Speed AI will use")]
		[Range(0.01f, 1f)]
		public float CruiseSpeedPercentMin = 0.7f;
		[Tooltip("Represents percentage range of Max Speed AI will use")]
		[Range(0.01f, 1f)]
		public float CruiseSpeedPercentMax = 0.8f;

		[Title("Acceleration")]
		public float MaxRPM = 7000;

		[Title("Mobility")]
		[Range(0.001f, 1f)]
		[Tooltip("The power of turning the wheels in the direction of the drift")]
		public float HelpSteerPower = 0.2f; //The power of turning the wheels in the direction of the drift.
		public float MaxSteerAngle = 42;

		[Title("Durability")]
		[Tooltip("Defines how hard Player will push opponents")]
		public float Durability = 50f;
		[Space(20)]

		[Header("Secondary settings")]
		[Title("Engine")]
		public float MaxMotorTorque = 800; //Max motor torque engine (Without GearBox multiplier).

		[Range(0, 1)]
		[Tooltip("Probability backfire: 0 - off backfire, 1 always on backfire")]
		public float ProbabilityBackfire = 0.1f; //Probability backfire: 0 - off backfire, 1 always on backfire.

		[Title("Braking")]
		public float MaxBrakeTorque = 2000;
		[Space(20)]

		[Title("Drift")]
		public float MaxForwardSlipToBlockChangeGear = 0.25f; //Maximum rear wheel slip for shifting gearbox.
		public float RpmEngineToRpmWheelsLerpSpeed = 15; //Lerp Speed change of RPM.

		[Header("Read only settings and automatically calculated properties")]
		[ReadOnly] public bool AutomaticGearBox = true;
		[ReadOnly] public AnimationCurve MotorTorqueFromRpmCurve = new AnimationCurve(_kf); //Curve motor torque (Y(0-1) motor torque, X(0-7) motor RPM).
		[ReadOnly] public float[] GearsRatio = {3.38f, 2.36f, 1.67f, 1.312f, 1f }; //Forward gears ratio.
		[ReadOnly] public float MainRatio = 3.56f;
		[ReadOnly] public float ReversGearRatio = 4;//Reverse gear ratio.

		public float RpmToNextGear = 6500;                      //The speed at which there is an increase in gearbox.
		public float RpmToPrevGear = 4500;                      //The speed at which there is an decrease in gearbox.

		
		

		

		[Header("Helper settings")]                             //This settings block in the full version is stored in the regime settings.
		public bool EnableSteerAngleMultiplier = true;
		public float MinSteerAngleMultiplier = 0.05f;           //Min steer angle multiplayer to limit understeer at high speeds.
		public float MaxSteerAngleMultiplier = 1f;          //Max steer angle multiplayer to limit understeer at high speeds.
		public float MaxSpeedForMinAngleMultiplier = 250;       //The maximum speed at which there will be a minimum steering angle multiplier.
		[Space]
		public float SteerAngleChangeSpeed = 100f;                     //Wheel turn speed.
		public float MinSpeedForSteerHelp = 1f;                      //Min speed at which helpers are enabled.
		public float OppositeAngularVelocityHelpPower = 0.05f;   //The power of the helper to turn the rigidbody in the direction of the control turn.
		public float PositiveAngularVelocityHelpPower = 0f;   //The power of the helper to positive turn the rigidbody in the direction of the control turn.
		public float MaxAngularVelocityHelpAngle = 45f;               //The angle at which the assistant works 100%.
		public float AngularVelocityInMaxAngle = 0.5f;                 //Min angular velocity, reached at max drift angles.
		public float AngularVelocityInMinAngle = 2f;                 //Max angular velocity, reached at min drift angles.



		[ShowInInspector, ReadOnly]
		public float MinRPM => MaxRPM * MinMaxRPMFactor;

		[ShowInInspector, ReadOnly]
		public float CutOffRPM => MaxRPM * 0.99f;//The RPM at which the cutoff is triggered.

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

		private readonly static Keyframe[] _kf = new Keyframe[] 
		{ 
			new Keyframe(0f, 0.3f), 
			new Keyframe(3.54f, 0.869f, 0.1f, 0.1f), 
			new Keyframe(6.4f, 1f), 
			new Keyframe(8f, 0.2f) 
		};

		private const float MinMaxRPMFactor = 0.1f;
		private const float CutOffOffsetRPMFactor = 0.71f;
		private const float PrevToNextGearFactor = 0.69f;
	}
}
