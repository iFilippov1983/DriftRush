using Sirenix.OdinInspector;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Vehicles
{
    [CreateAssetMenu(menuName = "ValuesSettings/VehicleSettings", fileName = "VehicleSettings_Name")]
    public class VehicleSettings : ScriptableObject
    {
        [Title("Move options")]
        public CarDriveType CarDriveType = CarDriveType.FourWheelDrive;
        public SpeedType SpeedType = SpeedType.KPH;
        public float SpeedTop = 200f;
        public float SpeedCruise = 2f;
        [Title("Engine properties")]
        public float FullTorqueOverAllWheels = 2000;
        public float ReverseTorque = 200;
        public float MaxHandbrakeTorque = float.MaxValue;
        public static int NoOfGears = 5;
    }
}
