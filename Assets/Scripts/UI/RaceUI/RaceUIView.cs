using UnityEngine;

namespace RaceManager.UI
{
    public class RaceUIView : MonoBehaviour
    {
        [SerializeField] private PositionIndicatorView _positionIndicatorView;
        [SerializeField] private SpeedIndicatorView _speedIndicatorView;
        [SerializeField] private DriftScoresIndicatorView _driftScoresIndicatorView;
        [SerializeField] private RaceProgressBarView _raceProgressBarView;
        [SerializeField] private RespawnCarButtonView _respawnCarButton;
        [SerializeField] private RespawnCarButtonView _getToCheckpointButton;

        public PositionIndicatorView PositionIndicator => _positionIndicatorView;
        public SpeedIndicatorView SpeedIndicator => _speedIndicatorView;
        public DriftScoresIndicatorView DriftScoresIndicator => _driftScoresIndicatorView;
        public RaceProgressBarView RaceProgressBar => _raceProgressBarView;
        public RespawnCarButtonView RespawnCarButton => _respawnCarButton;
        public RespawnCarButtonView GetToCheckpointButton => _getToCheckpointButton;
    }
}

