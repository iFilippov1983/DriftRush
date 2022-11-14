//using Grpc.Core;
using RaceManager.Cars;
using RaceManager.Tools;
using System;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UniRx.Operators;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class TuningPanel : MonoBehaviour
    {
        [SerializeField] private Button _tuneStatsButton;
        [SerializeField] private Button _tuneWheelsViewButton;
        [SerializeField] private Button _tuneCarViewButton;
        [SerializeField] private Button _closePanelButton;
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
        [SerializeField] private TMP_Text _factorPointsAvailableText;
        [Space]
        [SerializeField] private TMP_Text _speedPointsText;
        [SerializeField] private TMP_Text _speedPointsMaxText;
        [Space]
        [SerializeField] private TMP_Text _mobilityPointsText;
        [SerializeField] private TMP_Text _mobilityPointsMaxText;
        [Space]
        [SerializeField] private TMP_Text _durabilityPointsText;
        [SerializeField] private TMP_Text _durabilityPointsMaxText;
        [Space]
        [SerializeField] private TMP_Text _accelerationPointsText;
        [SerializeField] private TMP_Text _accelerationPointsMaxText;

        public Slider SpeedSlider => _speedSlider;
        public Slider MobilitySlider => _mobilitySlider;
        public Slider DurabilitySlider => _durabilitySlider;
        public Slider AccelerationSlider => _accelerationSlider;

        public Button CloseButton => _closePanelButton;

        private void OnEnable()
        {
            OpenStatsValuesPanel();
        }

        private void OnDisable()
        {
            DeactivateAllPanels();
        }

        public void RegisterButtonsListeners()
        {
            _tuneStatsButton.onClick.AddListener(OpenStatsValuesPanel);
            _tuneWheelsViewButton.onClick.AddListener(OpenTuneWheelsViewPanel);
            _tuneCarViewButton.onClick.AddListener(OpenTuneCarViewPanel);
            _closePanelButton.onClick.AddListener(DeactivateAllPanels);
        }

        public void SetBorderValues(CharacteristicType characteristics, int minValue, int maxValue)
        {
            //Debug.Log($"BORDER => {characteristics} - min {minValue} - max {maxValue}");
            switch (characteristics)
            {
                case CharacteristicType.Speed:
                    SpeedSlider.minValue = minValue;
                    SpeedSlider.maxValue = maxValue;
                    _speedPointsMaxText.text = maxValue.ToString();
                    break;
                case CharacteristicType.Mobility:
                    MobilitySlider.minValue = minValue;
                    MobilitySlider.maxValue = maxValue;
                    _mobilityPointsMaxText.text = maxValue.ToString();
                    break;
                case CharacteristicType.Durability:
                    DurabilitySlider.minValue = minValue;
                    DurabilitySlider.maxValue = maxValue;
                    _durabilityPointsMaxText.text = maxValue.ToString();
                    break;
                case CharacteristicType.Acceleration:
                    AccelerationSlider.minValue = minValue;
                    AccelerationSlider.maxValue = maxValue;
                    _accelerationPointsMaxText.text = maxValue.ToString();
                    break;
            }
        }

        public void SetValueToSlider(CharacteristicType characteristics, int sliderValue)
        {
            switch (characteristics)
            {
                case CharacteristicType.Speed:
                    SpeedSlider.value = sliderValue;
                    _speedPointsText.text = sliderValue.ToString();
                    break;
                case CharacteristicType.Mobility:
                    MobilitySlider.value = sliderValue;
                    _mobilityPointsText.text = sliderValue.ToString();
                    break;
                case CharacteristicType.Durability:
                    DurabilitySlider.value = sliderValue;
                    _durabilityPointsText.text = sliderValue.ToString();
                    break;
                case CharacteristicType.Acceleration:
                    AccelerationSlider.value = sliderValue;
                    _accelerationPointsText.text = sliderValue.ToString();
                    break;
            }
        }

        public void UpdateCurrentInfoValues(int available)
        {
            _factorPointsAvailableText.text = available.ToString();

            _speedPointsText.text = SpeedSlider.value.ToString();
            _mobilityPointsText.text = MobilitySlider.value.ToString();
            _durabilityPointsText.text = DurabilitySlider.value.ToString();
            _accelerationPointsText.text = AccelerationSlider.value.ToString();
        }

        public void UpdateAllSlidersValues(int speed, int mobility, int durability, int acceleration, int fatorsAvailable)
        {
            SpeedSlider.value = speed;
            MobilitySlider.value = mobility;
            DurabilitySlider.value = durability;
            AccelerationSlider.value = acceleration;

            //_speedPointsText.text = speed.ToString();
            //_mobilityPointsText.text = mobility.ToString();
            //_durabilityPointsText.text = durability.ToString();
            //_accelerationPointsText.text = acceleration.ToString();

            UpdateCurrentInfoValues(fatorsAvailable);
        }

        public void UpdateCarStatsProgress(string carName, int currentValue, int maxValue)
        {
            _carNameText.text = carName;
            _carStatsProgressText.text = $"{currentValue}/{maxValue}";
        }

        public void OpenStatsValuesPanel()
        {
            bool isActive = _statsValuesPanel.gameObject.activeSelf;
            isActive = !isActive;
            _statsValuesPanel.SetActive(isActive);
            _tuneWheelsViewPanel.SetActive(false);
            _tuneCarViewPanel.SetActive(false);
        }

        public void OpenTuneWheelsViewPanel()
        {
            bool isActive = _tuneWheelsViewPanel.gameObject.activeSelf;
            isActive = !isActive;
            _statsValuesPanel.SetActive(false);
            _tuneWheelsViewPanel.SetActive(isActive);
            _tuneCarViewPanel.SetActive(false);
        }

        public void OpenTuneCarViewPanel()
        {
            bool isActive = _tuneCarViewPanel.gameObject.activeSelf;
            isActive = !isActive;
            _statsValuesPanel.SetActive(false);
            _tuneWheelsViewPanel.SetActive(false);
            _tuneCarViewPanel.SetActive(isActive);
        }

        public void DeactivateAllPanels()
        {
            _statsValuesPanel.SetActive(false);
            _tuneWheelsViewPanel.SetActive(false);
            _tuneCarViewPanel.SetActive(false);
        }
    }
}