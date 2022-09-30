using Sirenix.OdinInspector;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Vehicles
{
    [CreateAssetMenu(menuName = "ValuesSettings/AIDriverSettings", fileName = "AIDriverSettings_Name")]
    public class AIDriverSettings : ScriptableObject
    {
        [Title("Defines the range of AI cruise speed for random selection")]
        [Range(0f, 1f)] public float CruiseSpeedPercentMax = 0.8f;
        [Range(0f, 1f)] public float CruiseSpeedPercentMin = 0.7f;
        [Title("For Player")]
        public float CruiseSpeed = 20f;

        public enum DriverLevel
        { 
            Rookie,
            Regular,
            Experienced
        }
    }
}
