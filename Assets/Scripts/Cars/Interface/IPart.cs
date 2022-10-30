using System;
using UnityEngine;

namespace RaceManager.Cars
{
    public interface IPart
    {
        PartType Type { get; }
        bool IsActive { get; set; }
    }

    [Serializable]
    public struct WheelProperty
    {
        public float WheelRadius;
        public Vector3 WheelScale;
    }

    [Serializable]
    public struct SuspentionProperty
    {
        public float SuspentionHeight;
    }
}
