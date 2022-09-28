using Sirenix.OdinInspector;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Vehicles
{
    [CreateAssetMenu(menuName = "ValuesSettings/VehicleSettings", fileName = "VehicleSettings_Name")]
    public class VehicleSettings : ScriptableObject
    {
        [Title("Move options")]
        public CarDriveType CarDriveType = CarDriveType.FrontWheelDrive;
        public SpeedType SpeedType = SpeedType.KPH;
        
        [Title("Engine properties")]
        public float SpeedTop = 200f;
        public float SpeedCruise = 20f;
        public float FullTorqueOverAllWheels = 2000;
        public float ReverseTorque = 200;
        [ReadOnly]
        public float MaxHandbrakeTorque = float.MaxValue;
        public static int NoOfGears = 5;
        public float RevRangeBoundary = 1f;

        [Title("Brakes properties")]
        public float Downforce = 100f;
        public float BrakeTorque = 20000f;

        [Title("Steering properties")]
        [Range(0, 1)]
        public float SteerHelper = 0.7f; // 0 is raw physics , 1 the car will grip in the direction it is facing
        public float MaximumSteerAngle = 35f;

        [Title("Wheels properties")]
        [Range(0, 1)] 
        public float TractionControl = 1f; // 0 is no traction control, 1 is full interference
        public float SlipLimit = 0.4f;

        [Title("Car body properties")]
        public float BodyStrength = 100f;
        public float Fragility = 10f;
    }
}
