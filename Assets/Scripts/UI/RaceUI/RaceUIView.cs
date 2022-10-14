using UnityEngine;

namespace RaceManager.UI
{
    public class RaceUIView : MonoBehaviour
    {
        [SerializeField] private PositionIndicatorView _positionIndicatorView;
        [SerializeField] private SpeedIndicatorView _speedIndicatorView;
        [SerializeField] private RaceProgressBarView _raceProgressBarView;
        [SerializeField] private RespawnCarButtonView _respawnCarButton;

        public PositionIndicatorView PositionIndicator => _positionIndicatorView;
        public SpeedIndicatorView SpeedIndicator => _speedIndicatorView;
        public RaceProgressBarView RaceProgressBar => _raceProgressBarView;
        public RespawnCarButtonView RespawnCarButton => _respawnCarButton;
    }
}

