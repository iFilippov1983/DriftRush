using Sirenix.OdinInspector;
using UnityEngine;

namespace RaceManager.Cars
{
    [CreateAssetMenu(menuName = "Settings/CarSettings", fileName = "CarSettings_Name")]
    public class CarSettings : ScriptableObject
    {
        private const float KPHFactor = 3.6f;
        private const float MPHFactor = 2.23693629f;

        public CarProfile CarProfile;

        [Title("Max Speed Settings")]
        public SpeedType SpeedType = SpeedType.KPH;
        public float MaxSpeed = 200f;
        [ReadOnly]
        public float MaxRBVelocityMagnitude = 20f;

        [Title("Defines the range of AI cruise speed for random selection")]
        [Range(0f, 1f)] public float CruiseSpeedPercentMax = 0.8f;
        [Range(0f, 1f)] public float CruiseSpeedPercentMin = 0.7f;

        [Title("Cruise Speed Settings (for Player)")]
        public float CruiseSpeed = 20f;
        [ReadOnly]
        public float CruiseRBVelocityMagnitude = 2f;

        [Title("Acceleration")]
        public float FullTorqueOverAllWheels = 2000;

        [Title("Mobility")]
        [Range(0, 1)]
        public float SteerHelper = 0.5f;        // 0 is raw physics , 1 the car will grip in the direction it is facing
        public float SteerHelperRange = 3f;
        [Range(0, 1), Tooltip("0 means no grip, 1 full grip, >1 extra drift")]
        public float FrontWheelsGripFactor = 0f;  
        [Range(0, 1), Tooltip("0 means no grip, 1 full grip, >1 extra drift")]
        public float BackWheelsGripFactor = 0f;   
        [Range(0, 1)]
        public float DriftFactor = 0.99f;       // 0 cannot drift at all 1 can drift anytime physics allowed
        public float MaximumSteerAngle = 35f;

        [Title("Durability")]
        public float Durability = 100f;

        [OnInspectorGUI]
        public void RecalculateVelocities()
        {
            switch (SpeedType)
            {
                case SpeedType.MPH:
                    MaxRBVelocityMagnitude =  MaxSpeed / MPHFactor;
                    CruiseRBVelocityMagnitude = CruiseSpeed / MPHFactor;
                    break;
                case SpeedType.KPH:
                    MaxRBVelocityMagnitude = MaxSpeed / KPHFactor;
                    CruiseRBVelocityMagnitude = CruiseSpeed / KPHFactor;
                    break;
            }
        }
    }
}