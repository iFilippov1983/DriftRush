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
using RaceManager.Infrastructure;

namespace RaceManager.Race
{
    public class RaceInitializer : MonoBehaviour, Root.IInitializable
    {
        [SerializeField] private MaterialsContainer _materialsContainer;
        [SerializeField] private CarsDepot _opponentsCarsDepot;

        private CarsDepot _playerCarsDepot;
        private PlayerProfile _playerProfile;
        private RaceUI _raceUI;
        private RaceCamerasHandler _camerasHandler; 
        private InRacePositionsHandler _positionsHandler;
        private RaceLevelInitializer _raceLevelInitializer;
        private RaceLevelView _level;

        private WaypointTrack _waypointTrackMain;
        private WaypointTrack _waypointTrackEven;
        private WaypointTrack _waypointTrackOdd;
        private StartPoint[] _startPoints;
        private List<Driver> _driversList;
        private List<WaypointsTracker> _waypointsTrackersList;

        private bool _raceStarted;

        public List<Driver> Drivers => _driversList;

        [Inject]
        private void Construct(RaceLevelInitializer levelInitializer, InRacePositionsHandler positionsHandler, RaceUI raceUI, CarsDepot playerCarsDepot, PlayerProfile playerProfile)
        {
            _camerasHandler = Singleton<RaceCamerasHandler>.Instance;
            
            _playerCarsDepot = playerCarsDepot;
            _playerProfile = playerProfile;
            _positionsHandler = positionsHandler;
            _raceUI = raceUI;
            _raceLevelInitializer = levelInitializer;
            //_level = levelInitializer.RaceLevel;

            //_startPoints = _level.StartPoints;
            //_waypointTrackMain = _level.WaypointTrackMain;
            //_waypointTrackEven = _level.WaypointTrackEven;
            //_waypointTrackOdd = _level.WaypointTrackOdd;
        }

        public void Initialize()
        {
            _level = _raceLevelInitializer.RaceLevel;

            _startPoints = _level.StartPoints;
            _waypointTrackMain = _level.WaypointTrackMain;
            _waypointTrackEven = _level.WaypointTrackEven;
            _waypointTrackOdd = _level.WaypointTrackOdd;

            InitCameras();
            InitDrivers();

            _positionsHandler.StartHandling(_driversList, _waypointsTrackersList);
        }

        private void Start()
        {
            //InitCameras();
            //InitDrivers();

            //_positionsHandler.StartHandling(_driversList, _waypointsTrackersList);
        }

        private void Update()
        {
            if (_raceStarted)
                return;

            _raceStarted = true;
            EventsHub<RaceEvent>.BroadcastNotification(RaceEvent.COUNTDOWN);
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
                var driverGo = Instantiate(driverPrefab, _startPoints[i].transform.position, _startPoints[i].transform.rotation);
                driverGo.name = $"{_startPoints[i].Type} driver";

                var driver = driverGo.GetComponent<Driver>();
                if (_startPoints[i].Type == DriverType.Player)
                {
                    driver.Initialize(_startPoints[i].Type, _playerCarsDepot, _waypointTrackMain, _materialsContainer, _playerProfile);

                    _camerasHandler.FollowAndLookAt(driver.CarObject.transform, driver.CarTargetToFollow);

                    driver.Subscribe(_raceUI);

                    var selfRighting = driver.CarObject.GetComponent<CarSelfRighting>();
                    var tracker = driver.CarObject.GetComponent<WaypointsTracker>();

                    void GetToCheckpoint() 
                    {
                        selfRighting.GetToCheckpoint();
                        tracker.ResetTargetToCashedValues();
                    }

                    _raceUI.Init(driver.Profile, () => selfRighting.RightCar(), GetToCheckpoint);
                }
                else
                {
                    WaypointTrack track = (i % 2) == 0 ? _waypointTrackEven : _waypointTrackOdd;
                    driver.Initialize(_startPoints[i].Type, _opponentsCarsDepot, track, _materialsContainer);
                    driverGo.name += $"_{i + 1}";
                }

                //driver.SetPositionInRace(i + 1);
                driverGo.transform.SetParent(parent.transform, false);
                _driversList.Add(driver);
                _waypointsTrackersList.Add(driver.WaypointsTracker);
            }
        }
    }
}