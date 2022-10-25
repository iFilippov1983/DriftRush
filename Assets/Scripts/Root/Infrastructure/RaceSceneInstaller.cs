using RaceManager.Cameras;
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

        private RaceSceneRoot _raceSceneRoot;

        public override void InstallBindings()
        {
            Bind(Singleton<ResolverService>.Instance);
            Bind(Singleton<RaceCamerasHandler>.Instance);
            Bind(Singleton<CarFXController>.Instance);

            Bind(_raceUI);

            Bind<SaveManager>();
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
