using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace RaceManager.Cars
{
    [Serializable]
    public struct Speed
    {
        public float Max;
        public float Min;
    }

    [Serializable]
    public struct Handling
    {
        //[FoldoutGroup("Front wheels")]
        //[Range(0f, 1f)]
        //public float f_frictionForward_Max;
        //[FoldoutGroup("Front wheels")]
        //[Range(0f, 1f)]
        //public float f_frictionForward_Min;
        //[Space]
        //[FoldoutGroup("Front wheels")]
        //[Range(0f, 1f)]
        //public float f_frictionSideway_Max;
        //[FoldoutGroup("Front wheels")]
        //[Range(0f, 1f)]
        //public float f_frictionSideway_Min;

        //[FoldoutGroup("Rear wheels")]
        //[Range(0f, 1f)]
        //public float r_frictionForward_Max;
        //[FoldoutGroup("Rear wheels")]
        //[Range(0f, 1f)]
        //public float r_frictionForward_Min;
        //[Space]
        //[FoldoutGroup("Rear wheels")]
        //[Range(0f, 1f)]
        //public float r_frictionSideway_Max;
        //[FoldoutGroup("Rear wheels")] 
        //[Range(0f, 1f)] 
        //public float r_frictionSideway_Min;
        //[Space]

        [Range(1f, 90f)]
        public float steerAngle_Max;
        [Range(1f, 90f)]
        public float steerAngle_Min;
        [Space]

        [Range(0.001f, 1f)]
        public float helpSteerPower_Max;
        [Range(0.001f, 1f)]
        public float helpSteerPower_Min;
        [Space] 
        
        public float steerAngleChangeSpeed_Max;
        public float steerAngleChangeSpeed_Min;
    }

    [Serializable]
    public struct Acceleration
    {
        public float torqueMax;
        public float torqueMin;
        [Space]
        public float brakeTorqueMax;
        public float brakeTorqueMin;
        [Space]
        [Range(0.05f, 0.97f)]
        public float rpmToNextGearMax;
        [Range(0.05f, 0.97f)]
        public float rpmToNextGearMin;
    }

    [Serializable]
    public struct Friction
    {
        [FoldoutGroup("Front wheels")]
        [Range(0f, 1f)]
        public float f_frictionForward_Max;
        [FoldoutGroup("Front wheels")]
        [Range(0f, 1f)]
        public float f_frictionForward_Min;
        [Space]
        [FoldoutGroup("Front wheels")]
        [Range(0f, 1f)]
        public float f_frictionSideway_Max;
        [FoldoutGroup("Front wheels")]
        [Range(0f, 1f)]
        public float f_frictionSideway_Min;

        [FoldoutGroup("Rear wheels")]
        [Range(0f, 1f)]
        public float r_frictionForward_Max;
        [FoldoutGroup("Rear wheels")]
        [Range(0f, 1f)]
        public float r_frictionForward_Min;
        [Space]
        [FoldoutGroup("Rear wheels")]
        [Range(0f, 1f)]
        public float r_frictionSideway_Max;
        [FoldoutGroup("Rear wheels")]
        [Range(0f, 1f)]
        public float r_frictionSideway_Min;
    }

    [Serializable]
    public struct Durability
    {
        public float durabilityMax;
        public float durabilityMin;
    }
}
