using System;
using UnityEngine;

namespace RaceManager.Cars
{
    public interface IPart
    {
        PartType Type { get; }
        GameObject Object { get; }
        PartProperty Property { get; }
    }

    [Serializable]
    public struct PartProperty
    {
        public string Name;
        public float Value;
        public Vector3 Scale;
    }
}
