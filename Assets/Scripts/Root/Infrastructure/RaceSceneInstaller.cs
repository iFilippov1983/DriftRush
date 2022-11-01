﻿using RaceManager.Cameras;
using RaceManager.Cars;
using RaceManager.Cars.Effects;
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

        private RaceSceneRoot _raceSceneRoot;

        public override void InstallBindings()
        {
            Bind(Singleton<Resolver>.Instance);
            Bind(Singleton<RaceCamerasHandler>.Instance);
            Bind(Singleton<CarFXController>.Instance);

            Bind(_raceUI);
            Bind(_playerCarDepot);

            Bind<SaveManager>();
            Bind<PlayerProfile>();
            Bind<InRacePositionsHandler>();
            Bind<RaceLevelInitializer>();
        }

        public override void Start()
        {
            base.Start();

            _raceSceneRoot ??= Singleton<RaceSceneRoot>.Instance;
            _raceSceneRoot.Run();
        }
    }
}