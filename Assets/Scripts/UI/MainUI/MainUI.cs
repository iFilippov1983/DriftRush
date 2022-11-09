using RaceManager.Cars;
using RaceManager.Root;
using RaceManager.Shed;
using RaceManager.Tools;
using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace RaceManager.UI
{
    public class MainUI : MonoBehaviour, Root.IInitializable
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
        [SerializeField] private BackPanel _backPanel;
        [Space]
        [SerializeField] private RectTransform _chestSlotsRect;
        [SerializeField] private List<ChestSlot> _chestSlots;

        private SaveManager _saveManager;
        private CarsDepot _playerCarDepot;
        private CarProfile _currentCarProfile;
        private Podium _podium;

        private GraphicRaycaster _graphicRaycaster;
        private PointerEventData _clickData;
        private List<RaycastResult> _raycastResults;

        private bool _inMainMenu = true;

        public Action OnMenuViewChange;
        public Action OnCarProfileChange;

        public IObservable<float> OnSpeedValueChange => _tuningPanel.SpeedSlider.onValueChanged.AsObservable();
        public IObservable<float> OnMobilityValueChange => _tuningPanel.MobilitySlider.onValueChanged.AsObservable();
        public IObservable<float> OnDurabilityValueChange => _tuningPanel.DurabilitySlider.onValueChanged.AsObservable();
        public IObservable<float> OnAccelerationValueChange => _tuningPanel.AccelerationSlider.onValueChanged.AsObservable();

        public IObserver<TuneData> OnCharValueLimit => Observer.Create((TuneData td) => SetTuningPanelValues(td));
        public Action<int> OnTuneValuesChange => (int v) => _tuningPanel.UpdateCurrentInfoValues(v);

        [Inject]
        private void Construct(SaveManager saveManager, CarsDepot playerCarDepot, Podium podium)
        {
            _saveManager = saveManager;
            _playerCarDepot = playerCarDepot;
            _podium = podium;

            OnCarProfileChange += UpdateTuningPanelValues;
        }

        public void Initialize()
        {
            _currentCarProfile = _playerCarDepot.CurrentCarProfile;

            UpdateTuningPanelValues();
            RegisterButtonsListeners();
        }

        private void Start()
        {
            _graphicRaycaster = GetComponent<GraphicRaycaster>();
            _clickData = new PointerEventData(EventSystem.current);
            _raycastResults = new List<RaycastResult>();

            OpenMainMenu(_inMainMenu);
            //InitializeUIElements();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                GetUIElementClicked();
            }
        }

        private void GetUIElementClicked()
        {
            if (_inMainMenu)
                return;

            _clickData.position = Input.mousePosition;
            _raycastResults.Clear();
            _graphicRaycaster.Raycast(_clickData, _raycastResults);
            bool backPanelClicked = false;
            bool otherElementClicked = false;

            foreach (var element in _raycastResults)
            { 
                if(element.gameObject.tag.Equals(Tag.BackPanel))
                    backPanelClicked = true;
                else
                    otherElementClicked = true;
            }

            if (backPanelClicked && !otherElementClicked)
            {
                ToggleTuningPanel();
            }
        }

        //private void InitializeUIElements()
        //{
        //    _currentCarProfile = _playerCarDepot.CurrentCarProfile;

        //    UpdateTuningPanelValues();
        //    RegisterButtonsListeners();
        //}

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

            _podium.ChestObject.SetActive(active);
        }

        private void ToggleTuningPanel()
        {
            _inMainMenu = !_inMainMenu;
            OpenMainMenu(_inMainMenu);
            _bottomPanel.SetActive(true);
            _tuningPanel.SetActive(!_inMainMenu);
            _worldSpaceUI.SetActive(!_inMainMenu);

            OnMenuViewChange?.Invoke();
        }

        private void UpdateTuningPanelValues()
        {
            _currentCarProfile = _playerCarDepot.CurrentCarProfile;

            InitializeSlidersMinMaxValues();

            var c = _currentCarProfile.CarCharacteristics;
            _tuningPanel.UpdateAllSlidersValues
                (
                c.CurrentSpeedFactor, 
                c.CurrentMobilityFactor, 
                c.CurrentDurabilityFactor, 
                c.CurrentAccelerationFactor,
                c.AvailableFactorsToUse
                );

            _tuningPanel.UpdateCarStatsProgress
                (
                _currentCarProfile.CarName.ToString(), 
                _currentCarProfile.CarCharacteristics.CurrentFactorsProgress, 
                _currentCarProfile.CarCharacteristics.FactorsMaxTotal
                );
        }

        private void SetTuningPanelValues(TuneData td)
        {
            _tuningPanel.SetValueToSlider(td.cType, td.value);
            _tuningPanel.UpdateCurrentInfoValues(td.available);
        }
            

        private void InitializeSlidersMinMaxValues()
        {
            var c = _currentCarProfile.CarCharacteristics;
            _tuningPanel.SetBorderValues(CharType.Speed, c.MinSpeedFactor, c.MaxSpeedFactor);
            _tuningPanel.SetBorderValues(CharType.Mobility, c.MinMobilityFactor, c.MaxMobilityFactor);
            _tuningPanel.SetBorderValues(CharType.Durability, c.MinDurabilityFactor, c.MaxDurabilityFactor);
            _tuningPanel.SetBorderValues(CharType.Acceleration, c.MinAccelerationFactor, c.MaxAccelerationFactor);
        }

        private void RegisterButtonsListeners()
        {
            _startButton.onClick.AddListener(StartRace);

            _bottomPanel.TuneButton.onClick.AddListener(ToggleTuningPanel);

            _tuningPanel.RegisterButtonsListeners();
        }

        private void OnDestroy()
        {
            OnCarProfileChange -= UpdateTuningPanelValues;
        }
    }
}

