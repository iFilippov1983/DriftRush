using RaceManager.Cameras;
using RaceManager.Cars.Effects;
using RaceManager.Race;
using RaceManager.UI;
using UnityEngine;

namespace RaceManager.Infrastructure
{
    public class RaceSceneInstaller : BaseInstaller
    {
        [SerializeField] private RaceUI _raceUI;

        public override void InstallBindings()
        {
            Bind(_raceUI);

            Bind(RaceCamerasHandler.Instance);
            Bind(CarFXController.Instance);

            Bind<InRacePositionsHandler>();
            Bind<RaceLevelInitializer>();
        }
    }
}
