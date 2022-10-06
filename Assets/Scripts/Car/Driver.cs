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
            _carObject = carFactory.InitCar(out _carController, out _carAIControl, out _waypointProgressTracker, out _profile);
            //transform.SetParent(_carObject.transform, false);

            _profile.CarState.Value = CarState.OnTrack;
            _profile.CarState.SubscribeOnChange(OnCarStateChange);

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

        private void OnDestroy()
        {
            _profile.CarState.UnSubscribeOnChange(OnCarStateChange);
        }

        private void Update()
        {
            UpdateProfile();
        }

        private void UpdateProfile()
        { 
            _profile.CarCurrentSpeed = _carController.CurrentSpeed;
            _profile.TrackProgress = _waypointProgressTracker.Progress;
            _profile.PositionInRace = _waypointProgressTracker.CarPosition;
            NotifyObservers();
        }

        private void OnCarStateChange(CarState carState)
        {
            switch (carState)
            {
                case CarState.InShed:
                    break;
                case CarState.OnTrack:
                    break;
                case CarState.Stuck:
                    break;
                case CarState.Finished:
                    StopRace();
                    break;
                case CarState.GotHit:
                    break;
            }
        }

        private void StartRace()
        {
            _carAIControl.StartEngine();
            NotifyObservers();
        }

        private void StopRace()
        {
            _carAIControl.StopEngine();

            if (DriverType == DriverType.Player)
            {
                RaceEventsHub.Unsunscribe(RaceEventType.FINISH, StopRace);
                RaceEventsHub.BroadcastNotification(RaceEventType.FINISH);
            }
                
            $"{gameObject.name} FINISHED".Log(StringConsoleLog.Color.Yellow);
        }

        private void NotifyObservers()
        {
            foreach (var o in _observerssList)
                o.OnNext(_profile);
        }

        public void SetPositionInRace(int position) => _profile.PositionInRace = position;

        public IDisposable Subscribe(IObserver<DriverProfile> observer)
        {
            _observerssList.Add(observer);
            return Disposable.Empty;
        }
    }
}