using UnityEngine;
using TMPro;

namespace RaceManager.UI
{
    public class DriftScoresIndicatorView : MonoBehaviour
    {
        [SerializeField] private RectTransform _itsRect;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _signText;
        [SerializeField] private TMP_Text _multiplierText;
        [SerializeField] private TMP_Text _totalText;

        public Transform Transform { get; private set; }
        public bool isFinalizing { get; set; }

        public RectTransform Rect => _itsRect;
        public TMP_Text TitleText => _titleText;
        public TMP_Text ScoreText => _scoreText;    
        public TMP_Text SignText => _signText;
        public TMP_Text MultiplierText => _multiplierText;
        public TMP_Text TotalText => _totalText;

        private void Awake()
        {
            Transform = transform;
        }
    }
}

