using RaceManager.Cameras;
using RaceManager.Cars;
using RaceManager.Effects;
using RaceManager.Progress;
using RaceManager.Root;
using RaceManager.Shed;
using RaceManager.Shop;
using RaceManager.UI;
using UnityEngine;

namespace RaceManager.Infrastructure
{
    public class MenuSceneInstaller : BaseInstaller
    {
        [SerializeField] private MainUI _mainUI;
        [SerializeField] private PodiumView _podium;
        [SerializeField] private MenuCamerasHandler _menuCamerasHandler;
        [SerializeField] private ShedHandler _shedHandler;
        [SerializeField] private ShopHandler _shopHandler;
        [SerializeField] private ShopCore _iapCore;
        [SerializeField] private MenuSceneHandler _menuHandler;
        [SerializeField] private GameFlagsHandler _gameFlagsHandler;
        [SerializeField] private TutorialSteps _tutorialSteps;
        [SerializeField] private GameRemindHandler _gameProgressReminder;
        [Space]
        [SerializeField] private CarsDepot _playerCarDepot;
        [Space]
        [SerializeField] private SpritesContainerCarCollection _spritesContainerCars;
        [SerializeField] private SpritesContainerRewards _spritesContainerRewards;
        [Space]
        [SerializeField] private GameProgressScheme _gameProgressScheme;
        [SerializeField] private RaceRewardsScheme _raceRewardsScheme;
        [SerializeField] private CarsUpgradeScheme _carUpgradeScheme;
        [SerializeField] private ShopScheme _shopScheme;
        [Space]
        [SerializeField] private GameSettingsContainer _effectsSettings;

        private MainSceneRoot _mainSceneRoot;

        public override void InstallBindings()
        {
            BindObjects();
            BindSingletons();
            BindClasses();
        }

        private void BindSingletons()
        {
            Bind(Singleton<Resolver>.Instance);
            Bind(Singleton<UIAnimator>.Instance);
        }

        private void BindObjects()
        {
            Bind(_iapCore);
            Bind(_effectsSettings);
            Bind(_podium);
            Bind(_playerCarDepot);
            Bind(_spritesContainerCars);
            Bind(_spritesContainerRewards);
            Bind(_gameProgressScheme);
            Bind(_raceRewardsScheme);
            Bind(_carUpgradeScheme);
            Bind(_mainUI);
            Bind(_menuCamerasHandler);
            Bind(_shedHandler);
            Bind(_shopHandler);
            Bind(_menuHandler);
            Bind(_gameFlagsHandler);
            Bind(_tutorialSteps);
            Bind(_gameProgressReminder);
            Bind(_shopScheme);
        }

        private void BindClasses()
        {
            Bind<SaveManager>();
            Bind<GameEvents>();
            Bind<PlayerProfile>();
            Bind<CarTuner>();
            Bind<Profiler>();
            Bind<RewardsHandler>();
            Bind<CarUpgradesHandler>();
            Bind<ProgressConditionInfo>();
        }

        public override void Start()
        {
            base.Start();

            _mainSceneRoot ??= Singleton<MainSceneRoot>.Instance;
            _mainSceneRoot.Run();
        }
    }
}
