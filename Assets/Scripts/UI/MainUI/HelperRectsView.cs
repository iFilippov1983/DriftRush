using UnityEngine;

namespace RaceManager.UI
{
    public class HelperRectsView : MonoBehaviour
    {
        [Tooltip("Appear Rects")]
        [SerializeField] private RectTransform _appearTopRect;
        [SerializeField] private RectTransform _appearBottomRect;
        [SerializeField] private RectTransform _appearLeftRect;
        [SerializeField] private RectTransform _appearRightRect;

        public RectTransform AppearTopRect => _appearTopRect;
        public RectTransform AppearBottomRect => _appearBottomRect;
        public RectTransform AppearLeftRect => _appearLeftRect;
        public RectTransform ApearRightRect => _appearRightRect;
    }
}

