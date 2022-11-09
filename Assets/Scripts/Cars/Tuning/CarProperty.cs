using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace RaceManager.Cars
{
    [Serializable]
    public struct Speed
    {
        public float Min;
        public float Max;
    }

    [Serializable]
    public struct Mobility
    {
        [Title("Weels friction")]
        [FoldoutGroup("Front")] public float f_frictionForward;
        [FoldoutGroup("Front")] public float f_frictionSideway;

        [FoldoutGroup("Rear")] public float r_frictionForward;
        [FoldoutGroup("Rear")] public float r_frictionSideway;

        [Title("Steering")]
        public float steerAngle_Max;
        public float steerAngle_Min;
        [Space]
        public float helpSteerPower_Min;
        public float helpSteerPower_Max;
        [Space]
        public float steerAngleSpeedChange_Min;
        public float steerAngleSpeedChange_Max;
    }

    [Serializable]
    public struct Acceleration
    { 
        
    }
}
