using RaceManager.Cameras;
using RaceManager.Cars;
using RaceManager.Effects;
using RaceManager.Progress;
using RaceManager.Race;
using RaceManager.Root;
using RaceManager.UI;
using UnityEngine;

namespace RaceManager.Infrastructure
{
    public class RaceSceneInstaller : BaseInstaller
    {
        [SerializeField] private RaceUI _raceUI;
        [SerializeField] private RaceHandler _raceHandler;
        [SerializeField] private RaceSceneEffectsHandler _effectsHandler;
        [Space]
        [SerializeField] private CarsDepot _playerCarDepot;
        [Space]
        [SerializeField] private GameProgressScheme _gameProgressScheme;
        [SerializeField] private RaceRewardsScheme _raceRewardsScheme;
        [Space]
        [SerializeField] private SpritesContainerRewards _spritesContainerRewards;
        [SerializeField] private EffectsSettingsContainer _settingsContainer;

        private RaceSceneRoot _raceSceneRoot;

        public override void InstallBindings()
        {
            BindSingletons();
            BindObjects();
            BindClasses();
        }

        private void BindSingletons()
        {
            Bind(Singleton<Resolver>.Instance);
            Bind(Singleton<RaceCamerasHandler>.Instance);
            Bind(Singleton<EffectsController>.Instance);
            Bind(Singleton<CarFXController>.Instance);
        }

        private void BindObjects()
        {
            Bind(_raceHandler);
            Bind(_settingsContainer);
            Bind(_raceUI);
            Bind(_playerCarDepot);
            Bind(_gameProgressScheme);
            Bind(_raceRewardsScheme);
            Bind(_spritesContainerRewards);
            Bind(_effectsHandler);
        }

        private void BindClasses()
        {
            Bind<SaveManager>();
            Bind<PlayerProfile>();
            Bind<InRacePositionsHandler>();
            Bind<RaceLevelInitializer>();
            Bind<Profiler>();
            Bind<RewardsHandler>();
        }

        public override void Start()
        {
            base.Start();

            _raceSceneRoot ??= Singleton<RaceSceneRoot>.Instance;
            _raceSceneRoot.Run();
        }
    }
}
