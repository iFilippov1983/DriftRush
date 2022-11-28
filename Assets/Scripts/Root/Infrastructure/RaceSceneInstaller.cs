﻿using RaceManager.Cameras;
using RaceManager.Cars;
using RaceManager.Cars.Effects;
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
        [SerializeField] private CarsDepot _playerCarDepot;
        [Space]
        [SerializeField] private GameProgressScheme _gameProgressScheme;
        [SerializeField] private RaceRewardsScheme _raceRewardsScheme;
        [Space]
        [SerializeField] private SpritesContainerRewards _spritesContainerRewards;

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
            Bind(Singleton<CarFXController>.Instance);
            Bind(Singleton<RaceHandler>.Instance);
            Bind(Singleton<RewardsHandler>.Instance);
        }

        private void BindObjects()
        {
            Bind(_raceUI);
            Bind(_playerCarDepot);
            Bind(_gameProgressScheme);
            Bind(_raceRewardsScheme);
            Bind(_spritesContainerRewards);
        }

        private void BindClasses()
        {
            Bind<SaveManager>();
            Bind<PlayerProfile>();
            Bind<InRacePositionsHandler>();
            Bind<RaceLevelInitializer>();
            Bind<Profiler>();
        }

        public override void Start()
        {
            base.Start();

            _raceSceneRoot ??= Singleton<RaceSceneRoot>.Instance;
            _raceSceneRoot.Run();
        }
    }
}
