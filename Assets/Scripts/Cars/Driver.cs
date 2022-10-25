using RaceManager.Race;
using RaceManager.Waypoints;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace RaceManager.Cars
{
    /// <summary>
    /// Initializes a car and handles it
    /// </summary>
    public class Driver : MonoBehaviour, IObservable<DriverProfile>
    {
        public DriverType DriverType;

        private DriverProfile _profile;
        private GameObject _carObject;
        private CarAI _carAI;
        private Car _car;
        private CarVisual _carVisual;
        private WaypointsTracker _waypointsTracker;
        private List<IObserver<DriverProfile>> _observersList;

        public DriverProfile Profile => _profile;
        public GameObject CarObject => _carObject;
        public Transform CarTargetToFollow => _carAI.Target;
        public WaypointsTracker WaypointsTracker => _waypointsTracker;

        public void Initialize(DriverType type, CarProfile carProfile, CarsDepot carsDepot, WaypointTrack waypointTrack)
        {
            DriverType = type;
            CarFactory carFactory = new CarFactory(type, carProfile, carsDepot, waypointTrack, transform);
            _carObject = carFactory.InitCar(out _car, out _carVisual, out _carAI, out _waypointsTracker, out _profile);

            _profile.CarState.Value = CarState.OnTrack;
            _profile.CarState.Subscribe(s => OnCarStateChange(s));

            _observersList = new List<IObserver<DriverProfile>>();
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

        //private void OnDestroy()
        //{
        //    _profile.CarState.UnSubscribeOnChange(OnCarStateChange);
        //}

        private void Update()
        {
            UpdateProfile();
        }

        private void UpdateProfile()
        {
            _profile.CarCurrentSpeed = _car.SpeedInDesiredUnits;
            _profile.TrackProgress = _waypointsTracker.Progress;
            _profile.PositionInRace = _waypointsTracker.CarPosition;
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
            _carAI.StartEngine();
            NotifyObservers();
        }

        private void StopRace()
        {
            _carAI.StopEngine();

            if (DriverType == DriverType.Player)
            {
                RaceEventsHub.Unsunscribe(RaceEventType.FINISH, StopRace);
                RaceEventsHub.BroadcastNotification(RaceEventType.FINISH);
            }

            $"{gameObject.name} FINISHED".Log(ConsoleLog.Color.Yellow);
        }

        private void NotifyObservers()
        {
            foreach (var o in _observersList)
                o.OnNext(_profile);
        }

        public IDisposable Subscribe(IObserver<DriverProfile> observer)
        {
            _observersList.Add(observer);
            return Disposable.Empty;
        }
    }
}