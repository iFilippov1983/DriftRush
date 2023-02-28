using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace RaceManager.UI
{
    [RequireComponent(typeof(RectTransform))]
    [Serializable]
    public class AnimatablePanelView : SerializedMonoBehaviour, IAnimatableRectHolder
    {
        [SerializeField] private RectTransform _hideRect;
        private RectTransform _showRect;

        private void Awake()
        {
            _showRect = GetComponent<RectTransform>();
        }

        public RectTransform ShowRect => _showRect;
        public RectTransform HideRect => _hideRect;
    }
}
