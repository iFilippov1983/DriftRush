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

        public RectTransform ShowRect
        {
            get 
            { 
                if(_showRect is null)
                    _showRect = GetComponent<RectTransform>();
                return _showRect;
            }
        }

        public RectTransform HideRect => _hideRect;
    }
}
