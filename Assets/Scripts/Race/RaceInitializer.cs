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
        [SerializeField] private DriverSettings _playerDriverSettings;
        [SerializeField] private CarSettings _playerCarSettings;
        [Space]
        [SerializeField] private DriverSettings _opponentDriverSettings;
        [SerializeField] private CarSettings _opponentCarSettings;
        [Space]
        [SerializeField] private RaceUI _raceUI;
        [SerializeField] private CinemachineVirtualCamera _followCam;
        [SerializeField] private RaceLevelView _level;
        private WaypointTrack _waypointTrack;
        private StartPoint[] _startPoints;
        private List<Driver> _driversList;

        private bool _raceStarted;

        public List<Driver> Drivers => _driversList;

        private void Awake()
        {
            _startPoints = _level.StartPoints;
            _waypointTrack = _level.WaypointTrack;
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
                    driver.Initialize(_startPoints[i].Type, _playerCarSettings, _playerDriverSettings, _carsDepot, _waypointTrack);
                    _followCam.LookAt = driver.CarObject.transform;
                    _followCam.Follow = driver.CarObject.transform;
                    driver.Subscribe(_raceUI);
                }
                else
                {
                    driver.Initialize(_startPoints[i].Type, _opponentCarSettings, _opponentDriverSettings, _carsDepot, _waypointTrack);
                    driverGo.name += $"_{i + 1}";
                }

                driver.SetPositionInRace(i + 1);
                driverGo.transform.SetParent(parent.transform, false);
                _driversList.Add(driver);
            }
        }
    }
}