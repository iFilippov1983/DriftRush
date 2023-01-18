using UnityEngine;

namespace RaceManager.Waypoints
{
    public struct RaceLineSegmentData
    {
        public float recomendedSpeed;
        public float fadeSpeed;
        public float colorTransitionSpeed;

        public Color baseColor;
        public Color warningColor;

        public float checkOffset;
    }
}
