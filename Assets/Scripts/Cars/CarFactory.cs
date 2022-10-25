using RaceManager.Cameras;
using RaceManager.Root;
using RaceManager.Tools;
using RaceManager.Waypoints;
using UnityEngine;

namespace RaceManager.Cars
{
    public class CarFactory
    {
        private readonly DriverType _driverType;
        private readonly CarProfile _carProfile;
        private readonly CarsDepot _carsDepot;
        private readonly WaypointTrack _waypointTrack;
        private readonly Transform _spawnPoint;

        public CarFactory(DriverType driverType, CarProfile carProfile, CarsDepot carsDepot, WaypointTrack waypointTrack, Transform spawnPoint)
        {
            _driverType = driverType;
            _carProfile = carProfile;
            _carsDepot = carsDepot;
            _waypointTrack = waypointTrack;
            _spawnPoint = spawnPoint;
        }

        public GameObject InitCar(out Car car, out CarVisual carVisual, out CarAI carAI, out WaypointsTracker waypointsTracker, out DriverProfile driverProfile)
        {
            var requiredName = _carProfile.CarName;
            var prefab = _carsDepot.Cars.Find(x => x.CarName == requiredName).Prefab;

            var go = Object.Instantiate(prefab, _spawnPoint.position, _spawnPoint.rotation);
            go.tag = _driverType.ToString();

            driverProfile = new DriverProfile(_driverType);

            carVisual = go.GetComponent<CarVisual>();
            carVisual.ApplyVisual(_carProfile.CarVisualContainer);

            car = go.GetComponent<Car>();
            car.Initialize(_carProfile.CarConfig);

            carAI = go.GetComponent<CarAI>();
            carAI.Initialize();

            waypointsTracker = go.GetComponent<WaypointsTracker>();
            waypointsTracker.Initialize(_waypointTrack, driverProfile);

            if (_driverType == DriverType.Player)
            {
                go.AddComponent<PlayerControl>();
                go.AddComponent<AudioListener>();
            }

            return go;
        }
    }
}


