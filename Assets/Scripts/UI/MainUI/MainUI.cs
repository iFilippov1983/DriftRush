using RaceManager.Cars;
using RaceManager.Root;
using RaceManager.Shed;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace RaceManager.UI
{
    public class MainUI : MonoBehaviour
    {
        [SerializeField] private WorldSpaceUI _worldSpaceUI;
        [Space]
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _settingsButton;
        [Space]
        [SerializeField] private ChestProgressPanel _chestProgress;
        [SerializeField] private CupsProgressPanel _cupsProgress;
        [SerializeField] private CurrencyAmountPanel _currencyAmount;
        [SerializeField] private TuningPanel _tuningPanel;
        [SerializeField] private BottomPanelView _bottomPanel;
        [Space]
        [SerializeField] private RectTransform _chestSlotsRect;
        [SerializeField] private List<ChestSlot> _chestSlots;

        private SaveManager _saveManager;
        private CarsDepot _playerCarDepot;
        private CarProfile _currentCarProfile;
        private Podium _podium;

        private bool _inMainMenu = true;

        public IObservable<float> OnSpeedValueChange;
        public IObservable<float> OnMobilityValueChange;
        public IObservable<float> OnDurabilityValueChange;
        public IObservable<float> OnAccelerationValueChange;

        [Inject]
        private void Construct(SaveManager saveManager, CarsDepot playerCarDepot, Podium podium)
        {
            _saveManager = saveManager;
            _playerCarDepot = playerCarDepot;
            _podium = podium;

            OnSpeedValueChange = _tuningPanel.SpeedSlider.OnValueChangedAsObservable();
            OnMobilityValueChange = _tuningPanel.MobilitySlider.OnValueChangedAsObservable();
            OnDurabilityValueChange = _tuningPanel.DurabilitySlider.OnValueChangedAsObservable();
            OnAccelerationValueChange = _tuningPanel.AccelerationSlider.OnValueChangedAsObservable();
        }

        private void Start()
        {
            OpenMainMenu(_inMainMenu);
            UpdateSlidersValues();
            RegisterButtonsListeners();
        }

        private void StartRace()
        {
            _saveManager.Save();
            Loader.Load(Loader.Scene.RaceScene);
        }

        private void OpenMainMenu(bool active)
        {
            _bottomPanel.SetActive(active);
            _startButton.SetActive(active);
            _chestSlotsRect.SetActive(active);
            _chestProgress.SetActive(active);
            _cupsProgress.SetActive(active);
        }

        private void ToggleTuningPanel()
        {
            _inMainMenu = !_inMainMenu;
            OpenMainMenu(_inMainMenu);
            _bottomPanel.SetActive(true);
            _tuningPanel.SetActive(!_inMainMenu);

            _tuningPanel.OpenStatsValuesPanel();
        }

        private void UpdateSlidersValues()
        {
            _currentCarProfile = _playerCarDepot.CurrentCarProfile;
            var c = _currentCarProfile.CarCharacteristics;
            _tuningPanel.UpdateAllSlidersValues
                (
                c.CurrentSpeedFactor, 
                c.CurrentMobilityFactor, 
                c.CurrentDurabilityFactor, 
                c.CurrentAccelerationFactor
                );

            _tuningPanel.UpdateCarStatsProgress
                (
                _currentCarProfile.CarName.ToString(), 
                _currentCarProfile.CarCharacteristics.CurrentFactorsProgress, 
                _currentCarProfile.CarCharacteristics.FactorsTotal
                );
        }

        private void RegisterButtonsListeners()
        {
            _startButton.onClick.AddListener(StartRace);

            _bottomPanel.TuneButton.onClick.AddListener(ToggleTuningPanel);

            _tuningPanel.RegisterButtonsListeners();
        }
    }
}

