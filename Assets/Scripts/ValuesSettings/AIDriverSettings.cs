using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Vehicles
{
    [CreateAssetMenu(menuName = "ValuesSettings/AIDriverSettings", fileName = "AIDriverSettings_Name")]
    public class AIDriverSettings : ScriptableObject
    {
        // "wandering" is used to give the cars a more human, less robotic feel. They can waver slightly
        // in speed and direction while driving towards their target.
        [Range(0, 1)] public float CautiousSpeedFactor = 0.05f;               // percentage of max speed to use when being maximally cautious
        [Range(0, 180)] public float CautiousMaxAngle = 180f;                 // angle of approaching corner to treat as warranting maximum caution
        public float CautiousMaxDistance = 100f;                              // distance at which distance-based cautiousness begins
        public float CautiousAngularVelocityFactor = 45f;                     // how cautious the AI should be when considering its own current angular velocity (i.e. easing off acceleration if spinning!)
        public float SteerSensitivity = 0.01f;                                // how sensitively the AI uses steering input to turn to the desired direction
        public float AccelSensitivity = 1f;                                   // How sensitively the AI uses the accelerator to reach the current desired speed
        public float BrakeSensitivity = 1f;                                   // How sensitively the AI uses the brake to reach the current desired speed
        public float LateralWanderDistance = 5f;                              // how far the car will wander laterally towards its target
        public float LateralWanderSpeed = 0.2f;                               // how fast the lateral wandering will fluctuate
        [Range(0, 1)] public float AccelWanderAmount = 0.1f;                  // how much the cars acceleration will wander
        public float AccelWanderSpeed = 0.1f;                                 // how fast the cars acceleration wandering will fluctuate
        public BrakeCondition BrakeCondition = BrakeCondition.TargetDistance; // what should the AI consider when accelerating/braking?
        public float ReachTargetThreshold = 2;                                // proximity to target to consider we 'reached' it, and stop driving.

        //defines range of AI cruise speed for random selection
        [Range(0f, 1f)] public float CruiseSpeedThresholdMax = 0.8f;
        [Range(0f, 1f)] public float CruiseSpeedThresholdMin = 0.7f;
    }
}
