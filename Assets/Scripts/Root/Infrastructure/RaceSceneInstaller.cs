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
        [SerializeField] private RaceSceneHandler _effectsHandler;
        [SerializeField] private GameFlagsHandler _gameFlagsHandler;
        [SerializeField] private TutorialSteps _tutorialSteps;
        [Space]
        [SerializeField] private CarsDepot _playerCarDepot;
        [Space]
        [SerializeField] private GameProgressScheme _gameProgressScheme;
        [SerializeField] private RaceRewardsScheme _raceRewardsScheme;
        [SerializeField] private OpponentsTuneScheme _opponentsTuneScheme;
        [Space]
        [SerializeField] private SpritesContainerRewards _spritesContainerRewards;
        [SerializeField] private GameSettingsContainer _settingsContainer;

        private RaceSceneRoot _raceSceneRoot;

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
            Bind(_raceHandler);
            Bind(_settingsContainer);
            Bind(_raceUI);
            Bind(_playerCarDepot);
            Bind(_gameProgressScheme);
            Bind(_raceRewardsScheme);
            Bind(_opponentsTuneScheme);
            Bind(_spritesContainerRewards);
            Bind(_effectsHandler);
            Bind(_gameFlagsHandler);
            Bind(_tutorialSteps);
        }

        private void BindClasses()
        {
            Bind<SaveManager>();
            Bind<GameEvents>();
            Bind<PlayerProfile>();
            Bind<InRacePositionsHandler>();
            Bind<Profiler>();
            Bind<RewardsHandler>();
            Bind<OpponentsCarTuner>();
        }

        public override void Start()
        {
            base.Start();

            _raceSceneRoot ??= Singleton<RaceSceneRoot>.Instance;
            _raceSceneRoot.Run();
        }
    }
}
