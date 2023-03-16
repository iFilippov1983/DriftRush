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
        [SerializeField] private AnimationSettings _settings;
        [ShowIf("Animate")]
        [SerializeField] private List<AnimationData> _animationsData;

        public string Name => gameObject.name;
        public AnimationSettings Settings => _settings;
        public List<AnimationData> AnimationDataList => _animationsData;
    }
}

