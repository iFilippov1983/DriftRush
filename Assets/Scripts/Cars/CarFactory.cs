using RaceManager.Effects;
using RaceManager.Waypoints;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Utilities.Editor;
using UnityEngine;
using Zenject;

namespace RaceManager.Cars
{
    public class CarFactory
    {
        private DriverType _driverType;
        private CarProfile _carProfile;
        private CarsDepot _carsDepot;
        private WaypointTrack _waypointTrack;
        private Transform _spawnPoint;
        private MaterialsContainer _materialsContainer;

        public CarFactory
            (
            DriverType driverType, 
            CarsDepot carsDepot, 
            WaypointTrack waypointTrack, 
            MaterialsContainer materialsContainer, 
            Transform spawnPoint
            )
        {
            _driverType = driverType;
            _carsDepot = carsDepot;
            _waypointTrack = waypointTrack;
            _materialsContainer = materialsContainer;
            _spawnPoint = spawnPoint;

            _carProfile = _driverType == DriverType.Player
                ? _carsDepot.CurrentCarProfile
                : GetRandomProfile();
        }

        public CarFactory(CarsDepot carsDepot, MaterialsContainer materialsContainer, Transform spawnPoint)
        {
            _carsDepot = carsDepot;
            _materialsContainer = materialsContainer;
            _spawnPoint = spawnPoint;

            _carProfile = _carsDepot.CurrentCarProfile;
        }

        public GameObject ConstructCarForRace
            (
            out Car car, 
            out CarVisual carVisual, 
            out CarAI carAI, 
            out WaypointsTracker waypointsTracker, 
            out DriverProfile driverProfile, 
            bool playSound)
        {
            var prefab = _carProfile.Prefab;

            var go = Object.Instantiate(prefab, _spawnPoint.position, _spawnPoint.rotation);
            go.tag = _driverType.ToString();

            driverProfile = new DriverProfile(_driverType);

            carVisual = go.GetComponent<CarVisual>();
            carVisual.Initialize(_carProfile.CarConfigVisual, _materialsContainer);

            car = go.GetComponent<Car>();
            car.Initialize(_carProfile.CarConfig);

            carAI = go.GetComponent<CarAI>();
            carAI.Initialize();

            waypointsTracker = go.GetComponent<WaypointsTracker>();
            waypointsTracker.Initialize(_waypointTrack, driverProfile);

            var carSfxController = go.GetComponent<CarSfxController>();
            carSfxController.enabled = playSound;
            if (playSound) carSfxController.Initialize();

            if (_driverType == DriverType.Player)
            {
                go.AddComponent<PlayerControl>();
                go.AddComponent<AudioListener>();
            }

            return go;
        }

        public GameObject ConstructCarForShed(out CarVisual carVisual)
        {
            var prefab = _carProfile.Prefab;

            var go = Object.Instantiate(prefab, _spawnPoint.position, _spawnPoint.rotation, _spawnPoint);

            carVisual = go.GetComponent<CarVisual>();
            carVisual.Initialize(_carProfile.CarConfigVisual, _materialsContainer);

            var car = go.GetComponent<Car>();
            for (int i = 0; i < car.Wheels.Length; i++)
                car.Wheels[i].enabled = false;
            car.enabled = false;

            var carAI = go.GetComponent<CarAI>();
            carAI.enabled = false;

            var waypointsTracker = go.GetComponent<WaypointsTracker>();
            waypointsTracker.enabled = false;

            var carSelfRighting = go.GetComponent<CarSelfRighting>();
            carSelfRighting.enabled = false;

            var bodyTilt = go.GetComponent<BodyTilt>();
            bodyTilt.enabled = false;

            var carSfxController = go.GetComponent<CarSfxController>();
            carSfxController.enabled = false;

            var carVfxController = go.GetComponent<CarVfxController>();
            carVfxController.enabled = false;

            return go;
        }

        private CarProfile GetRandomProfile()
        {
            CarProfile carProfile = _carsDepot.ProfilesList[Random.Range(0, _carsDepot.ProfilesList.Count)];
            CarProfile newCarProfile = new CarProfile();

            FastDeepCopier.DeepCopyFromToClass(carProfile, newCarProfile);

            //carProfile.CarConfigVisual.CurrentMaterialsSetType = (MaterialSetType)Random.Range(0, 2);

            return newCarProfile;
        }
    }
}


