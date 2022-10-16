using RaceManager.Root;
using RaceManager.Tools;
using RaceManager.Waypoints;
using UnityEngine;

namespace RaceManager.Cars
{
    public class CarFactory
    {
        private readonly DriverType _driverType;
        private readonly CarConfig _carConfig;
        private readonly CarsDepot _carsDepot;
        private readonly WaypointTrack _waypointTrack;
        private readonly Transform _spawnPoint;

        public CarFactory(DriverType driverType, CarConfig carConfig, CarsDepot carsDepot, WaypointTrack waypointTrack, Transform spawnPoint)
        {
            _driverType = driverType;
            _carConfig = carConfig;
            _carsDepot = carsDepot;
            _waypointTrack = waypointTrack;
            _spawnPoint = spawnPoint;
        }

        public GameObject InitCar(out Car car, out CarAI carAI, out WaypointsTracker waypointsTracker, out DriverProfile driverProfile)
        {
            string requiredName = _carConfig.CarProfile.Name;
            var prefab = _carsDepot.Cars.Find(x => x.Name == requiredName).Prefab;

            var go = Object.Instantiate(prefab, _spawnPoint.position, _spawnPoint.rotation);
            go.tag = _driverType.ToString();

            driverProfile = new DriverProfile();

            car = go.GetComponent<Car>();
            car.Initialize(_carConfig);

            carAI = go.GetComponent<CarAI>();

            waypointsTracker = go.GetComponent<WaypointsTracker>();
            waypointsTracker.Initialize(_waypointTrack, driverProfile);

            if (_driverType == DriverType.Player)
            {
                var playerContrrol = go.AddComponent<PlayerControl>();
                go.AddComponent<AudioListener>();
            }

            return go;
        }
    }
}


