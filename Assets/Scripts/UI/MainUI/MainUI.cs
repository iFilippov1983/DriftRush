using RaceManager.Cars;
using RaceManager.Progress;
using RaceManager.Root;
using RaceManager.Shed;
using RaceManager.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private Button _gameProgressButton;
        [SerializeField] private Button _settingsButton;
        [Space]
        [SerializeField] private CupsProgressPanel _cupsProgress;
        [SerializeField] private CurrencyAmountPanel _currencyAmount;
        [Space]
        [SerializeField] private TuningPanel _tuningPanel;
        [SerializeField] private GameProgressPanel _gameProgressPanel;
        [SerializeField] private CarsCollectionPanel _carsCollectionPanel;
        [SerializeField] private BottomPanel _bottomPanel;
        [SerializeField] private BackPanel _backPanel;
        [Space]
        [SerializeField] private LootboxSlotsHandler _lootboxSlotsHandler;
        [SerializeField] private LootboxWindow _lootboxWindow;

        private PlayerProfile _playerProfile;
        private RewardsHandler _rewardsHandler;
        private SaveManager _saveManager;
        private CarsDepot _playerCarDepot;
        private CarProfile _currentCarProfile;
        private PodiumView _podium;
        private GameProgressScheme _gameProgressScheme;

        private GraphicRaycaster _graphicRaycaster;
        private PointerEventData _clickData;
        private List<RaycastResult> _raycastResults;

        private bool _inMainMenu = true;

        public Action<bool> OnMainMenuActivityChange;
        public Action<CarName> OnCarProfileChange;

        public IObservable<float> OnSpeedValueChange => _tuningPanel.SpeedSlider.onValueChanged.AsObservable();
        public IObservable<float> OnMobilityValueChange => _tuningPanel.MobilitySlider.onValueChanged.AsObservable();
        public IObservable<float> OnDurabilityValueChange => _tuningPanel.DurabilitySlider.onValueChanged.AsObservable();
        public IObservable<float> OnAccelerationValueChange => _tuningPanel.AccelerationSlider.onValueChanged.AsObservable();

        public IObserver<TuneData> OnCharValueLimit => Observer.Create((TuneData td) => SetTuningPanelValues(td));
        public Action<int> OnTuneValuesChange => (int v) => _tuningPanel.UpdateCurrentInfoValues(v);

        [Inject]
        private void Construct
            (
            PlayerProfile playerProfile, 
            GameProgressScheme gameProgressScheme,
            RewardsHandler rewardsHandler,
            SaveManager saveManager, 
            CarsDepot playerCarDepot, 
            PodiumView podium
            )
        {
            _playerProfile = playerProfile;
            _rewardsHandler = rewardsHandler;
            _gameProgressScheme = gameProgressScheme;
            _saveManager = saveManager;
            _playerCarDepot = playerCarDepot;
            _podium = podium;
        }

        public void Initialize()
        {
            _currentCarProfile = _playerCarDepot.CurrentCarProfile;

            UpdateCurrencyAmountPanels();
            UpdateTuningPanelValues();

            InitializeCarsCollectionPanel();
            InitializeGameProgressPanel();
            InitializeLootboxSlotsHandler();

            RegisterButtonsListeners();
        }

        private void Start()
        {
            _graphicRaycaster = GetComponent<GraphicRaycaster>();
            _clickData = new PointerEventData(EventSystem.current);
            _raycastResults = new List<RaycastResult>();

            ActivateMainMenu(true);
        }

        //private void Update()
        //{
        //    if (Input.GetMouseButtonDown(0))
        //    {
        //        GetUIElementClicked();
        //    }
        //}

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
                if(element.gameObject.CompareTag(Tag.BackPanel))
                    backPanelClicked = true;
                else
                    otherElementClicked = true;
            }

            if (backPanelClicked && !otherElementClicked)
            {
                ActivateTuningPanel(false);
                ActivateCarsCollectionPanel(false);
            }
        }

        #region Activate methods
        private void ActivateMainMenu(bool active)
        {
            _bottomPanel.SetActive(active);
            _startButton.SetActive(active);
            _lootboxSlotsHandler.SetActive(active);
            _cupsProgress.SetActive(active);

            //_podium.ChestObject.SetActive(active);

            _inMainMenu = active;
            OnMainMenuActivityChange?.Invoke(active);
        }

        private void ActivateTuningPanel(bool active)
        {
            _tuningPanel.SetActive(active);
            _worldSpaceUI.SetActive(active);

            _bottomPanel.SetActive(active);
            _bottomPanel.TuningPressedImage.SetActive(active);
        }

        private void ActivateCarsCollectionPanel(bool active)
        {
            _carsCollectionPanel.SetActive(active);

            _bottomPanel.SetActive(active);
            _bottomPanel.CarsCollectionPressedImage.SetActive(active);
        }

        private void ActivateGameProgressPanel(bool active)
        { 
            _gameProgressPanel.SetActive(active);

            _bottomPanel.SetActive(!active);
            _podium.SetActive(!active);
        }

        private void ActivateLootboxWindow(List<CarCardReward> list)
        {
            _lootboxWindow.SetActive(true);
            UpdatePodiumActivity(true);
        }
        
        #endregion

        #region Initialize data methods
        private void InitializeSlidersMinMaxValues()
        {
            var c = _currentCarProfile.CarCharacteristics;
            _tuningPanel.SetBorderValues(CharacteristicType.Speed, c.MinSpeedFactor, c.MaxSpeedFactor);
            _tuningPanel.SetBorderValues(CharacteristicType.Mobility, c.MinMobilityFactor, c.MaxMobilityFactor);
            _tuningPanel.SetBorderValues(CharacteristicType.Durability, c.MinDurabilityFactor, c.MaxDurabilityFactor);
            _tuningPanel.SetBorderValues(CharacteristicType.Acceleration, c.MinAccelerationFactor, c.MaxAccelerationFactor);
        }

        private void InitializeCarsCollectionPanel()
        {
            foreach (var profile in _playerCarDepot.CarProfiles)
            {
                _carsCollectionPanel.AddCollectionCard
                    (
                    profile.CarName,
                    _playerProfile.CarCardsAmount(profile.CarName),
                    profile.Accessibility.CurrentStepPointsToAccess,
                    profile.Accessibility.CarIsAvailable
                    );
            }

            UpdateCarsCollectionInfo();

            _carsCollectionPanel.OnUseCarButtonPressed += ChangeCar;
        }

        private void InitializeGameProgressPanel()
        {
            foreach (var stepData in _gameProgressScheme.ProgressSteps)
            {
                int goalCupsAmount = stepData.Key;
                ProgressStep step = stepData.Value;
                step.IsReached = _playerProfile.Cups >= goalCupsAmount;

                _gameProgressPanel.AddProgressStep(goalCupsAmount, step, () => _rewardsHandler.RewardForProgress(goalCupsAmount));
            }

            _gameProgressPanel.SetCupsAmountSlider(_playerProfile.Cups);

            var closestGlobalGoal = _gameProgressScheme.ProgressSteps.First(p => p.Value.BigPrefab && p.Value.IsReached == false || p.Value.IsLast);
            int globalCupsAmountGoal = closestGlobalGoal.Key > _playerProfile.Cups
                ? closestGlobalGoal.Key
                : _gameProgressScheme.LastGlobalGoal.Key;


            InitializeCupsProgressPanel(globalCupsAmountGoal);

            _rewardsHandler.OnProgressReward += UpdateCurrencyAmountPanels;
            _rewardsHandler.OnLootboxOpen += ActivateLootboxWindow;
            _rewardsHandler.OnLootboxOpen += _lootboxWindow.RepresentLootbox;
            _rewardsHandler.OnLootboxOpen += (List<CarCardReward> list) => UpdateCurrencyAmountPanels();
        }

        private void InitializeCupsProgressPanel(int globalGoalCupsAmount)
        {
            int currentCupsAmount = _playerProfile.Cups;
            _cupsProgress.CupsAmountOwned.text = currentCupsAmount.ToString();
            _cupsProgress.NextGlobalGoalAmount.text = globalGoalCupsAmount.ToString();

            float fillAmount = 1f - (float)(globalGoalCupsAmount - currentCupsAmount) / (float)globalGoalCupsAmount;

            if (fillAmount > 1)
                fillAmount = 1f;

            _cupsProgress.FillImage.fillAmount = fillAmount;
            UpdateHasRewardsImage(_gameProgressScheme.HasUnreceivedRewards);
        }

        private void InitializeLootboxSlotsHandler()
        {
            _lootboxSlotsHandler.Initialize(_playerProfile);

            _lootboxSlotsHandler.OnPopupIsActive += UpdatePodiumActivity;
        }
        #endregion

        #region Update data methods
        public void UpdateTuningPanelValues()
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

        public void UpdateCarsCollectionInfo()
        {
            _carsCollectionPanel.UpdateStatsProgress
                (
                _currentCarProfile.CarName.ToString(),
                _currentCarProfile.CarCharacteristics.CurrentFactorsProgress,
                _currentCarProfile.CarCharacteristics.FactorsMaxTotal
                );
        }

        private void UpdateCurrencyAmountPanels()
        {
            _currencyAmount.MoneyAmount.text = _playerProfile.Money.ToString();
            _currencyAmount.GemsAmount.text = _playerProfile.Gems.ToString();

            _gameProgressPanel.MoneyAmountText.text = _playerProfile.Money.ToString();
            _gameProgressPanel.gemsAmountText.text = _playerProfile.Gems.ToString();
        }

        private void UpdateHasRewardsImage(bool hasUnreceived) => _cupsProgress.HasUnreceivedRewardsImage.SetActive(hasUnreceived);
        private void UpdatePodiumActivity(bool needToHide) => _podium.SetActive(!needToHide);
        #endregion

        #region Other methods
        private void SetTuningPanelValues(TuneData td)
        {
            _tuningPanel.SetValueToSlider(td.cType, td.value);
            _tuningPanel.UpdateCurrentInfoValues(td.available);
        }

        private void ChangeCar(CarName newCarName)
        {
            if (newCarName == _playerCarDepot.CurrentCarName)
                return;

            OnCarProfileChange?.Invoke(newCarName);
        }

        private void StartRace()
        {
            _saveManager.Save();
            Loader.Load(Loader.Scene.RaceScene);
        }

        private void RegisterButtonsListeners()
        {
            _startButton.onClick.AddListener(StartRace);

            _bottomPanel.TuneButton.onClick.AddListener(() => ActivateCarsCollectionPanel(false));
            _bottomPanel.TuneButton.onClick.AddListener(() => ActivateMainMenu(false));
            _bottomPanel.TuneButton.onClick.AddListener(() => ActivateTuningPanel(true));

            _bottomPanel.MainMenuButton.onClick.AddListener(() => ActivateCarsCollectionPanel(false));
            _bottomPanel.MainMenuButton.onClick.AddListener(() => ActivateTuningPanel(false));
            _bottomPanel.MainMenuButton.onClick.AddListener(() => ActivateMainMenu(true));

            _bottomPanel.CarsCollectionButton.onClick.AddListener(() => ActivateTuningPanel(false));
            _bottomPanel.CarsCollectionButton.onClick.AddListener(() => ActivateMainMenu(false));
            _bottomPanel.CarsCollectionButton.onClick.AddListener(() => ActivateCarsCollectionPanel(true));

            _tuningPanel.RegisterButtonsListeners();
            _tuningPanel.CloseButton.onClick.AddListener(() => ActivateTuningPanel(false));
            _tuningPanel.CloseButton.onClick.AddListener(() => ActivateMainMenu(true));

            _carsCollectionPanel.CloseButton.onClick.AddListener(() => ActivateCarsCollectionPanel(false));
            _carsCollectionPanel.CloseButton.onClick.AddListener(() => ActivateMainMenu(true));

            _gameProgressPanel.BackButton.onClick.AddListener(() => ActivateGameProgressPanel(false));
            _gameProgressPanel.BackButton.onClick.AddListener(() => UpdateHasRewardsImage(_gameProgressScheme.HasUnreceivedRewards));

            _gameProgressButton.onClick.AddListener(() => ActivateGameProgressPanel(true));
            _gameProgressButton.onClick.AddListener(_gameProgressPanel.OffsetContent);

            _lootboxWindow.OkButton.onClick.AddListener(() => _lootboxWindow.SetActive(false));
            _lootboxWindow.OkButton.onClick.AddListener(() => _lootboxSlotsHandler.InitializeLootboxProgressPanel());
            _lootboxWindow.OkButton.onClick.AddListener(() => UpdatePodiumActivity(false));
        }

        private void OnDestroy()
        {
            _carsCollectionPanel.OnUseCarButtonPressed -= ChangeCar;

            _rewardsHandler.OnProgressReward -= UpdateCurrencyAmountPanels;
            _rewardsHandler.OnLootboxOpen -= ActivateLootboxWindow;
            _rewardsHandler.OnLootboxOpen -= _lootboxWindow.RepresentLootbox;
            _rewardsHandler.OnLootboxOpen -= (List<CarCardReward> list) => UpdateCurrencyAmountPanels();

            _lootboxSlotsHandler.OnPopupIsActive -= UpdatePodiumActivity;
        }
        #endregion
    }
}

