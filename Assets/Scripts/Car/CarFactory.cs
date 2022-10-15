using RaceManager.Root;
using RaceManager.Tools;
using RaceManager.Waypoints;
using UnityEngine;

namespace RaceManager.Cars
{
    public class CarFactory
    {
        private readonly DriverType _driverType;
        private readonly CarSettings _carSettings;
        private readonly CarsDepot _carsDepot;
        private readonly WaypointTrack _waypointTrack;
        private readonly Transform _spawnPoint;

        public CarFactory(DriverType driverType, CarSettings carSettings, CarsDepot carsDepot, WaypointTrack waypointTrack, Transform spawnPoint)
        {
            _driverType = driverType;
            _carSettings = carSettings;
            _carsDepot = carsDepot;
            _waypointTrack = waypointTrack;
            _spawnPoint = spawnPoint;
        }

        public GameObject InitCar(out CarController carController, out CarAIControl carAIControl, out WaypointProgressTracker waypointTracker, out DriverProfile driverProfile)
        {
            string requiredName = _carSettings.CarProfile.Name;
            var prefab = _carsDepot.Cars.Find(x => x.Name == requiredName).Prefab;

            var go = Object.Instantiate(prefab, _spawnPoint.position, _spawnPoint.rotation);
            go.tag = _driverType.ToString();
            

            Car car = go.GetComponent<Car>();
            for (int i = 0; i < car.WheelColliders.Length; i++)
            {
                car.WheelColliders[i].enabled = true;
            }

            driverProfile = new DriverProfile();

            carController = go.GetComponent<CarController>();
            carController.Initialize(_carSettings);

<<<<<<< Updated upstream
            carAIControl = go.GetComponent<CarAIControl>();
            carAIControl.Initialize(_carSettings);
=======
            carAI = go.GetComponent<CarAI>();
>>>>>>> Stashed changes

            waypointTracker = go.GetComponent<WaypointProgressTracker>();
            waypointTracker.Initialize(_waypointTrack, driverProfile);

            if (_driverType == DriverType.Player)
            {
<<<<<<< Updated upstream
                var playerContrrol = go.AddComponent<CarPlayerControl>();
                playerContrrol.Initialize(carAIControl, _carSettings);
=======
                var playerContrrol = go.AddComponent<PlayerControl>();
                go.AddComponent<AudioListener>();
>>>>>>> Stashed changes
            }

            return go;
        }
    }
}


