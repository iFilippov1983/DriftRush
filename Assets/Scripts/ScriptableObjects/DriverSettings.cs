using RaceManager.Waypoints;
using Sirenix.OdinInspector;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Cars
{
    [CreateAssetMenu(menuName = "Settings/DriverSettings", fileName = "DriverSettings_Name")]
    public class DriverSettings : ScriptableObject
    {
        public string CurrentCarName;

        [Title("Defines the range of AI cruise speed for random selection")]
        [Range(0f, 1f)] public float CruiseSpeedPercentMax = 0.8f;
        [Range(0f, 1f)] public float CruiseSpeedPercentMin = 0.7f;
        
        [Title("For Player")]
        public float CruiseSpeed = 20f;
    }
}
