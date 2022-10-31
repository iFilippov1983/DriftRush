using Google.Protobuf.WellKnownTypes;
using RaceManager.Alt;
using RaceManager.Cameras;
using RaceManager.Cars.Effects;
using RaceManager.Root;
using RaceManager.Tools;
using RaceManager.Waypoints;
using System;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace RaceManager.Cars
{
    public class CarFactory
    {
        private readonly DriverType _driverType;
        private readonly CarProfile _carProfile;
        private readonly CarsDepot _carsDepot;
        private readonly WaypointTrack _waypointTrack;
        private readonly Transform _spawnPoint;
        private readonly MaterialsContainer _materialsContainer;

        public CarFactory(DriverType driverType, CarsDepot carsDepot, WaypointTrack waypointTrack, MaterialsContainer materialsContainer, Transform spawnPoint)
        {
            _driverType = driverType;
            _carsDepot = carsDepot;
            _waypointTrack = waypointTrack;
            _materialsContainer = materialsContainer;
            _spawnPoint = spawnPoint;

            _carProfile = _driverType == DriverType.Player
                ? _carsDepot.CurrentCarProfile
                : GetOpponentsProfile();
        }

        public CarFactory(CarsDepot carsDepot, MaterialsContainer materialsContainer, Transform spawnPoint)
        {
            _carsDepot = carsDepot;
            _materialsContainer = materialsContainer;
            _spawnPoint = spawnPoint;

            _carProfile = _carsDepot.CurrentCarProfile;
        }

        public GameObject ConstructCarForRace(out Car car, out CarVisual carVisual, out CarAI carAI, out WaypointsTracker waypointsTracker, out DriverProfile driverProfile)
        {
            var prefab = _carProfile.Prefab;

            var go = Object.Instantiate(prefab, _spawnPoint.position, _spawnPoint.rotation);
            go.tag = _driverType.ToString();

            driverProfile = new DriverProfile(_driverType);

            carVisual = go.GetComponent<CarVisual>();
            carVisual.CarVisualContainer = _carProfile.CarVisualContainer;
            carVisual.CarVisualContainer.SetMaterialsContainer(_materialsContainer);
            carVisual.ApplyVisual();
            

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

        public GameObject ConstructCarForShed(out CarVisual carVisual)
        {
            var prefab = _carProfile.Prefab;

            var go = Object.Instantiate(prefab, _spawnPoint.position, _spawnPoint.rotation, _spawnPoint);

            carVisual = go.GetComponent<CarVisual>();
            carVisual.CarVisualContainer = _carProfile.CarVisualContainer;
            carVisual.CarVisualContainer.SetMaterialsContainer(_materialsContainer);
            carVisual.ApplyVisual();

            var car = go.GetComponent<Car>();
            car.EffectsChild.SetActive(false);
            car.enabled = false;

            var carAI = go.GetComponent<CarAI>();
            carAI.enabled = false;

            var waypointsTracker = go.GetComponent<WaypointsTracker>();
            waypointsTracker.enabled = false;

            var carSelfRighting = go.GetComponent<CarSelfRighting>();
            carSelfRighting.enabled = false;

            var bodyTilt = go.GetComponent<BodyTilt>();
            bodyTilt.enabled = false;

            var carSoundController = go.GetComponent<CarSoundController>();
            carSoundController.enabled = false;

            go.AddComponent<AudioListener>();

            return go;
        }

        private CarProfile GetOpponentsProfile()
        {
            //TODO: make settings generation depending on Player's progress level
            
            CarProfile carProfile = _carsDepot.Cars[Random.Range(0, _carsDepot.Cars.Count)];
            carProfile.CarVisualContainer.CurrentMaterialsSetType = (PartsSetType)Random.Range(0, 2);


            return carProfile;
        }
    }
}


