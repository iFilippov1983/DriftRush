using Sirenix.OdinInspector;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Vehicles
{
    [CreateAssetMenu(menuName = "ValuesSettings/VehicleSettings", fileName = "VehicleSettings_Name")]
    public class VehicleSettings : ScriptableObject
    {
        [Title("Speed")]
        public float MaxSpeed = 200f;

        [Title("Acceleration")]
        [Range(0.1f, 1f), InfoBox("More factor means less acceleration")]
        public float AccelerationFactor = 1f;
        public float FullTorqueOverAllWheels = 2000;

        [Title("Mobility")]
        [Range(0, 1)]
        public float SteerHelper = 0.7f; // 0 is raw physics , 1 the car will grip in the direction it is facing
        public float MaximumSteerAngle = 35f;

        [Title("Durability")]
        public float Durability = 100f;
    }
}
