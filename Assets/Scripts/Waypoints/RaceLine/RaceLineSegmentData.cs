
using UnityEngine;

namespace RaceManager.Waypoints
{
    [System.Serializable]
    public struct RaceLineSegmentData
    {
        public float recomendedSpeed;

        public float colorTransitionDuration;
        public float alphaTransitionDuration;

        public Color baseColor;
        public Color warningColor;

        public float checkOffset;
    }
}
