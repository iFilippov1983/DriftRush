using RaceManager.Progress;
using RaceManager.Race;
using RaceManager.Root;
using RaceManager.Waypoints;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace RaceManager.Cars
{
    /// <summary>
    /// Handles a specific car
    /// </summary>
    public class Driver : MonoBehaviour, IObservable<DriverProfile>
    {
        public DriverType DriverType;

        private Profiler _profiler;
        private DriverProfile _driverProfile;
        private GameObject _carObject;
        private CarAI _carAI;
        private Car _car;
        private CarVisual _carVisual;
        private WaypointsTracker _waypointsTracker;
        private MaterialsContainer _materialsContainer;

        private List<IObserver<DriverProfile>> _observersList;

        public DriverProfile DriverProfile => _driverProfile;
        public GameObject CarObject => _carObject;
        public Car Car => _car;
        public Transform CarCameraLookTarget => _car.CameraLookTarget;
        public Transform CarCameraFollowTarget => _car.CameraFollowTarget;
        public WaypointsTracker WaypointsTracker => _waypointsTracker;

        public void Initialize
            (
            DriverType type, 
            CarsDepot carsDepot, 
            WaypointTrack waypointTrack, 
            MaterialsContainer materialsContainer,
            bool playSouds = true,
            Profiler profiler = null
            )
        {
            DriverType = type;
            _materialsContainer = materialsContainer;
            _profiler = profiler;

            CarFactory carFactory = new CarFactory(type, carsDepot, waypointTrack, _materialsContainer, transform);
            _carObject = carFactory.ConstructCarForRace(out _car, out _carVisual, out _carAI, out _waypointsTracker, out _driverProfile, playSouds);

            _driverProfile.CarState.Value = CarState.OnTrack;
            _driverProfile.CarState.Subscribe(s => OnCarStateChange(s));

            _observersList = new List<IObserver<DriverProfile>>();
        }

        private void OnEnable()
        {
            EventsHub<RaceEvent>.Subscribe(RaceEvent.START, StartRace);
        }

        private void OnDisable()
        {
            EventsHub<RaceEvent>.Unsunscribe(RaceEvent.START, StartRace);
        }

        private void Update()
        {
            UpdateProfile();
        }

        private void UpdateProfile()
        {
            _driverProfile.CarCurrentSpeed = _car.SpeedInDesiredUnits;
            _driverProfile.TrackProgress = _waypointsTracker.Progress;
            _driverProfile.DistanceFromStart = _waypointsTracker.DistanceFromStart;
            _driverProfile.PositionInRace = (PositionInRace)_waypointsTracker.CarPosition;
            NotifyObservers();
        }

        private void OnCarStateChange(CarState carState)
        {
            switch (carState)
            {
                case CarState.OnTrack:
                    break;
                case CarState.Finished:
                    StopRace();
                    break;
            }
        }

        private void StartRace()
        {
            _carAI.StartEngine();
            NotifyObservers();
        }

        public void StopRace()
        {
            _carAI.StopEngine();

            if (DriverType == DriverType.Player)
            {
                _profiler.SetInRacePosition(_driverProfile.PositionInRace);
                EventsHub<RaceEvent>.BroadcastNotification(RaceEvent.FINISH);
            }

            $"{gameObject.name} FINISHED".Log(Logger.ColorYellow);
        }

        private void NotifyObservers()
        {
            foreach (var o in _observersList)
                o.OnNext(_driverProfile);
        }

        public IDisposable Subscribe(IObserver<DriverProfile> observer)
        {
            _observersList.Add(observer);
            return Disposable.Empty;
        }
    }
}