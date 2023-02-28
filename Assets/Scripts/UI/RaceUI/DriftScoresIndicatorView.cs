using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;

namespace RaceManager.UI
{
    public class DriftScoresIndicatorView : MonoBehaviour
    {
        [Title("Current scores")]
        [SerializeField] private RectTransform _currentScoresRect;
        [SerializeField] private TMP_Text _currentScoresText;
        [SerializeField] private TMP_Text _pauseTimerText;
        [Space()]
        [Title("Total scores")]
        [SerializeField] private RectTransform _totalScoresRect;
        [SerializeField] private TMP_Text _totalScoresText;

        public RectTransform CurrentScoresRect => _currentScoresRect;
        public TMP_Text CurrentScoresText => _currentScoresText;
        public TMP_Text PauseTimerText => _pauseTimerText;

        public RectTransform TotalScoresRect => _totalScoresRect;
        public TMP_Text TotalScoresText => _totalScoresText;
    }
}

