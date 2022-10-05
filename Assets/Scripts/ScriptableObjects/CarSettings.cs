using Sirenix.OdinInspector;
using UnityEngine;

namespace RaceManager.Cars
{
    [CreateAssetMenu(menuName = "Settings/CarSettings", fileName = "CarSettings_Name")]
    public class CarSettings : ScriptableObject
    {
        [Title("Speed")]
        public float MaxSpeed = 200f;

        [Title("Acceleration")]
        public float FullTorqueOverAllWheels = 2000;

        [Title("Mobility")]
        [Range(0, 1)]
        public float SteerHelper = 0.7f; // 0 is raw physics , 1 the car will grip in the direction it is facing
        public float MaximumSteerAngle = 35f;
        public float DriftFactor = 0.99f; // 0 cannot drift at all 1 can drift anytime physics allowed

        [Title("Durability")]
        public float Durability = 100f;
    }
}
