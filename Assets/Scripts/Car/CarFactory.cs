using RaceManager.Tools;
using RaceManager.Waypoints;
using UnityEngine;

namespace RaceManager.Cars
{
    public class CarFactory
    {
        private readonly DriverType _driverType;
        private readonly CarSettings _carSettings;
        private readonly DriverSettings _driverSettings;
        private readonly CarsDepot _carsDepot;
        private readonly WaypointTrack _waypointTrack;
        private readonly Transform _spawnPoint;

        public CarFactory(DriverType driverType, CarSettings carSettings, DriverSettings driverSettings, CarsDepot carsDepot, WaypointTrack waypointTrack, Transform spawnPoint)
        {
            _driverType = driverType;
            _carSettings = carSettings;
            _driverSettings = driverSettings;
            _carsDepot = carsDepot;
            _waypointTrack = waypointTrack;
            _spawnPoint = spawnPoint;
        }

        public GameObject InitCar(out CarController carController, out CarAIControl carAIControl, out WaypointProgressTracker waypointTracker)
        {
            string requiredName = _driverSettings.CurrentCarName;
            var prefab = _carsDepot.Cars.Find(x => x.Name == requiredName).Prefab;

            var go = Object.Instantiate(prefab, _spawnPoint.position, _spawnPoint.rotation);
            go.tag = _driverType.ToString();

            Car car = go.GetComponent<Car>();
            for (int i = 0; i < car.WheelColliders.Length; i++)
            {
                car.WheelColliders[i].enabled = true;
            }

            carController = go.GetComponent<CarController>();
            carController.Initialize(_carSettings);

            carAIControl = go.GetComponent<CarAIControl>();
            carAIControl.Initialize(_driverSettings);

            waypointTracker = go.GetComponent<WaypointProgressTracker>();
            waypointTracker.Initialize(_waypointTrack);

            if (_driverType == DriverType.Player)
            {
                var playerContrrol = go.AddComponent<CarPlayerControl>();
                playerContrrol.Initialize(carController, carAIControl);
            }

            return go;
        }
    }
}


