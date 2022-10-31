using RaceManager.Cars;
using RaceManager.Root;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace RaceManager.UI
{
    public class MainUI : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private List<ChestSlot> _chestSlots;
        [SerializeField] private ChestProgressPanel _chestProgress;
        [SerializeField] private CupsProgressPanel _cupsProgress;
        [SerializeField] private CurrencyAmountPanel _currencyAmount;
        [SerializeField] private TuningPanel _tuningPanel;

        private SaveManager _saveManager;
        private CarsDepot _playerCarDepot;
        private CarProfile _currentCarProfile;

        public IObservable<float> OnSpeedValueChange;
        public IObservable<float> OnMobilityValueChange;
        public IObservable<float> OnDurabilityValueChange;
        public IObservable<float> OnAccelerationValueChange;

        [Inject]
        private void Construct(SaveManager saveManager, CarsDepot playerCarDepot)
        {
            _saveManager = saveManager;
            _playerCarDepot = playerCarDepot;

            OnSpeedValueChange = _tuningPanel.SpeedSlider.OnValueChangedAsObservable();
            OnMobilityValueChange = _tuningPanel.MobilitySlider.OnValueChangedAsObservable();
            OnDurabilityValueChange = _tuningPanel.DurabilitySlider.OnValueChangedAsObservable();
            OnAccelerationValueChange = _tuningPanel.AccelerationSlider.OnValueChangedAsObservable();
        }

        private void Start()
        {
            UpdateSlidersValues();
            RegisterButtonsListeners();
        }

        private void StartRace()
        {
            _saveManager.Save();
            Loader.Load(Loader.Scene.RaceScene);
        }

        private void OpenStatsValuesPanel()
        { 
            
        }

        private void UpdateSlidersValues()
        {
            _currentCarProfile = _playerCarDepot.CurrentCarProfile;
            var c = _currentCarProfile.CarCharacteristics;
            _tuningPanel.SetValueToSlider(CarCharacteristics.Speed, c.CurrentSpeedFactor);
            _tuningPanel.SetValueToSlider(CarCharacteristics.Mobility, c.CurrentMobilityFactor);
            _tuningPanel.SetValueToSlider(CarCharacteristics.Durability, c.CurrentDurabilityFactor);
            _tuningPanel.SetValueToSlider(CarCharacteristics.Acceleration, c.CurrentAccelerationFactor);
        }

        private void RegisterButtonsListeners()
        {
            _startButton.onClick.AddListener(StartRace);

            _tuningPanel.TuneStatsButton.onClick.AddListener(OpenStatsValuesPanel);
        }
    }
}

