using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.Root
{
    public class TuningPanel : MonoBehaviour
    {
        [SerializeField] private Button _tuneStatsButton;
        [SerializeField] private Button _tuneWheelsViewButton;
        [SerializeField] private Button _tuneCarViewButton;
        [SerializeField] private TMP_Text _carNameText;
        [SerializeField] private TMP_Text _carStatsProgressText;

        public Button TuneStatsButton => _tuneStatsButton;
        public Button TuneWheelsViewButton => _tuneWheelsViewButton;
        public Button TuneCarViewButton => _tuneCarViewButton;
        public TMP_Text CarNameText => _carNameText;
        public TMP_Text CarStatsProgressText => _carStatsProgressText;
    }
}

