using RaceManager.Cameras;
using RaceManager.Cars;
using RaceManager.Progress;
using RaceManager.Root;
using RaceManager.Shed;
using RaceManager.UI;
using UnityEngine;

namespace RaceManager.Infrastructure
{
    public class MenuSceneInstaller : BaseInstaller
    {
        [SerializeField] private MainUI _mainUI;
        [SerializeField] private PodiumView _podium;
        [Space]
        [SerializeField] private CarsDepot _playerCarDepot;
        [Space]
        [SerializeField] private SpritesContainerCarCollection _spritesContainerCars;
        [SerializeField] private SpritesContainerCarCards _spritesContainerCards;
        [SerializeField] private SpritesContainerRewards _spritesContainerRewards;
        [Space]
        [SerializeField] private GameProgressScheme _gameProgressScheme;
        [SerializeField] private RaceRewardsScheme _raceRewardsScheme;

        private MainSceneRoot _mainSceneRoot;

        public override void InstallBindings()
        {
            BindSingletons();
            BindObjects();
            BindClasses();
        }

        private void BindSingletons()
        {
            Bind(Singleton<Resolver>.Instance);
            Bind(Singleton<MenuCamerasHandler>.Instance);
            Bind(Singleton<ShedHandler>.Instance);
            Bind(Singleton<RewardsHandler>.Instance);
        }

        private void BindObjects()
        {
            Bind(_mainUI);
            Bind(_podium);
            Bind(_playerCarDepot);
            Bind(_spritesContainerCars);
            Bind(_spritesContainerCards);
            Bind(_spritesContainerRewards);
            Bind(_gameProgressScheme);
            Bind(_raceRewardsScheme);
        }

        private void BindClasses()
        {
            Bind<SaveManager>();
            Bind<PlayerProfile>();
            Bind<CarTuner>();
            Bind<Profiler>();
        }

        public override void Start()
        {
            base.Start();

            _mainSceneRoot ??= Singleton<MainSceneRoot>.Instance;
            _mainSceneRoot.Run();
        }
    }
}
