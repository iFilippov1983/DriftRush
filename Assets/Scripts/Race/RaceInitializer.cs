using Cinemachine;
using RaceManager.Root;
using RaceManager.Tools;
using RaceManager.Cars;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RaceManager.Waypoints;
using RaceManager.UI;

namespace RaceManager.Race
{
    public class RaceInitializer : Singleton<RaceInitializer>
    {
        [SerializeField] private CarsDepot _carsDepot;
        [Space]
        [SerializeField] private CarConfigScriptable _playerCarConfigSO;
        [Space]
        [SerializeField] private CarConfigScriptable _opponentCarConfigSO;
        [Space]
        [SerializeField] private RaceUI _raceUI;
        [SerializeField] private CinemachineVirtualCamera _followCam;
        [SerializeField] private RaceLevelView _level;
        private WaypointTrack _waypointTrackMain;
        private WaypointTrack _waypointTrackEven;
        private WaypointTrack _waypointTrackOdd;
        private StartPoint[] _startPoints;
        private List<Driver> _driversList;

        private bool _raceStarted;

        public List<Driver> Drivers => _driversList;

        protected override void AwakeSingleton()
        {
            _startPoints = _level.StartPoints;
            _waypointTrackMain = _level.WaypointTrackMain;
            _waypointTrackEven = _level.WaypointTrackEven;
            _waypointTrackOdd = _level.WaypointTrackOdd;
        }

        private void Start()
        {
            InitDrivers();
            RaceHandler.Instance.StartHandle();
        }

        private void Update()
        {
            if (_raceStarted)
                return;

            _raceStarted = true;
            RaceEventsHub.BroadcastNotification(RaceEventType.COUNTDOWN);
        }

        private void InitDrivers()
        {
            _driversList = new List<Driver>();
            GameObject driverPrefab = ResourcesLoader.LoadPrefab(ResourcePath.DriverPrefab);
            GameObject parent = new GameObject("[Drivers]");

            for (int i = 0; i < _startPoints.Length; i++)
            {
                var driverGo = Instantiate(driverPrefab, _startPoints[i].transform.position, _startPoints[i].transform.rotation);//, parent.transform);
                driverGo.name = $"{_startPoints[i].Type} driver";

                var driver = driverGo.GetComponent<Driver>();
                if (_startPoints[i].Type == DriverType.Player)
                {
                    driver.Initialize(_startPoints[i].Type, _playerCarConfigSO.CarConfig, _carsDepot, _waypointTrackMain);
                    _followCam.LookAt = driver.CarObject.transform;
                    _followCam.Follow = driver.CarObject.transform;
                    //_followCam.LookAt = driver.TargetToFollow;
                    //_followCam.Follow = driver.TargetToFollow;
                    driver.Subscribe(_raceUI);
                    _raceUI.Init(driver.Profile, () => driver.CarObject.GetComponent<CarSelfRighting>().RightCar());
                }
                else
                {
                    WaypointTrack track = (i % 2) == 0 ? _waypointTrackEven : _waypointTrackOdd;
                    driver.Initialize(_startPoints[i].Type, _opponentCarConfigSO.CarConfig, _carsDepot, track);
                    driverGo.name += $"_{i + 1}";
                }

                //driver.SetPositionInRace(i + 1);
                driverGo.transform.SetParent(parent.transform, false);
                _driversList.Add(driver);
            }
        }
    }
}