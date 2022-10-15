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
        private CarAIControl _carAIControl;
        private CarController _carController;
        private WaypointProgressTracker _waypointProgressTracker;
        private List<IObserver<DriverProfile>> _observersList;

        public DriverProfile Profile => _profile;
        public GameObject CarObject => _carObject;
<<<<<<< Updated upstream
        public Transform TargetToFollow => _carAIControl.Target;
=======
        //public Transform TargetToFollow => _carAI.Target;
>>>>>>> Stashed changes

        public void Initialize(DriverType type, CarSettings carSettings, CarsDepot carsDepot, WaypointTrack waypointTrack)
        {
            DriverType = type;
            CarFactory carFactory = new CarFactory(type, carSettings, carsDepot, waypointTrack, transform);
            _carObject = carFactory.InitCar(out _carController, out _carAIControl, out _waypointProgressTracker, out _profile);
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
<<<<<<< Updated upstream
            _profile.CarCurrentSpeed = _carController.VelocityMagnitude;
            _profile.TrackProgress = _waypointProgressTracker.Progress;
            _profile.PositionInRace = _waypointProgressTracker.CarPosition;
=======
            //_profile.CarCurrentSpeed = _car.VelocityMagnitude;
            _profile.TrackProgress = _waypointsTracker.Progress;
            _profile.PositionInRace = _waypointsTracker.CarPosition;
>>>>>>> Stashed changes
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