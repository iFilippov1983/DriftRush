using RaceManager.Cars;
using RaceManager.Progress;
using RaceManager.Root;
using RaceManager.Shed;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using IInitializable = RaceManager.Root.IInitializable;

namespace RaceManager.UI
{
    public class MainUI : MonoBehaviour, IInitializable
    {
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
        [Space]
        [SerializeField] private LootboxSlotsHandler _lootboxSlotsHandler;
        [SerializeField] private LootboxWindow _lootboxWindow;
        [Space]
        [SerializeField] private SettingsPopup _settingsPopup;
        [SerializeField] private ShopPanel _shopPanel;

        private PlayerProfile _playerProfile;
        private RewardsHandler _rewardsHandler;
        private CarUpgradesHandler _upgradesHandler;
        private SaveManager _saveManager;
        private CarsDepot _playerCarDepot;
        private CarProfile _currentCarProfile;
        private PodiumView _podium;
        private GameProgressScheme _gameProgressScheme;

        public Action<bool> OnMainMenuActivityChange;
        public Action<CarName> OnCarProfileChange;
        public Action<string> OnButtonPressed;

        #region Car tuning properties

        public IObservable<float> OnSpeedValueChange => _tuningPanel.SpeedSlider.onValueChanged.AsObservable();
        public IObservable<float> OnMobilityValueChange => _tuningPanel.MobilitySlider.onValueChanged.AsObservable();
        public IObservable<float> OnDurabilityValueChange => _tuningPanel.DurabilitySlider.onValueChanged.AsObservable();
        public IObservable<float> OnAccelerationValueChange => _tuningPanel.AccelerationSlider.onValueChanged.AsObservable();

        public IObserver<TuneData> OnCharValueLimit => Observer.Create((TuneData td) => SetTuningPanelValues(td));
        public Action<int> OnTuneValuesChange => (int v) => _tuningPanel.UpdateCurrentInfoValues(v);

        #endregion

        #region Settings change properties

        public IObservable<float> OnSoundsSettingChange => _settingsPopup.SoundToggleSlider.onValueChanged.AsObservable();
        public IObservable<float> OnMusicSettingChange => _settingsPopup.MusicToggleSlider.onValueChanged.AsObservable();
        public IObservable<float> OnVibroSettingChange => _settingsPopup.VibrationToggleSlider.onValueChanged.AsObservable();
        public IObservable<float> OnRaceLineSettingsChange => _settingsPopup.RaceLineToggleSlider.onValueChanged.AsObservable();

        public IObserver<SettingsData> OnSettingsInitialize => Observer.Create((SettingsData sd) => SetSettingsPopupValues(sd));

        #endregion

        #region Shop properties

        public ShopPanel ShopPanel => _shopPanel;

        #endregion

        #region Initial methods

        [Inject]
        private void Construct
            (
            PlayerProfile playerProfile,
            GameProgressScheme gameProgressScheme,
            RewardsHandler rewardsHandler,
            CarUpgradesHandler upgradesHandler,
            SaveManager saveManager,
            CarsDepot playerCarDepot,
            PodiumView podium
            )
        {
            _playerProfile = playerProfile;
            _rewardsHandler = rewardsHandler;
            _upgradesHandler = upgradesHandler;
            _gameProgressScheme = gameProgressScheme;
            _saveManager = saveManager;
            _playerCarDepot = playerCarDepot;
            _podium = podium;

            RegisterButtonsListeners();
        }

        public void Initialize()
        {
            _currentCarProfile = _playerCarDepot.CurrentCarProfile;

            InitializeCarsCollectionPanel();
            InitializeGameProgressPanel();
            InitializeLootboxSlotsHandler();

            UpdateCurrencyAmountPanels();
            UpdateTuningPanelValues();
        }

        private void Awake()
        {
            ActivateMainMenu(true);
        }

        #endregion

        #region Activate Functions

        private void ActivateMainMenu(bool active)
        {
            _bottomPanel.SetActive(active);
            _startButton.SetActive(active);
            _lootboxSlotsHandler.SetActive(active);
            _cupsProgress.SetActive(active);

            OnMainMenuActivityChange?.Invoke(active);
        }

        private void ActivateTuningPanel(bool active)
        {
            _tuningPanel.SetActive(active);

            _bottomPanel.SetActive(active);
            _bottomPanel.TuningPressedImage.SetActive(active);
        }

        private void ActivateCarsCollectionPanel(bool active)
        {
            _carsCollectionPanel.SetActive(active);

            _bottomPanel.SetActive(active);
            _bottomPanel.CarsCollectionPressedImage.SetActive(active);
        }

        private void ActivateShopPanel(bool active)
        {
            _shopPanel.SetActive(active);

            _bottomPanel.SetActive(active);
            _bottomPanel.IapSopPressedImage.SetActive(active);
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

        private void ActivateCarWindow(bool active)
        {
            _carsCollectionPanel.CarWindow.SetActive(active);

            _bottomPanel.SetActive(!active);
        }

        private void ActivateSettingsPopup(bool active) => _settingsPopup.SetActive(active);

        #endregion

        #region Initialize Data Functions

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
                    profile.RankingScheme.CurrentRankPointsForAccess,
                    profile.RankingScheme.CarIsAvailable,
                    profile.RankingScheme.AllRanksGranted
                    );

                if (profile.CarName == _currentCarProfile.CarName)
                    _carsCollectionPanel.UsedCarName = profile.CarName;
            }

            UpdateCarsCollectionInfo();

            _carsCollectionPanel.OnUseCar += ChangeCar;
            _carsCollectionPanel.OnCarWindowOpen += InitializeCarWindow;
            _carsCollectionPanel.OnButtonPressed += OnButtonPressedMethod;
        }

        private void InitializeCarWindow()
        {
            UpdateCarWindow();
            ActivateCarWindow(true);

            _carsCollectionPanel.CarWindowUpgradeButton.onClick.RemoveAllListeners();
            _carsCollectionPanel.CarWindowUpgradeButton.onClick.AddListener(CarRankUpgrade);
            _carsCollectionPanel.CarWindowUpgradeButton.onClick.AddListener(() => OnButtonPressedMethod(_carsCollectionPanel.CarWindowUpgradeButton));

            _carsCollectionPanel.CarWindowBackButton.onClick.RemoveAllListeners();
            _carsCollectionPanel.CarWindowBackButton.onClick.AddListener(() => ActivateCarWindow(false));
            _carsCollectionPanel.CarWindowBackButton.onClick.AddListener(() => OnButtonPressedMethod(_carsCollectionPanel.CarWindowBackButton));
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

            _gameProgressPanel.OnButtonPressed += OnButtonPressedMethod;
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
            _lootboxSlotsHandler.OnButtonPressed += OnButtonPressedMethod;
        }
        #endregion

        #region Update Data Functions

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

            _tuningPanel.UpgradeWindow.SetActive(_upgradesHandler.CanUpgradeCurrentCarFactors());
            _tuningPanel.UpgradeCostText.text = _upgradesHandler.CurrentUpgradeCost.ToString();
            _tuningPanel.PartsAmountText.text = _upgradesHandler.CurrentUpgradeStatsToAdd.ToString();
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

        public void UpdateCarsCollectionCards(CarName carName)
        {
            CarProfile profile = _playerCarDepot.CarProfiles.Find(p => p.CarName == carName);

            _carsCollectionPanel.UpdateCard
                    (
                    profile.CarName,
                    _playerProfile.CarCardsAmount(profile.CarName),
                    profile.RankingScheme.CurrentRankPointsForAccess,
                    profile.RankingScheme.CarIsAvailable,
                    profile.RankingScheme.AllRanksGranted
                    );

            if (profile.CarName == _currentCarProfile.CarName)
                _carsCollectionPanel.UsedCarName = profile.CarName;
        }

        public void UpdateCarWindow()
        {
            var rank = _currentCarProfile.RankingScheme.CurrentRank;
            int cost = rank.AccessCost;
            bool isUpgraded = rank.IsGranted;
            bool isAvailable = rank.IsReached;

            _carsCollectionPanel.SetCarWindow(cost, isUpgraded, isAvailable);
        }

        public void UpdateCurrencyAmountPanels()
        {
            _currencyAmount.MoneyAmount.text = _playerProfile.Money.ToString();
            _currencyAmount.GemsAmount.text = _playerProfile.Gems.ToString();

            _gameProgressPanel.MoneyAmountText.text = _playerProfile.Money.ToString();
            _gameProgressPanel.gemsAmountText.text = _playerProfile.Gems.ToString();
        }

        public void UpdateGameProgressPanel()
        {
            _rewardsHandler.OnProgressReward -= UpdateCurrencyAmountPanels;
            _rewardsHandler.OnLootboxOpen -= ActivateLootboxWindow;
            _rewardsHandler.OnLootboxOpen -= _lootboxWindow.RepresentLootbox;
            _rewardsHandler.OnLootboxOpen -= (List<CarCardReward> list) => UpdateCurrencyAmountPanels();

            _gameProgressPanel.ClearProgressSteps();
            InitializeGameProgressPanel();
        }

        private void UpdateHasRewardsImage(bool hasUnreceived) => _cupsProgress.HasUnreceivedRewardsImage.SetActive(hasUnreceived);
        private void UpdatePodiumActivity(bool needToHide) => _podium.SetActive(!needToHide);

        #endregion 

        #region Other Private Functions

        private void SetTuningPanelValues(TuneData td)
        {
            _tuningPanel.SetValueToSlider(td.cType, td.value);
            _tuningPanel.UpdateCurrentInfoValues(td.available);
        }

        private void SetSettingsPopupValues(SettingsData sd)
        {
            _settingsPopup.SoundToggleSlider.value = sd.playSounds ? 1f : 0f;
            _settingsPopup.MusicToggleSlider.value = sd.playMusic ? 1f : 0f;
            _settingsPopup.VibrationToggleSlider.value = sd.useHaptics ? 1f : 0f;
            _settingsPopup.RaceLineToggleSlider.value = sd.useRaceLine ? 1f : 0f;
        }

        private void ChangeCar(CarName newCarName)
        {
            if (newCarName == _playerCarDepot.CurrentCarName)
                return;

            OnCarProfileChange?.Invoke(newCarName);
        }

        private void CarRankUpgrade()
        {
            if (_upgradesHandler.TryUpgradeCurrentCarRank())
            {
                UpdateCurrencyAmountPanels();
                //TODO: Visualize upgrade success
            }
            else
            {
                //TODO: Visualize upgrade fail
            }
        }

        private void CarFactorsUpgrade()
        {
            if (_upgradesHandler.TryUpgradeCurrentCarFactors())
            {
                UpdateCurrencyAmountPanels();
                //TODO: Visualize upgrade success
            }
            else
            {
                //TODO: Visualize upgrade fail
            }
        }

        private void StartRace()
        {
            _saveManager.Save();
            Loader.Load(Loader.Scene.RaceScene);
        }

        private void OnButtonPressedMethod(Button button) => OnButtonPressed?.Invoke(button.gameObject.name);

        private void RegisterButtonsListeners()
        {
            _startButton.onClick.AddListener(StartRace);
            _startButton.onClick.AddListener(() => OnButtonPressedMethod(_startButton));

            _bottomPanel.TuneButton.onClick.AddListener(() => ActivateMainMenu(false));
            _bottomPanel.TuneButton.onClick.AddListener(() => ActivateCarsCollectionPanel(false));
            _bottomPanel.TuneButton.onClick.AddListener(() => ActivateShopPanel(false));
            _bottomPanel.TuneButton.onClick.AddListener(() => ActivateTuningPanel(true));
            _bottomPanel.TuneButton.onClick.AddListener(() => OnButtonPressedMethod(_bottomPanel.TuneButton));

            _bottomPanel.MainMenuButton.onClick.AddListener(() => ActivateTuningPanel(false));
            _bottomPanel.MainMenuButton.onClick.AddListener(() => ActivateCarsCollectionPanel(false));
            _bottomPanel.MainMenuButton.onClick.AddListener(() => ActivateShopPanel(false));
            _bottomPanel.MainMenuButton.onClick.AddListener(() => ActivateMainMenu(true));
            _bottomPanel.MainMenuButton.onClick.AddListener(() => OnButtonPressedMethod(_bottomPanel.MainMenuButton));

            _bottomPanel.CarsCollectionButton.onClick.AddListener(() => ActivateMainMenu(false));
            _bottomPanel.CarsCollectionButton.onClick.AddListener(() => ActivateTuningPanel(false));
            _bottomPanel.CarsCollectionButton.onClick.AddListener(() => ActivateShopPanel(false));
            _bottomPanel.CarsCollectionButton.onClick.AddListener(() => ActivateCarsCollectionPanel(true));
            _bottomPanel.CarsCollectionButton.onClick.AddListener(() => OnButtonPressedMethod(_bottomPanel.CarsCollectionButton));

            _bottomPanel.ShopButton.onClick.AddListener(() => ActivateTuningPanel(false));
            _bottomPanel.ShopButton.onClick.AddListener(() => ActivateMainMenu(false));
            _bottomPanel.ShopButton.onClick.AddListener(() => ActivateCarsCollectionPanel(false));
            _bottomPanel.ShopButton.onClick.AddListener(() => ActivateShopPanel(true));
            _bottomPanel.ShopButton.onClick.AddListener(() => OnButtonPressedMethod(_bottomPanel.ShopButton));

            _tuningPanel.RegisterButtonsListeners();

            _tuningPanel.TuneStatsButton.onClick.AddListener(() => OnButtonPressedMethod(_tuningPanel.TuneStatsButton));
            _tuningPanel.TuneWeelsViewButton.onClick.AddListener(() => OnButtonPressedMethod(_tuningPanel.TuneWeelsViewButton));
            _tuningPanel.TuneCarViewButton.onClick.AddListener(() => OnButtonPressedMethod(_tuningPanel.TuneCarViewButton));

            _tuningPanel.ClosePanelButton.onClick.AddListener(() => ActivateTuningPanel(false));
            _tuningPanel.ClosePanelButton.onClick.AddListener(() => ActivateMainMenu(true));
            _tuningPanel.ClosePanelButton.onClick.AddListener(() => OnButtonPressedMethod(_tuningPanel.ClosePanelButton));

            _tuningPanel.ClosePanelWindowButton.onClick.AddListener(() => ActivateTuningPanel(false));
            _tuningPanel.ClosePanelWindowButton.onClick.AddListener(() => ActivateMainMenu(true));
            _tuningPanel.ClosePanelWindowButton.onClick.AddListener(() => OnButtonPressedMethod(_tuningPanel.ClosePanelWindowButton));

            _tuningPanel.UpgradeButton.onClick.AddListener(CarFactorsUpgrade);
            _tuningPanel.UpgradeButton.onClick.AddListener(() => OnButtonPressedMethod(_tuningPanel.UpgradeButton));

            _carsCollectionPanel.CloseButton.onClick.AddListener(() => ActivateCarsCollectionPanel(false));
            _carsCollectionPanel.CloseButton.onClick.AddListener(() => ActivateMainMenu(true));
            _carsCollectionPanel.CloseButton.onClick.AddListener(() => OnButtonPressedMethod(_carsCollectionPanel.CloseButton));

            _carsCollectionPanel.ClosePanelWindowButton.onClick.AddListener(() => ActivateCarsCollectionPanel(false));
            _carsCollectionPanel.ClosePanelWindowButton.onClick.AddListener(() => ActivateMainMenu(true));
            _carsCollectionPanel.ClosePanelWindowButton.onClick.AddListener(() => OnButtonPressedMethod(_carsCollectionPanel.ClosePanelWindowButton));

            _gameProgressPanel.BackButton.onClick.AddListener(() => ActivateGameProgressPanel(false));
            _gameProgressPanel.BackButton.onClick.AddListener(() => UpdateHasRewardsImage(_gameProgressScheme.HasUnreceivedRewards));
            _gameProgressPanel.BackButton.onClick.AddListener(() => OnButtonPressedMethod(_gameProgressPanel.BackButton));

            _gameProgressButton.onClick.AddListener(() => ActivateGameProgressPanel(true));
            _gameProgressButton.onClick.AddListener(_gameProgressPanel.OffsetContent);
            _gameProgressButton.onClick.AddListener(() => OnButtonPressedMethod(_gameProgressButton));

            _lootboxWindow.OkButton.onClick.AddListener(() => _lootboxWindow.SetActive(false));
            _lootboxWindow.OkButton.onClick.AddListener(() => _lootboxSlotsHandler.InitializeLootboxProgressPanel());
            _lootboxWindow.OkButton.onClick.AddListener(() => UpdatePodiumActivity(false));
            _lootboxWindow.OkButton.onClick.AddListener(() => OnButtonPressedMethod(_lootboxWindow.OkButton));

            _settingsButton.onClick.AddListener(() => ActivateSettingsPopup(true));
            _settingsButton.onClick.AddListener(() => OnButtonPressedMethod(_settingsButton));

            _settingsPopup.OkButton.onClick.AddListener(() => ActivateSettingsPopup(false));
            _settingsPopup.OkButton.onClick.AddListener(() => OnButtonPressedMethod(_settingsPopup.OkButton));

            _settingsPopup.ClosePopupWindowButton.onClick.AddListener(() => ActivateSettingsPopup(false));
            _settingsPopup.ClosePopupWindowButton.onClick.AddListener(() => OnButtonPressedMethod(_settingsPopup.ClosePopupWindowButton));

            _shopPanel.GetFreeLootboxButton.onClick.AddListener(() => _lootboxSlotsHandler.OpenFreeLootboxPopup());
        }

        private void OnDestroy()
        {
            _carsCollectionPanel.OnUseCar -= ChangeCar;
            _carsCollectionPanel.OnCarWindowOpen -= InitializeCarWindow;
            _carsCollectionPanel.OnButtonPressed -= OnButtonPressedMethod;

            _rewardsHandler.OnProgressReward -= UpdateCurrencyAmountPanels;
            _rewardsHandler.OnLootboxOpen -= ActivateLootboxWindow;
            _rewardsHandler.OnLootboxOpen -= _lootboxWindow.RepresentLootbox;
            _rewardsHandler.OnLootboxOpen -= (List<CarCardReward> list) => UpdateCurrencyAmountPanels();

            _gameProgressPanel.OnButtonPressed -= OnButtonPressedMethod;

            _lootboxSlotsHandler.OnPopupIsActive -= UpdatePodiumActivity;
            _lootboxSlotsHandler.OnButtonPressed -= OnButtonPressedMethod;
        }

        #endregion

        #region Legacy

        //private GraphicRaycaster _graphicRaycaster;
        //private PointerEventData _clickData;
        //private List<RaycastResult> _raycastResults;

        //private void Start
        //{
        //    _graphicRaycaster = GetComponent<GraphicRaycaster>();
        //    _clickData = new PointerEventData(EventSystem.current);
        //    _raycastResults = new List<RaycastResult>();
        //}

        //private void Update()
        //{
        //    if (Input.GetMouseButtonDown(0))
        //    {
        //        GetUIElementClicked();
        //    }
        //}

        //private void GetUIElementClicked()
        //{
        //    if (_inMainMenu)
        //        return;

        //    _clickData.position = Input.mousePosition;
        //    _raycastResults.Clear();
        //    _graphicRaycaster.Raycast(_clickData, _raycastResults);
        //    bool backPanelClicked = false;
        //    bool otherElementClicked = false;

        //    foreach (var element in _raycastResults)
        //    { 
        //        if(element.gameObject.CompareTag(Tag.BackPanel))
        //            backPanelClicked = true;
        //        else
        //            otherElementClicked = true;
        //    }

        //    if (backPanelClicked && !otherElementClicked)
        //    {
        //        ActivateTuningPanel(false);
        //        ActivateCarsCollectionPanel(false);
        //    }
        //}

        #endregion
    }
}

