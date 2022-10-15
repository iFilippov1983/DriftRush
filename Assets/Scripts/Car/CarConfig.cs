﻿using Sirenix.OdinInspector;
using UnityEngine;

namespace RaceManager.Cars
{
    /// <summary>
    /// For easy initialization and change of parameters in the future. TODO Add tuning.
    /// </summary>
    [System.Serializable]
	public class CarConfig
	{
		private const float KPHFactor = 3.6f;
		private const float MPHFactor = 2.23693629f;

		public CarProfile CarProfile;

        [Space(20)]
        //-----------------------------------------------------------------------------
        [Header("Main settings")]

        [Title("Speed")]
        public float MaxSpeed = 200;
        [Tooltip("Cruise Speed for Player")]
        public float CruiseSpeed = 20f;

        [Header("Mobility")]
        public float MaxSteerAngle = 42;

        [Title("Durability")]
        [Tooltip("Defines how hard Player will push opponents")]
        public float Durability = 50f;

        

        [Space(20)]
        //-----------------------------------------------------------------------------
        [Title("Drive settings")]
        public SpeedType SpeedType = SpeedType.KPH;
        public DriveType DriveType = DriveType.RWD;             //Drive type AWD, FWD, RWD. With the current parameters of the car only RWD works well.

        [Tooltip("Represents percentage range of Max Speed AI will use")]
        [Range(0.01f, 1f)]
        public float CruiseSpeedPercentMin = 0.7f;

        [Tooltip("Represents percentage range of Max Speed AI will use")]
        [Range(0.01f, 1f)]
        public float CruiseSpeedPercentMax = 0.8f;

        [Space(20)]
        //-----------------------------------------------------------------------------
        [Header("Secondary settings")]

        [Title("Engine and power settings")]
        [ReadOnly]
		public bool AutomaticGearBox = true;
		public float MaxMotorTorque = 800;                      //Max motor torque engine (Without GearBox multiplier).
		public AnimationCurve MotorTorqueFromRpmCurve;          //Curve motor torque (Y(0-1) motor torque, X(0-7) motor RPM).
		public float MaxRPM = 7000;
		public float MinRPM = 700;
		public float CutOffRPM = 6800;                          //The RPM at which the cutoff is triggered.
		public float CutOffOffsetRPM = 500;
		public float CutOffTime = 0.1f;
		[Range(0, 1)] public float ProbabilityBackfire = 0.1f;   //Probability backfire: 0 - off backfire, 1 always on backfire.
		public float RpmToNextGear = 6500;                      //The speed at which there is an increase in gearbox.
		public float RpmToPrevGear = 4500;                      //The speed at which there is an decrease in gearbox.
		public float MaxForwardSlipToBlockChangeGear = 0.25f;    //Maximum rear wheel slip for shifting gearbox.
		public float RpmEngineToRpmWheelsLerpSpeed = 15;        //Lerp Speed change of RPM.
		public float[] GearsRatio =
			{3.38f, 2.36f, 1.67f, 1f };                         //Forward gears ratio.
		public float MainRatio = 3.56f;
		public float ReversGearRatio = 4;                           //Reverse gear ratio.

		[Header("Braking settings")]
		public float MaxBrakeTorque = 2000;

		[Header("Helper settings")]                             //This settings block in the full version is stored in the regime settings.

		public bool EnableSteerAngleMultiplier = true;
		public float MinSteerAngleMultiplier = 0.1f;           //Min steer angle multiplayer to limit understeer at high speeds.
		public float MaxSteerAngleMultiplier = 1f;          //Max steer angle multiplayer to limit understeer at high speeds.
		public float MaxSpeedForMinAngleMultiplier = 250;       //The maximum speed at which there will be a minimum steering angle multiplier.
		public float SteerAngleChangeSpeed = 60f;                     //Wheel turn speed.
		public float MinSpeedForSteerHelp = 2f;                      //Min speed at which helpers are enabled.
		[Range(0f, 1f)] public float HelpSteerPower = 0.2f;            //The power of turning the wheels in the direction of the drift.
		public float OppositeAngularVelocityHelpPower = 0.05f;   //The power of the helper to turn the rigidbody in the direction of the control turn.
		public float PositiveAngularVelocityHelpPower = 0f;   //The power of the helper to positive turn the rigidbody in the direction of the control turn.
		public float MaxAngularVelocityHelpAngle = 45f;               //The angle at which the assistant works 100%.
		public float AngularVelocityInMaxAngle = 0.5f;                 //Min angular velucity, reached at max drift angles.
		public float AngularVelocityInMinAngle = 2f;                 //Max angular velucity, reached at min drift angles.


        //[Space(20)]
        ////-----------------------------------------------------------------------------
        //[Header("Automatically calculated settings")]
        [ReadOnly, ShowInInspector]
        public float MaxRBVelocityMagnitude => CalculateVelocity(MaxSpeed);
        [ReadOnly, ShowInInspector]
        public float CruiseRBVelocityMagnitude => CalculateVelocity(CruiseSpeed);

        public float CalculateVelocity(float vel)
        {
            switch (SpeedType)
            {
                case SpeedType.MPH:
                    vel /=  MPHFactor;
                    break;
                case SpeedType.KPH:
                    vel /=  KPHFactor;
                    break;
            }
            return vel;
        }
    }
}
