using RaceManager.Race;
using RaceManager.Waypoints;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;

namespace RaceManager.Cars
{
    public class Driver : MonoBehaviour, IObservable<DriverProfile>
    {
        public DriverType DriverType;

        private DriverProfile _profile;
        private GameObject _carObject;
        private CarAI _carAI;
        private Car _car;
        private WaypointsTracker _waypointsTracker;
        private List<IObserver<DriverProfile>> _observersList;

        public DriverProfile Profile => _profile;
        public GameObject CarObject => _carObject;
        public Transform TargetToFollow => _carAI.Target;

        public void Initialize(DriverType type, CarConfig carConfig, CarsDepot carsDepot, WaypointTrack waypointTrack)
        {
            DriverType = type;
            CarFactory carFactory = new CarFactory(type, carConfig, carsDepot, waypointTrack, transform);
            _carObject = carFactory.InitCar(out _car, out _carAI, out _waypointsTracker, out _profile);
            //transform.SetParent(_carObject.transform, false);

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

            $"{gameObject.name} FINISHED".Log(StringConsoleLog.Color.Yellow);
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