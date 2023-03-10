using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class AnimatableSubject : MonoBehaviour, IAnimatableSubject
    {
        public bool Animate;

        [Title("Animation Settings")]
        [ShowIf("Animate")]
        [SerializeField] private List<AnimationData> _animationsData;

        public string Name => gameObject.name;
        public List<AnimationData> AnimationDataList => _animationsData;
    }
}

