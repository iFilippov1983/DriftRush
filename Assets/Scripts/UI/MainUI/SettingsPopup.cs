using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class SettingsPopup : MonoBehaviour
    {
        [SerializeField] private Button _okButton;
        [SerializeField] private Button _closePopupWindowButton;
        [SerializeField] private Slider _soundToggleSlider;
        [SerializeField] private Slider _musicToggleSlider;
        [SerializeField] private Slider _vibrationToggleSlider;
        [SerializeField] private Slider _raceLineToggleSlider;

        public Button OkButton => _okButton;
        public Button ClosePopupWindowButton => _closePopupWindowButton;
        public Slider SoundToggleSlider => _soundToggleSlider;
        public Slider MusicToggleSlider => _musicToggleSlider;
        public Slider VibrationToggleSlider => _vibrationToggleSlider;
        public Slider RaceLineToggleSlider => _raceLineToggleSlider;
    }
}

