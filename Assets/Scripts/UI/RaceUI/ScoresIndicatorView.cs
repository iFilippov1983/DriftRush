using UnityEngine;
using TMPro;

namespace RaceManager.UI
{
    public class ScoresIndicatorView : MonoBehaviour
    {
        [SerializeField] private RectTransform _scoresRect;
        [SerializeField] private TMP_Text _scoresText;
        [SerializeField] private TMP_Text _pauseTimerText;
        [Space()]
        [SerializeField] private RectTransform _extraScoresRect;
        [SerializeField] private RectTransform _driftScoresRect;
        [SerializeField] private RectTransform _driftScoresMoveToRect;
        
        public RectTransform ScoresRect => _scoresRect;
        public TMP_Text ScoresText => _scoresText;
        public TMP_Text PauseTimerText => _pauseTimerText;
        public RectTransform ExtraScoresRect => _extraScoresRect;
        public RectTransform DriftScoresRect => _driftScoresRect;
        public RectTransform DriftScoresMoveToRect => _driftScoresMoveToRect;
    }
}

