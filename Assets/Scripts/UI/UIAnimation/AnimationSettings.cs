using System;
using UnityEngine;

namespace RaceManager.UI
{
    [Serializable]
    public struct AnimationSettings
    {
        public float appearAnimDuration;
        public float disappearAnimDuration;
        public float stateAnimDuration;
        public float animPauseDuration;
        [Space]
        public float maxScale;
        public float minScale;
    }
}

