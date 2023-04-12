using RaceManager.Cars;
using RaceManager.Progress;
using RaceManager.Root;
using RaceManager.Shed;
using RaceManager.Tools;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using IInitializable = RaceManager.Root.IInitializable;

namespace RaceManager.UI
{
    public class MainUI : MonoBehaviour, IInitializable
    {
        [SerializeField] private StartButtonView _startButton;
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
        [SerializeField] private LootboxProgressPanel _lootboxProgressPanel;
        [SerializeField] private LootboxWindow _lootboxWindow;
        [Space]
        [SerializeField] private NotificationPopup _notificationPopup;
        [SerializeField] private SettingsPopup _settingsPopup;
        [SerializeField] private ShopPanel _shopPanel;
        [Space]
        [SerializeField] private Transform _animParent;
        [SerializeField] private HelperRectsView _helperRects;
        [Space]
        [SerializeField] private bool _showPerformance;
        [ShowIf("_showPerformance")]
        [SerializeField] private PerformacePanelView _fpsPanelView;
        [ShowIf("_showPerformance")]
        [SerializeField] private int _maxFpsToDisplay = 99;

        private PlayerProfile _playerProfile;
        private RewardsHandler _rewardsHandler;
        private CarUpgradesHandler _upgradesHandler;
        private SaveManager _saveManager;
        private CarsDepot _playerCarDepot;
        private CarProfile _currentCarProfile;
        private PodiumView _podium;
        private GameProgressScheme _gameProgressScheme;
        private PerformanceDisplayer _fpsDisplayer;
        [Title("Debug only")]
        [ShowInInspector]
        private NotificationsHandler _notificationsHandler;

        public MainUIStatus Status { get; private set; }

        public Action<CarName> OnCarProfileChange;

        /// <summary>
        /// string = Button name
        /// </summary>
        public Action<string> OnButtonPressed;
        public readonly Subject<MainUIStatus> OnStatusChange = new Subject<MainUIStatus>();

        #region Car tuning properties

        public IObservable<float> OnSpeedValueChange => _tuningPanel.SpeedSlider.onValueChanged.AsObservable();
        public IObservable<float> OnHandlingValueChange => _tuningPanel.HandlingSlider.onValueChanged.AsObservable();
        public IObservable<float> OnAccelerationValueChange => _tuningPanel.AccelerationSlider.onValueChanged.AsObservable();
        public IObservable<float> OnFrictionValueChange => _tuningPanel.FrictionSlider.onValueChanged.AsObservable();

        public IObserver<TuneData> OnCharValueLimit => Observer.Create((TuneData td) => SetTuningPanelValues(td));
        public Action<int> OnTuneValuesChange => (int v) => _tuningPanel.UpdateCurrentInfoValues(v);

        #endregion

        #region Settings change properties

        public IObservable<bool> OnSoundsSettingChange => _settingsPopup.SoundToggle.onValueChanged.AsObservable();
        public IObservable<bool> OnMusicSettingChange => _settingsPopup.MusicToggle.onValueChanged.AsObservable();
        public IObservable<bool> OnVibroSettingChange => _settingsPopup.VibrationToggle.onValueChanged.AsObservable();
        public IObservable<bool> OnRaceLineSettingsChange => _settingsPopup.RaceLineToggle.onValueChanged.AsObservable();

        public IObserver<SettingsData> OnSettingsInitialize => Observer.Create((SettingsData sd) => SetSettingsPopupValues(sd));

        #endregion

        #region Other properties

        public ShopPanel ShopPanel => _shopPanel;
        public CurrencyAmountPanel CurrencyPanel => _currencyAmount;
        private UIAnimator Animator => Singleton<UIAnimator>.Instance;

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
            PodiumView podium,
            NotificationsHandler notificationsHandler
            )
        {
            _playerProfile = playerProfile;
            _rewardsHandler = rewardsHandler;
            _upgradesHandler = upgradesHandler;
            _gameProgressScheme = gameProgressScheme;
            _saveManager = saveManager;
            _playerCarDepot = playerCarDepot;
            _podium = podium;
            _notificationsHandler = notificationsHandler;

            RegisterButtonsListeners();
        }

        public void Initialize()
        {
            _currentCarProfile = _playerCarDepot.CurrentCarProfile;

            InitializeCarsCollectionPanel();
            InitializeGameProgressPanel();
            InitializeLootboxSlotsHandler();
            InitializeUINotifications();
            InitializeFpsPanel();

            UpdateCurrencyAmountPanels();
            UpdateTuningPanelValues();

            OnStatusChange
                .Where(s => s != Status)
                .Subscribe(s =>
                {
                    ActivateStatus(s);
                });

            this.FixedUpdateAsObservable()
                .Subscribe(_ => 
                { 
                    _lootboxSlotsHandler.UpdateTimer(); 
                })
                .AddTo(this);
        }

        private void Awake()
        {
            Status = MainUIStatus.ActiveMainUI;
            ActivateMainMenu(true);
        }

        #endregion

        #region Activate Functions

        private void ActivateStatus(MainUIStatus newStatus)
        {
            Action disablePreviouse = Status switch
            {
                MainUIStatus.ActiveMainUI => () => ActivateMainMenu(false, newStatus),
                MainUIStatus.ActiveTuningPanel => () => ActivateTuningPanel(false),
                MainUIStatus.ActiveCarsCollection => () => ActivateCarsCollectionPanel(false),
                MainUIStatus.ActiveShop => () => ActivateShopPanel(false),
                MainUIStatus.ActiveGameProgress => () => ActivateGameProgressPanel(false),
                MainUIStatus.ActiveNotification => () => ActivateNotificationPopup(false),
                _ => null,
            };

            disablePreviouse?.Invoke();

            Action enableNew = newStatus switch
            {
                MainUIStatus.ActiveMainUI => () => ActivateMainMenu(true, newStatus),
                MainUIStatus.ActiveTuningPanel => () => ActivateTuningPanel(true),
                MainUIStatus.ActiveCarsCollection => () => ActivateCarsCollectionPanel(true),
                MainUIStatus.ActiveShop => () => ActivateShopPanel(true),
                MainUIStatus.ActiveGameProgress => () => ActivateGameProgressPanel(true),
                MainUIStatus.ActiveNotification => () => ActivateNotificationPopup(true),
                _ => null,
            };

            enableNew?.Invoke();

            Status = newStatus;
        }

        private void ActivateMainMenu(bool active, MainUIStatus newStatus = MainUIStatus.ActiveMainUI)
        {
            //Debug.Log($"Main UI Status: <b>{Status}</b> => New status: <b>{newStatus}</b>");
            Animator.ForceCompleteAnimation?.OnNext(_cupsProgress.Name);
            Animator.ForceCompleteAnimation?.OnNext(_lootboxProgressPanel.Name);
            Animator.ForceCompleteAnimation?.OnNext(_lootboxSlotsHandler.Name);
            Animator.ForceCompleteAnimation?.OnNext(_startButton.Name);

            Transform t;
            MainUIStatus status = active ? Status : newStatus;
            t = status switch
             {
                 MainUIStatus.ActiveMainUI => null,
                 MainUIStatus.ActiveTuningPanel => _helperRects.AppearLeftRect.transform,
                 MainUIStatus.ActiveCarsCollection => _helperRects.ApearRightRect.transform,
                 MainUIStatus.ActiveShop => _helperRects.AppearTopRect.transform,
                 MainUIStatus.ActiveGameProgress => _helperRects.AppearBottomRect.transform,
                 MainUIStatus.ActiveNotification => _helperRects.AppearBottomRect,
                 _ => null,
             };

            if (t != null)
            {
                if (active)
                {
                    Activate(t);
                }
                else
                {
                    Deactivate(t);
                }
            }

            _bottomPanel.MainMenuPressedImage.SetActive(active);

            void Activate(Transform appearTransform)
            {
                _cupsProgress.SetActive(true);
                Animator.AppearSubject(_cupsProgress, _helperRects.AppearTopRect.transform)?.AddTo(this);

                _lootboxProgressPanel.SetActive(true);
                Animator.AppearSubject(_lootboxProgressPanel, appearTransform)?.AddTo(this);

                _lootboxSlotsHandler.SetActive(true);
                Animator.AppearSubject(_lootboxSlotsHandler, appearTransform)?.AddTo(this);

                _startButton.SetActive(true);
                Animator.AppearSubject(_startButton, _helperRects.AppearBottomRect.transform, _notificationsHandler.NotifyIfNeeded)?.AddTo(this);
            }

            void Deactivate(Transform disappearTransform)
            {
                Animator.DisappearSubject(_cupsProgress, _helperRects.AppearTopRect.transform, true, true)?.AddTo(this);

                Animator.DisappearSubject(_lootboxProgressPanel, disappearTransform, true, true);

                Animator.DisappearSubject(_lootboxSlotsHandler, disappearTransform, true, true);

                Animator.DisappearSubject(_startButton, _helperRects.AppearBottomRect.transform, true, true);
            }
        }

        private void ActivateTuningPanel(bool active)
        {
            Animator.ForceCompleteAnimation?.OnNext(_tuningPanel.Name);

            if (active)
            {
                _tuningPanel.SetActive(active);
                Animator.AppearSubject(_tuningPanel).AddTo(this);
            }
            else
            {
                _tuningPanel.DeactivateAllPanels();
                Animator.DisappearSubject(_tuningPanel, null, true, false, () => _tuningPanel.SetActive(active)).AddTo(this);
            }

            _bottomPanel.TuningPressedImage.SetActive(active);
        }

        private void ActivateCarsCollectionPanel(bool active)
        {
            Animator.ForceCompleteAnimation?.OnNext(_carsCollectionPanel.Name);

            if (active)
            {
                _carsCollectionPanel.SetActive(active);
                Animator.AppearSubject(_carsCollectionPanel).AddTo(this);
            }
            else
            {
                Animator.DisappearSubject(_carsCollectionPanel, null, true, false, () => _carsCollectionPanel.SetActive(active)).AddTo(this);
            }

            _bottomPanel.CarsCollectionPressedImage.SetActive(active);
        }

        private void ActivateShopPanel(bool active)
        {
            Animator.ForceCompleteAnimation?.OnNext(_shopPanel.Name);

            if (active)
            {
                _shopPanel.SetActive(active);
                Animator.AppearSubject(_shopPanel, _helperRects.AppearBottomRect.transform);
            }
            else
            {
                Animator.DisappearSubject(_shopPanel, _helperRects.AppearBottomRect.transform, true, true);
            }

            _bottomPanel.IapShopPressedImage.SetActive(active);
        }

        private void ActivateGameProgressPanel(bool active)
        {
            _gameProgressPanel.SetActive(active);

            if (active)
            {
                //Status = MainUIStatus.ActiveGameProgress;
                Animator.AppearSubject(_gameProgressPanel);
            }
        }

        private void ActivateLootboxWindow(int moneyAmount, List<CarCardReward> list)
        {
            _lootboxWindow.SetActive(true);
            UpdatePodiumActivity(true);
        }

        private void ActivateCarWindow(bool active)
        {
            Animator.ForceCompleteAnimation?.OnNext(_carsCollectionPanel.CarWindow.name);
            Animator.ForceCompleteAnimation?.OnNext(_bottomPanel.name);

            if (active)
            {
                _carsCollectionPanel.CarWindow.SetActive(true);
                Animator.AppearSubject(_carsCollectionPanel.CarWindow, _helperRects.AppearBottomRect)?.AddTo(this);

                Animator.DisappearSubject(_bottomPanel, null, true, true)?.AddTo(this);
            }
            else if(!_bottomPanel.isActiveAndEnabled)
            {
                Animator.DisappearSubject(_carsCollectionPanel.CarWindow, _helperRects.AppearBottomRect, true, true)?.AddTo(this);

                _bottomPanel.SetActive(true);
                Animator.AppearSubject(_bottomPanel)?.AddTo(this);
            }
        }

        private void ActivateSettingsPopup(bool active)
        {
            _settingsPopup.SetActive(active);

            if(active)
                Animator.AppearSubject(_settingsPopup, _settingsButton.transform)?.AddTo(this);
        }

        private void ActivateNotificationPopup(CarName carName)
        {
            _notificationPopup.UpgradeCarButton.onClick.RemoveAllListeners();
            _notificationPopup.UpgradeCarButton.onClick.AddListener(() => _bottomPanel.CarsCollectionButton.onClick?.Invoke());
            _notificationPopup.UpgradeCarButton.onClick.AddListener(() => _carsCollectionPanel.OpenCarWindow(carName));

            OnStatusChange?.OnNext(MainUIStatus.ActiveNotification);
        }

        private void ActivateNotificationPopup(bool active)
        {
            _notificationPopup.SetActive(active);

            if (active)
                Animator.AppearSubject(_notificationPopup, _carsCollectionPanel.transform)?.AddTo(this);
        }

        #endregion

        #region Initialize Data Functions

        private void InitializeSlidersMinMaxValues()
        {
            var c = _currentCarProfile.CarCharacteristics;
            _tuningPanel.SetBorderValues(CharacteristicType.Speed, c.MinSpeedFactor, c.MaxSpeedFactor);
            _tuningPanel.SetBorderValues(CharacteristicType.Handling, c.MinHandlingFactor, c.MaxHandlingFactor);
            _tuningPanel.SetBorderValues(CharacteristicType.Acceleration, c.MinAccelerationFactor, c.MaxAccelerationFactor);
            _tuningPanel.SetBorderValues(CharacteristicType.Friction, c.MinFrictionFactor, c.MaxFrictionFactor);
        }

        private void InitializeCarsCollectionPanel()
        {
            foreach (var profile in _playerCarDepot.ProfilesList)
            {
                _carsCollectionPanel.AddCollectionCard
                    (
                    profile.CarName,
                    profile.CarCharacteristics.Rarity,
                    profile.CarCharacteristics.CurrentFactorsProgress,
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
            foreach (var stepData in _gameProgressScheme.Steps)
            {
                int goalCupsAmount = stepData.Key;
                ProgressStep step = stepData.Value;
                step.IsReached = _playerProfile.Cups >= goalCupsAmount;

                _gameProgressPanel.AddProgressStep(goalCupsAmount, step, () =>
                {
                    _rewardsHandler.RewardForProgress(goalCupsAmount);
                    UpdateCurrencyAmountPanels(step.Rewards, _gameProgressPanel.GetProgressStepView(goalCupsAmount).transform);
                });
            }

            _gameProgressPanel.Initialize(_playerProfile.Cups);

            var closestGlobalGoal = _gameProgressScheme.Steps.First(p => p.Value.BigPrefab && p.Value.IsReached == false || p.Value.IsLast);
            int globalCupsAmountGoal = closestGlobalGoal.Key > _playerProfile.Cups
                ? closestGlobalGoal.Key
                : _gameProgressScheme.LastGlobalGoal.Key;


            InitializeCupsProgressPanel(globalCupsAmountGoal);

            _rewardsHandler.OnLootboxOpen += ActivateLootboxWindow;
            _rewardsHandler.OnLootboxOpen += _lootboxWindow.RepresentLootbox;
            _rewardsHandler.OnLootboxOpen += (int moneyAmount, List<CarCardReward> list) => UpdateCurrencyAmountPanels(GameUnitType.Money);

            _rewardsHandler.OnCarCardsReward += _gameProgressPanel.RepresentCards;

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

            _lootboxSlotsHandler.OnInstantLootboxOpen += () => UpdateCurrencyAmountPanels(GameUnitType.Gems);
            _lootboxSlotsHandler.OnButtonPressed += OnButtonPressedMethod;
        }

        private void InitializeUINotifications()
        {
            _notificationsHandler.Initialize(_notificationPopup);
            _notificationsHandler.OnNotification += ActivateNotificationPopup;
        }

        private void InitializeFpsPanel()
        {
            _fpsPanelView.SetActive(_showPerformance);

            if (_showPerformance)
            {
                _fpsDisplayer = new PerformanceDisplayer(new PerformanceDisplayData() 
                { 
                    averageText = _fpsPanelView.AverageText,
                    highestText = _fpsPanelView.HighestText,
                    lowestText = _fpsPanelView.LowestText,
                    monoUsedText = _fpsPanelView.MonoUsedText,
                    monoHeapText = _fpsPanelView.MonoHeapText,
                    MaxFpsToDisplay = _maxFpsToDisplay,
                });

                this.UpdateAsObservable().Subscribe(_ => 
                {
                    _fpsDisplayer.Display(Time.unscaledDeltaTime);
                }).AddTo(this);
            }
        }

        #endregion

        #region Update Data Functions

        public void UpdateTuningPanelValues(bool addFactors = false)
        {
            _currentCarProfile = _playerCarDepot.CurrentCarProfile;

            InitializeSlidersMinMaxValues();

            var c = _currentCarProfile.CarCharacteristics;
            _tuningPanel.UpdateAllSlidersValues
                (
                c.CurrentSpeedFactor,
                c.CurrentHandlingFactor,
                c.CurrentAccelerationFactor,
                c.CurrentFrictionFactor,
                c.AvailableFactorsToUse,
                addFactors
                );

            _tuningPanel.UpdateCarStatsProgress
                (
                _currentCarProfile.CarName.ToString(),
                _currentCarProfile.CarCharacteristics.CurrentFactorsProgress
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
                _currentCarProfile.CarCharacteristics.CurrentFactorsProgress
                );
        }

        public void UpdateCarsCollectionCard(CarName carName)
        {
            CarProfile profile = _playerCarDepot.GetProfile(carName);

            _carsCollectionPanel.UpdateCard
                    (
                    profile.CarName,
                    profile.CarCharacteristics.CurrentFactorsProgress,
                    _playerProfile.CarCardsAmount(profile.CarName),
                    profile.RankingScheme.CurrentRankPointsForAccess,
                    profile.RankingScheme.CarIsAvailable,
                    profile.RankingScheme.AllRanksGranted
                    );

            if (profile.CarName == _currentCarProfile.CarName)
                _carsCollectionPanel.UsedCarName = profile.CarName;
        }

        public void UpdateCarWindow(CarProfile carProfile = null)
        {
            CarProfile profile = carProfile ?? _currentCarProfile;
            var rank = profile.RankingScheme.CurrentRank;
            int cost = rank.AccessCost;
            bool isUpgraded = rank.IsGranted;
            bool isAvailable = rank.IsReached;

            Rarity carRarity = profile.CarCharacteristics.Rarity;

            _carsCollectionPanel.SetCarWindow(carRarity, cost, isUpgraded, isAvailable);
        }

        public void UpdateCurrencyAmountPanels(GameUnitType type)
        {
            switch (type)
            {
                case GameUnitType.Money:
                case GameUnitType.Gems:
                    ScrambleCurrencyText(type);
                    break;
                default:
                    return;
            }
        }

        public void UpdateCurrencyAmountPanels(List<IReward> rewards = null, Transform callerTransform = null)
        {
            if (rewards == null)
            {
                _currencyAmount.MoneyAmount.text = _playerProfile.Money.ToString();
                _gameProgressPanel.MoneyAmountText.text = _playerProfile.Money.ToString();

                _currencyAmount.GemsAmount.text = _playerProfile.Gems.ToString();
                _gameProgressPanel.gemsAmountText.text = _playerProfile.Gems.ToString();
            }
            else
            {
                foreach (var reward in rewards) 
                {
                    Transform moveToTransform = reward.Type switch
                    {
                        GameUnitType.Money => _currencyAmount.MoneyImage.transform,
                        GameUnitType.Gems => _currencyAmount.GemsImage.transform,
                        _ => null,
                    };
                    Animator.SpawnGroupOnAndMoveTo(reward.Type, _animParent, callerTransform, moveToTransform, () => ScrambleCurrencyText(reward.Type)).AddTo(this);
                }
            }
        }

        private void ScrambleCurrencyText(GameUnitType type)
        {
            switch (type)
            {
                case GameUnitType.Money:
                    Animator.ScrambleNumeralsText(_currencyAmount.MoneyAmount, _playerProfile.Money.ToString())?.AddTo(this);
                    Animator.ScrambleNumeralsText(_gameProgressPanel.MoneyAmountText, _playerProfile.Money.ToString())?.AddTo(this);
                    break;
                case GameUnitType.Gems:
                    Animator.ScrambleNumeralsText(_currencyAmount.GemsAmount, _playerProfile.Gems.ToString())?.AddTo(this);
                    Animator.ScrambleNumeralsText(_gameProgressPanel.gemsAmountText, _playerProfile.Gems.ToString())?.AddTo(this);
                    break;
            }
        }

        public void UpdateGameProgressPanel()
        {
            _rewardsHandler.OnLootboxOpen -= ActivateLootboxWindow;
            _rewardsHandler.OnLootboxOpen -= _lootboxWindow.RepresentLootbox;
            _rewardsHandler.OnLootboxOpen -= (int moneyAmount, List<CarCardReward> list) => UpdateCurrencyAmountPanels(GameUnitType.Money);

            _rewardsHandler.OnCarCardsReward -= _gameProgressPanel.RepresentCards;

            _gameProgressPanel.OnButtonPressed -= OnButtonPressedMethod;

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
            _settingsPopup.SoundToggle.isOn = sd.playSounds;// ? 1f : 0f;
            _settingsPopup.MusicToggle.isOn = sd.playMusic;// ? 1f : 0f;
            _settingsPopup.VibrationToggle.isOn = sd.useHaptics;// ? 1f : 0f;
            _settingsPopup.RaceLineToggle.isOn = sd.useRaceLine;// ? 1f : 0f;
        }

        private void ChangeCar(CarName newCarName)
        {
            if (newCarName == _playerCarDepot.CurrentCarName)
                return;

            OnCarProfileChange?.Invoke(newCarName);
        }

        private void CarRankUpgrade()
        {
            if (_upgradesHandler.TryUpgradeCarRank())
            {
                UpdateCurrencyAmountPanels(GameUnitType.Money);
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
                UpdateCurrencyAmountPanels(GameUnitType.Money);
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
            _startButton.Button.onClick.AddListener(StartRace);
            _startButton.Button.onClick.AddListener(() => OnButtonPressedMethod(_startButton.Button));

            _bottomPanel.TuneButton.onClick.AddListener(() => OnStatusChange?.OnNext(MainUIStatus.ActiveTuningPanel));
            _bottomPanel.TuneButton.onClick.AddListener(() => OnButtonPressedMethod(_bottomPanel.TuneButton));

            _bottomPanel.MainMenuButton.onClick.AddListener(() => OnStatusChange?.OnNext(MainUIStatus.ActiveMainUI));
            _bottomPanel.MainMenuButton.onClick.AddListener(() => OnButtonPressedMethod(_bottomPanel.MainMenuButton));

            _bottomPanel.CarsCollectionButton.onClick.AddListener(() => OnStatusChange?.OnNext(MainUIStatus.ActiveCarsCollection));
            _bottomPanel.CarsCollectionButton.onClick.AddListener(() => OnButtonPressedMethod(_bottomPanel.CarsCollectionButton));

            _bottomPanel.ShopButton.onClick.AddListener(() => OnStatusChange?.OnNext(MainUIStatus.ActiveShop));
            _bottomPanel.ShopButton.onClick.AddListener(() => OnButtonPressedMethod(_bottomPanel.ShopButton));

            _tuningPanel.RegisterButtonsListeners();

            _tuningPanel.TuneStatsButton.onClick.AddListener(() => OnButtonPressedMethod(_tuningPanel.TuneStatsButton));
            _tuningPanel.TuneWeelsViewButton.onClick.AddListener(() => OnButtonPressedMethod(_tuningPanel.TuneWeelsViewButton));
            _tuningPanel.TuneCarViewButton.onClick.AddListener(() => OnButtonPressedMethod(_tuningPanel.TuneCarViewButton));

            _tuningPanel.ClosePanelButton.onClick.AddListener(() => OnStatusChange?.OnNext(MainUIStatus.ActiveMainUI));
            _tuningPanel.ClosePanelButton.onClick.AddListener(() => OnButtonPressedMethod(_tuningPanel.ClosePanelButton));

            //_tuningPanel.ClosePanelWindowButton.onClick.AddListener(() => OnStatusChange?.OnNext(MainUIStatus.ActiveMainUI));
            //_tuningPanel.ClosePanelWindowButton.onClick.AddListener(() => OnButtonPressedMethod(_tuningPanel.ClosePanelWindowButton));

            _tuningPanel.UpgradeButton.onClick.AddListener(CarFactorsUpgrade);
            _tuningPanel.UpgradeButton.onClick.AddListener(() => OnButtonPressedMethod(_tuningPanel.UpgradeButton));

            _carsCollectionPanel.CloseButton.onClick.AddListener(() => OnStatusChange?.OnNext(MainUIStatus.ActiveMainUI));
            _carsCollectionPanel.CloseButton.onClick.AddListener(() => OnButtonPressedMethod(_carsCollectionPanel.CloseButton));

            //_carsCollectionPanel.ClosePanelWindowButton.onClick.AddListener(() => OnStatusChange?.OnNext(MainUIStatus.ActiveMainUI));
            //_carsCollectionPanel.ClosePanelWindowButton.onClick.AddListener(() => ActivateCarWindow(false));
            //_carsCollectionPanel.ClosePanelWindowButton.onClick.AddListener(() => OnButtonPressedMethod(_carsCollectionPanel.ClosePanelWindowButton));

            _gameProgressPanel.BackButton.onClick.AddListener(() => OnStatusChange?.OnNext(MainUIStatus.ActiveMainUI));
            _gameProgressPanel.BackButton.onClick.AddListener(() => UpdateHasRewardsImage(_gameProgressScheme.HasUnreceivedRewards));
            _gameProgressPanel.BackButton.onClick.AddListener(() => OnButtonPressedMethod(_gameProgressPanel.BackButton));

            _gameProgressButton.onClick.AddListener(() => OnStatusChange?.OnNext(MainUIStatus.ActiveGameProgress));
            _gameProgressButton.onClick.AddListener(_gameProgressPanel.OffsetContent);
            _gameProgressButton.onClick.AddListener(() => OnButtonPressedMethod(_gameProgressButton));

            _lootboxWindow.OkButton.onClick.AddListener(() => _lootboxWindow.SetActive(false));
            _lootboxWindow.OkButton.onClick.AddListener(() => OnStatusChange?.OnNext(MainUIStatus.ActiveMainUI));
            _lootboxWindow.OkButton.onClick.AddListener(() => _lootboxSlotsHandler.InitializeLootboxProgressPanel());
            _lootboxWindow.OkButton.onClick.AddListener(() => UpdatePodiumActivity(false));
            _lootboxWindow.OkButton.onClick.AddListener(() => OnButtonPressedMethod(_lootboxWindow.OkButton));

            //_notificationPopup.OkButton.onClick.AddListener(() => ActivateNotificationPopup(false));
            _notificationPopup.OkButton.onClick.AddListener(() => OnStatusChange?.OnNext(MainUIStatus.ActiveMainUI));
            _notificationPopup.OkButton.onClick.AddListener(() => OnButtonPressedMethod(_notificationPopup.OkButton));

            //_notificationPopup.ClosePopupWindowButton.onClick.AddListener(() => ActivateNotificationPopup(false));
            _notificationPopup.ClosePopupWindowButton.onClick.AddListener(() => OnStatusChange?.OnNext(MainUIStatus.ActiveMainUI));
            _notificationPopup.ClosePopupWindowButton.onClick.AddListener(() => OnButtonPressedMethod(_notificationPopup.ClosePopupWindowButton));

            //_notificationPopup.OpenCollectionButton.onClick.AddListener(() => ActivateNotificationPopup(false));
            _notificationPopup.OpenCollectionButton.onClick.AddListener(() => _bottomPanel.CarsCollectionButton.onClick?.Invoke());

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

            _rewardsHandler.OnLootboxOpen -= ActivateLootboxWindow;
            _rewardsHandler.OnLootboxOpen -= _lootboxWindow.RepresentLootbox;
            _rewardsHandler.OnLootboxOpen -= (int moneyAmount, List<CarCardReward> list) => UpdateCurrencyAmountPanels(GameUnitType.Money);

            _rewardsHandler.OnCarCardsReward -= _gameProgressPanel.RepresentCards;

            _gameProgressPanel.OnButtonPressed -= OnButtonPressedMethod;

            //_lootboxSlotsHandler.OnPopupIsActive -= UpdatePodiumActivity;
            _lootboxSlotsHandler.OnInstantLootboxOpen -= () => UpdateCurrencyAmountPanels(GameUnitType.Gems);
            _lootboxSlotsHandler.OnButtonPressed -= OnButtonPressedMethod;

            _notificationsHandler.OnNotification -= ActivateNotificationPopup;
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

