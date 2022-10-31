using RaceManager.Cars;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class TuningPanel : MonoBehaviour
    {
        [SerializeField] private Button _tuneStatsButton;
        [SerializeField] private Button _tuneWheelsViewButton;
        [SerializeField] private Button _tuneCarViewButton;
        [Space]
        [SerializeField] private RectTransform _statsValuesPanel;
        [SerializeField] private RectTransform _tuneWheelsViewPanel;
        [SerializeField] private RectTransform _tuneCarViewPanel;
        [Space]
        [SerializeField] private Slider _speedSlider;
        [SerializeField] private Slider _mobilitySlider;
        [SerializeField] private Slider _durabilitySlider;
        [SerializeField] private Slider _accelerationSlider;
        [Space]
        [SerializeField] private TMP_Text _carNameText;
        [SerializeField] private TMP_Text _carStatsProgressText;

        public Button TuneStatsButton => _tuneStatsButton;
        public Button TuneWheelsViewButton => _tuneWheelsViewButton;
        public Button TuneCarViewButton => _tuneCarViewButton;

        public RectTransform StatsValuesPanel => _statsValuesPanel;
        public RectTransform TuneWheelsViewPanel => _tuneWheelsViewPanel;
        public RectTransform TuneCarViewPanel => _tuneCarViewPanel;

        public Slider SpeedSlider => _speedSlider;
        public Slider MobilitySlider => _mobilitySlider;
        public Slider DurabilitySlider => _durabilitySlider;
        public Slider AccelerationSlider => _accelerationSlider;

        public TMP_Text CarNameText => _carNameText;
        public TMP_Text CarStatsProgressText => _carStatsProgressText;

        public void RegisterButtonsListeners()
        {
            TuneStatsButton.onClick.AddListener(OpenStatsValuesPanel);
            TuneWheelsViewButton.onClick.AddListener(OpenTuneWheelsViewPanel);
            TuneCarViewButton.onClick.AddListener(OpenTuneCarViewPanel);
        }

        public void SetBorderValueToSlider(CarCharacteristics characteristics, int minValue, int maxValue)
        {
            switch (characteristics)
            {
                case CarCharacteristics.Speed:
                    SpeedSlider.minValue = minValue;
                    SpeedSlider.maxValue = maxValue;
                    break;
                case CarCharacteristics.Mobility:
                    MobilitySlider.minValue = minValue;
                    MobilitySlider.maxValue = maxValue;
                    break;
                case CarCharacteristics.Durability:
                    DurabilitySlider.minValue = minValue;
                    DurabilitySlider.maxValue = maxValue;
                    break;
                case CarCharacteristics.Acceleration:
                    AccelerationSlider.minValue = minValue;
                    AccelerationSlider.maxValue = maxValue;
                    break;
            }
        }

        public void SetValueToSlider(CarCharacteristics characteristics, int value)
        {
            switch (characteristics)
            {
                case CarCharacteristics.Speed:
                    SpeedSlider.value = value;
                    break;
                case CarCharacteristics.Mobility:
                    MobilitySlider.value = value;
                    break;
                case CarCharacteristics.Durability:
                    DurabilitySlider.value = value;
                    break;
                case CarCharacteristics.Acceleration:
                    AccelerationSlider.value = value;
                    break;
            }
        }

        public void UpdateAllSlidersValues(int speed, int mobility, int durability, int acceleration)
        { 
            SpeedSlider.value = speed;
            MobilitySlider.value = mobility;
            DurabilitySlider.value = durability;
            AccelerationSlider.value = acceleration;
        }

        public void UpdateCarStatsProgress(string carName, int currentValue, int maxValue)
        {
            CarNameText.text = carName;
            CarStatsProgressText.text = $"{currentValue}/{maxValue}";
        }

        public void OpenStatsValuesPanel()
        {
            StatsValuesPanel.SetActive(true);
            TuneWheelsViewPanel.SetActive(false);
            TuneCarViewPanel.SetActive(false);
        }

        public void OpenTuneWheelsViewPanel()
        {
            StatsValuesPanel.SetActive(false);
            TuneWheelsViewPanel.SetActive(true);
            TuneCarViewPanel.SetActive(false);
        }

        public void OpenTuneCarViewPanel()
        {
            StatsValuesPanel.SetActive(false);
            TuneWheelsViewPanel.SetActive(false);
            TuneCarViewPanel.SetActive(true);
        }
    }
}