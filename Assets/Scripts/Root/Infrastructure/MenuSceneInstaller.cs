using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using RaceManager.Cameras;
using RaceManager.Cars;
using RaceManager.Cars.Effects;
using RaceManager.Root;
using RaceManager.Shed;
using RaceManager.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Infrastructure
{
    public class MenuSceneInstaller : BaseInstaller
    {
        [SerializeField] private MainUI _mainUI;
        [SerializeField] private Podium _podium;
        [SerializeField] private CarsDepot _playerCarDepot;

        private MainSceneRoot _mainSceneRoot;

        public override void InstallBindings()
        {
            Bind(Singleton<Resolver>.Instance);
            Bind(Singleton<MenuCamerasHandler>.Instance);
            Bind(Singleton<ShedInitializer>.Instance);

            Bind(_mainUI);
            Bind(_podium);
            Bind(_playerCarDepot);

            Bind<SaveManager>();
            Bind<PlayerProfile>();
            Bind<CarTuner>();
        }

        public override void Start()
        {
            base.Start();

            _mainSceneRoot ??= Singleton<MainSceneRoot>.Instance;
            _mainSceneRoot.Run();
        }
    }
}
