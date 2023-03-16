using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class AnimatableImage : AnimatableSubject
    {
        [Space(20)]
        [SerializeField] private Image _image;

        private RectTransform _rect;

        public Image Image => _image;

        public RectTransform Rect
        {
            get
            { 
                if(_rect == null)
                    _rect = GetComponent<RectTransform>();
                return _rect;
            }
        }
    }
}

