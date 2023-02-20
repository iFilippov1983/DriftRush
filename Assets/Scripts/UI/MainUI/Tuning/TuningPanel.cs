﻿using RaceManager.Cars;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class TuningPanel : MonoBehaviour
    {
        [SerializeField] private UpgradeWindowTuner _upgradeWindow;
        [Space]
        [SerializeField] private Button _tuneStatsButton;
        [SerializeField] private Button _tuneWheelsViewButton;
        [SerializeField] private Button _tuneCarViewButton;
        [SerializeField] private Button _closePanelButton;
        [SerializeField] private Button _closePanelWindowButton;
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
        //[SerializeField] private TMP_Text _speedPointsMaxText;
        [Space]
        [SerializeField] private TMP_Text _mobilityPointsText;
        //[SerializeField] private TMP_Text _mobilityPointsMaxText;
        [Space]
        [SerializeField] private TMP_Text _durabilityPointsText;
        //[SerializeField] private TMP_Text _durabilityPointsMaxText;
        [Space]
        [SerializeField] private TMP_Text _accelerationPointsText;
        //[SerializeField] private TMP_Text _accelerationPointsMaxText;
        [Space]
        [SerializeField] private Image _tuneCarStatsActiveImage;
        [SerializeField] private Image _tuneWheelsActiveImage;
        [SerializeField] private Image _tuneCarViewActiveImage;

        public Slider SpeedSlider => _speedSlider;
        public Slider MobilitySlider => _mobilitySlider;
        public Slider DurabilitySlider => _durabilitySlider;
        public Slider AccelerationSlider => _accelerationSlider;

        public UpgradeWindowTuner UpgradeWindow => _upgradeWindow;

        public Button UpgradeButton => _upgradeWindow.UpgradeButton;
        public Button TuneStatsButton => _tuneStatsButton;
        public Button TuneWeelsViewButton => _tuneWheelsViewButton;
        public Button TuneCarViewButton => _tuneCarViewButton;
        public Button ClosePanelButton => _closePanelButton;
        public Button ClosePanelWindowButton => _closePanelWindowButton;

        public TMP_Text UpgradeCostText => _upgradeWindow.CostText;
        public TMP_Text PartsAmountText => _upgradeWindow.PartsAmountText;

        public Action OnButtonPressed;

        private void OnEnable()
        {
            OpenStatsValuesPanel();
        }

        private void OnDisable()
        {
            DeactivateAllPanels();
            DeactivetaAllPanelActiveImages();
        }

        public void RegisterButtonsListeners()
        {
            _tuneStatsButton.onClick.AddListener(OpenStatsValuesPanel);

            _tuneWheelsViewButton.onClick.AddListener(OpenTuneWheelsViewPanel);

            _tuneCarViewButton.onClick.AddListener(OpenTuneCarViewPanel);

            _closePanelButton.onClick.AddListener(DeactivateAllPanels);

            _closePanelWindowButton.onClick.AddListener(DeactivateAllPanels);
        }

        public void SetBorderValues(CharacteristicType characteristics, int minValue, int maxValue)
        {
            //Debug.Log($"BORDER => {characteristics} - min {minValue} - max {maxValue}");
            switch (characteristics)
            {
                case CharacteristicType.Speed:
                    SpeedSlider.minValue = minValue;
                    SpeedSlider.maxValue = maxValue;
                    //_speedPointsMaxText.text = maxValue.ToString();
                    break;
                case CharacteristicType.Mobility:
                    MobilitySlider.minValue = minValue;
                    MobilitySlider.maxValue = maxValue;
                    //_mobilityPointsMaxText.text = maxValue.ToString();
                    break;
                case CharacteristicType.Durability:
                    DurabilitySlider.minValue = minValue;
                    DurabilitySlider.maxValue = maxValue;
                    //_durabilityPointsMaxText.text = maxValue.ToString();
                    break;
                case CharacteristicType.Acceleration:
                    AccelerationSlider.minValue = minValue;
                    AccelerationSlider.maxValue = maxValue;
                    //_accelerationPointsMaxText.text = maxValue.ToString();
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

            UpdateCurrentInfoValues(fatorsAvailable);
        }

        public void UpdateCarStatsProgress(string carName, int currentValue)
        {
            string name = carName.SplitByUppercaseWith(" ");

            _carNameText.text = name.ToUpper();
            _carStatsProgressText.text = $"{currentValue}";
        }

        public void OpenStatsValuesPanel()
        {
            DeactivateAllPanels();
            _statsValuesPanel.SetActive(true);

            DeactivetaAllPanelActiveImages();
            _tuneCarStatsActiveImage.SetActive(true);
        }

        public void OpenTuneWheelsViewPanel()
        {
            DeactivateAllPanels();
            _tuneWheelsViewPanel.SetActive(true);

            DeactivetaAllPanelActiveImages();
            _tuneWheelsActiveImage.SetActive(true);
        }

        public void OpenTuneCarViewPanel()
        {
            DeactivateAllPanels();
            _tuneCarViewPanel.SetActive(true);

            DeactivetaAllPanelActiveImages();
            _tuneCarViewActiveImage.SetActive(true);
        }

        public void DeactivateAllPanels()
        {
            _statsValuesPanel.SetActive(false);
            _tuneWheelsViewPanel.SetActive(false);
            _tuneCarViewPanel.SetActive(false);
        }

        public void DeactivetaAllPanelActiveImages()
        { 
            _tuneCarStatsActiveImage.SetActive(false);
            _tuneWheelsActiveImage.SetActive(false);
            _tuneCarViewActiveImage.SetActive(false);
        }
    }
}