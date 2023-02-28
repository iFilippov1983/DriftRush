using UnityEngine;

namespace RaceManager.UI
{
    public class RaceUIView : MonoBehaviour
    {
        [SerializeField] private PositionIndicatorView _positionIndicatorView;
        [SerializeField] private SpeedIndicatorView _speedIndicatorView;
        [SerializeField] private ScoresIndicatorView _scoresIndicatorView;
        [SerializeField] private RaceProgressBarView _raceProgressBarView;
        [SerializeField] private RespawnCarButtonView _respawnCarButton;
        [SerializeField] private RespawnCarButtonView _getToCheckpointButton;

        public PositionIndicatorView PositionIndicator => _positionIndicatorView;
        public SpeedIndicatorView SpeedIndicator => _speedIndicatorView;
        public ScoresIndicatorView ScoresIndicator => _scoresIndicatorView;
        public RaceProgressBarView RaceProgressBar => _raceProgressBarView;
        public RespawnCarButtonView RespawnCarButton => _respawnCarButton;
        public RespawnCarButtonView GetToCheckpointButton => _getToCheckpointButton;
    }
}

