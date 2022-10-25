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
using Sirenix.OdinInspector;
using RaceManager.Cameras;
using Zenject;

namespace RaceManager.Race
{
    public class RaceInitializer : MonoBehaviour
    {
        [SerializeField] private CarsDepot _carsDepot;
        [Space]
        [SerializeField] private CarProfile _playerCarProfile;
        [Space]
        [SerializeField] private List<CarProfile> _opponentsCarProfiles;
        private RaceUI _raceUI;
        private RaceCamerasHandler _camerasHandler;
        private InRacePositionsHandler _positionsHandler;
        private RaceLevelView _level;
        
        private CarProfile _opponentCarProfile;
        private WaypointTrack _waypointTrackMain;
        private WaypointTrack _waypointTrackEven;
        private WaypointTrack _waypointTrackOdd;
        private StartPoint[] _startPoints;
        private List<Driver> _driversList;
        private List<WaypointsTracker> _waypointsTrackersList;

        private bool _raceStarted;

        public List<Driver> Drivers => _driversList;

        [Inject]
        private void Construct(RaceLevelInitializer levelInitializer, InRacePositionsHandler positionsHandler, RaceUI raceUI)
        {
            _camerasHandler = Singleton<RaceCamerasHandler>.Instance;
            _positionsHandler = positionsHandler;
            _raceUI = raceUI;
            _level = levelInitializer.RaceLevel;

            _startPoints = _level.StartPoints;
            _waypointTrackMain = _level.WaypointTrackMain;
            _waypointTrackEven = _level.WaypointTrackEven;
            _waypointTrackOdd = _level.WaypointTrackOdd;
        }

        private void Start()
        {
            InitCameras();
            InitDrivers();

            _positionsHandler.StartHandling(_driversList, _waypointsTrackersList);
        }

        private void Update()
        {
            if (_raceStarted)
                return;

            _raceStarted = true;
            RaceEventsHub.BroadcastNotification(RaceEventType.COUNTDOWN);
        }

        private void InitCameras()
        {
            _camerasHandler.FollowCam.position = _level.FollowCamInitialPosition;
            _camerasHandler.StartCam.position = _level.StartCamInitialPosition;
            _camerasHandler.FinishCam.position = _level.FinishCamInitialPosition;
        }

        private void InitDrivers()
        {
            _raceStarted = false;
            _driversList = new List<Driver>();
            _waypointsTrackersList = new List<WaypointsTracker>();

            GameObject driverPrefab = ResourcesLoader.LoadPrefab(ResourcePath.DriverPrefab);
            GameObject parent = new GameObject("[Drivers]");

            for (int i = 0; i < _startPoints.Length; i++)
            {
                var driverGo = Instantiate(driverPrefab, _startPoints[i].transform.position, _startPoints[i].transform.rotation);//, parent.transform);
                driverGo.name = $"{_startPoints[i].Type} driver";

                var driver = driverGo.GetComponent<Driver>();
                if (_startPoints[i].Type == DriverType.Player)
                {
                    driver.Initialize(_startPoints[i].Type, _playerCarProfile, _carsDepot, _waypointTrackMain);

                    _camerasHandler.FollowAndLookAt(driver.CarObject.transform, driver.CarTargetToFollow);

                    driver.Subscribe(_raceUI);

                    _raceUI.Init(driver.Profile, () => driver.CarObject.GetComponent<CarSelfRighting>().RightCar());
                }
                else
                {
                    WaypointTrack track = (i % 2) == 0 ? _waypointTrackEven : _waypointTrackOdd;
                    _opponentCarProfile = GetOpponentsProfile();
                    driver.Initialize(_startPoints[i].Type, _opponentCarProfile, _carsDepot, track);
                    driverGo.name += $"_{i + 1}";
                }

                //driver.SetPositionInRace(i + 1);
                driverGo.transform.SetParent(parent.transform, false);
                _driversList.Add(driver);
                _waypointsTrackersList.Add(driver.WaypointsTracker);
            }
        }

        private CarProfile GetOpponentsProfile()
        {
            //TODO: make settings generation depending on Player's progress level

            return _opponentsCarProfiles[UnityEngine.Random.Range(0, _opponentsCarProfiles.Count)];
        }
    }
}