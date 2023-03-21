using RaceManager.Progress;
using RaceManager.Race;
using RaceManager.Root;
using RaceManager.Waypoints;
using System;
using System.Collections;
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
        private const float StopSpeedThreshold = 1f;

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
        public Transform CameraFinalTarget => _car.CameraFinalTarget;
        public Transform CameraFinalPosition => _car.CameraFinalPosition;
        public Transform StartCameraTarget => _car.StartCameraTarget;
        public Transform StartCameraPosition => _car.StartCameraPosition;
        public WaypointsTracker WaypointsTracker => _waypointsTracker;
        public CarVisual CarVisual => _carVisual;

        public void Initialize
            (
            DriverType type, 
            CarsDepot carsDepot, 
            WaypointTrack waypointTrack, 
            MaterialsContainer materialsContainer,
            Profiler profiler = null,
            bool playSouds = true
            )
        {
            DriverType = type;
            _materialsContainer = materialsContainer;
            _profiler = profiler;

            CarFactory carFactory = new CarFactory(type, carsDepot, waypointTrack, _materialsContainer, transform);
            _carObject = carFactory.ConstructCarForRace(out _car, out _carVisual, out _carAI, out _waypointsTracker, out _driverProfile, playSouds);

            //_driverProfile.CarState.Value = CarState.OnTrack;
            _driverProfile.CarState.Subscribe(s => OnCarStateChange(s));

            _observersList = new List<IObserver<DriverProfile>>();
        }

        public void TrackRecommendedSpeed()
        {
            WaypointsTracker.OnRecommendedSpeedChange
                .Subscribe(s => 
                {
                    _carAI.DesiredSpeed = s < _car.SpeedInDesiredUnits
                    ?  s
                    : _car.CarConfig.MaxSpeed;
                })
                .AddTo(this);
        }

        private void OnEnable()
        {
            EventsHub<RaceEvent>.Subscribe(RaceEvent.START, StartRace);
        }

        private void OnDisable()
        {
            EventsHub<RaceEvent>.Unsunscribe(RaceEvent.START, StartRace);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private void Update()
        {
            UpdateProfile();
        }

        private void UpdateProfile()
        {
            if (_profiler != null)
            {
                _driverProfile.CarCurrentSpeed = _car.SpeedInDesiredUnits;
                _driverProfile.TrackProgress = _waypointsTracker.Progress;
                _driverProfile.DistanceFromStart = _waypointsTracker.DistanceFromStart;
                _driverProfile.PositionInRace = (PositionInRace)_waypointsTracker.CarPosition;
                NotifyObservers();
            }
        }

        private void OnCarStateChange(CarState carState)
        {
            switch (carState)
            {
                case CarState.None:
                    break;
                case CarState.CanStart:
                    SetToTrack();
                    break;
                case CarState.OnTrack:
                    break;
                case CarState.Finished:
                    StopRace();
                    break;
                case CarState.Stopped:
                    break;
            }
        }

        private void SetToTrack()
        {
            _driverProfile.CarState.Value = CarState.OnTrack;

            if (_profiler != null)
            {
                _profiler.SetImmediateStart();
            }
        }

        private void StartRace()
        {
            _carAI.StartDriving();
            NotifyObservers();
        }

        public void StopRace()
        {
            _carAI.StopEngine();

            if (_profiler != null)
            {
                _profiler.SetInRacePosition(_driverProfile.PositionInRace);
                EventsHub<RaceEvent>.BroadcastNotification(RaceEvent.FINISH);
            }

            StartCoroutine(WaitForCarStop());

            $"{gameObject.name} FINISHED".Log(Logger.ColorYellow);
        }

        private IEnumerator WaitForCarStop()
        {
            while (_car.CurrentSpeed > StopSpeedThreshold)
                yield return null;
            
            _driverProfile.CarState.Value = CarState.Stopped;
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