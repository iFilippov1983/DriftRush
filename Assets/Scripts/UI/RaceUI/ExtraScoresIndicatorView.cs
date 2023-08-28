using UnityEngine;
using TMPro;

namespace RaceManager.UI
{
    public class ExtraScoresIndicatorView : MonoBehaviour
    {
        [SerializeField] private RectTransform _itsRect;
        [SerializeField] private TMP_Text _extraScoresTitle;
        [SerializeField] private TMP_Text _extraScoresText;

        public Transform Transform { get; private set; }
        public RectTransform Rect => _itsRect;
        public TMP_Text ExtraScoresTitle => _extraScoresTitle;
        public TMP_Text ExtraScoresText => _extraScoresText;

        private void Awake()
        {
            Transform = transform;
        }
    }
}

