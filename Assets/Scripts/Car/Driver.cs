using RaceManager.Race;
using RaceManager.Waypoints;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace RaceManager.Cars
{
    [RequireComponent(typeof(CarController), typeof(CarAIControl), typeof(WaypointProgressTracker))]
    public class Driver : MonoBehaviour, IObservable<DriverProfile>
    {
        public DriverType DriverType;

        private DriverProfile _profile;
        private GameObject _carObject;
        private CarAIControl _carAIControl;
        private CarController _carController;
        private WaypointProgressTracker _waypointProgressTracker;
        private List<IObserver<DriverProfile>> _observerssList;

        public DriverProfile Profile => _profile;
        public GameObject CarObject => _carObject;

        public void Initialize(DriverType type, CarSettings carSettings , DriverSettings driverSettings, CarsDepot carsDepot, WaypointTrack waypointTrack)
        {
            DriverType = type;
            CarFactory carFactory = new CarFactory(type, carSettings, driverSettings, carsDepot, waypointTrack, transform);
            _carObject = carFactory.InitCar(out _carController, out _carAIControl, out _waypointProgressTracker);
            //transform.SetParent(_carObject.transform, false);

            _profile = new DriverProfile();
            _observerssList = new List<IObserver<DriverProfile>>();
        }

        private void OnEnable()
        {
            RaceEventsHub.Subscribe(RaceEventType.START, StartRace);
            RaceEventsHub.Subscribe(RaceEventType.FINISH, StopRace);
        }

        private void OnDisable()
        {
            RaceEventsHub.Unsunscribe(RaceEventType.START, StartRace);
            RaceEventsHub.Unsunscribe(RaceEventType.FINISH, StopRace);
        }

        private void Update()
        {
            UpdateProfile();
        }

        private void UpdateProfile()
        { 
            _profile.CarCurrentSpeed = _carController.CurrentSpeed;
            _profile.TrackProgress = _waypointProgressTracker.Progress;
            NotifyObservers();
        }


        private void StartRace()
        {
            _carAIControl.StartEngine();
        }

        private void StopRace()
        {
            _carAIControl.StopEngine();
        }

        private void NotifyObservers()
        {
            foreach (var o in _observerssList)
                o.OnNext(_profile);
        }

        public IDisposable Subscribe(IObserver<DriverProfile> observer)
        {
            _observerssList.Add(observer);
            return Disposable.Empty;
        }
    }
}


