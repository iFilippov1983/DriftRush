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
using RaceManager.Progress;
using UniRx;
using RaceManager.Effects;

namespace RaceManager.Race
{
    public class RaceHandler : MonoBehaviour, Root.IInitializable
    {
        [SerializeField] private MaterialsContainer _materialsContainer;
        [SerializeField] private RaceRewardsScheme _rewardsScheme;
        [SerializeField] private CarsDepot _opponentsCarsDepot;

        private CarsDepot _playerCarsDepot;
        private EffectsSettingsContainer _settingsContainer;
        private PlayerProfile _playerProfile;
        private Profiler _profiler;
        private RaceUI _raceUI;
        private RaceCamerasHandler _camerasHandler; 
        private InRacePositionsHandler _positionsHandler;
        private RaceLevelInitializer _raceLevelInitializer;
        private IRaceLevel _raceLevel;
        private RewardsHandler _rewardsHandler;

        private WaypointTrack _waypointTrackMain;
        private WaypointTrack _waypointTrackEven;
        private WaypointTrack _waypointTrackOdd;
        private StartPoint[] _startPoints;
        private List<Driver> _driversList;
        private List<WaypointsTracker> _waypointsTrackersList;

        private bool _raceStarted;

        public List<Driver> Drivers => _driversList;

        [Inject]
        private void Construct
            (
            RaceLevelInitializer levelInitializer, 
            RewardsHandler rewardsHandler, 
            InRacePositionsHandler positionsHandler, 
            RaceUI raceUI, 
            CarsDepot playerCarsDepot, 
            EffectsSettingsContainer settingsContainer,
            PlayerProfile playerProfile,
            Profiler profiler
            )
        {
            _camerasHandler = Singleton<RaceCamerasHandler>.Instance;
            
            _playerCarsDepot = playerCarsDepot;
            _settingsContainer = settingsContainer;
            _playerProfile = playerProfile;
            _profiler = profiler;
            _positionsHandler = positionsHandler;
            _raceUI = raceUI;
            _raceLevelInitializer = levelInitializer;
            _rewardsHandler = rewardsHandler;
        }

        public void Initialize()
        {
            _raceLevel = _raceLevelInitializer.GetRaceLevel();

            _startPoints = _raceLevel.StartPoints;
            _waypointTrackMain = _raceLevel.WaypointTrackMain;
            _waypointTrackEven = _raceLevel.WaypointTrackEven;
            _waypointTrackOdd = _raceLevel.WaypointTrackOdd;

            InitCameras();
            InitDrivers();

            _positionsHandler.StartHandling(_waypointsTrackersList);

            _rewardsHandler.OnRaceRewardLootboxAdded += NotifyRaceUI;
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
            _camerasHandler.FollowCam.position = _raceLevel.FollowCamInitialPosition;
            _camerasHandler.StartCam.position = _raceLevel.StartCamInitialPosition;
            _camerasHandler.FinishCam.position = _raceLevel.FinishCamInitialPosition;
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
                    driver.Initialize
                        (
                        _startPoints[i].Type, 
                        _playerCarsDepot, 
                        _waypointTrackMain, 
                        _materialsContainer, 
                        _settingsContainer.PlaySounds, 
                        _profiler
                        );

                    _camerasHandler.FollowAndLookAt(driver.CarCameraFollowTarget, driver.CarCameraLookTarget);

                    driver.Subscribe(_raceUI);
                    driver.DriverProfile.CarState.Subscribe(cs => HandlePlayerCarState(cs, driver));

                    var selfRighting = driver.CarObject.GetComponent<CarSelfRighting>();
                    var tracker = driver.CarObject.GetComponent<WaypointsTracker>();

                    void GetToCheckpoint() 
                    {
                        selfRighting.GetToCheckpoint();
                        tracker.ResetTargetToCashedValues();
                    }

                    _raceUI.Initialize(_raceLevelInitializer, selfRighting.RightCar, GetToCheckpoint);
                }
                else
                {
                    WaypointTrack track = (i % 2) == 0 ? _waypointTrackEven : _waypointTrackOdd;

                    driver.Initialize
                        (
                        _startPoints[i].Type, 
                        _opponentsCarsDepot, 
                        track, 
                        _materialsContainer, 
                        _settingsContainer.PlaySounds
                        );

                    driverGo.name += $"_{i + 1}";
                }

                driverGo.transform.SetParent(parent.transform, false);
                _driversList.Add(driver);
                _waypointsTrackersList.Add(driver.WaypointsTracker);
            }
        }

        private IDisposable HandlePlayerCarState(CarState playerCarState, Driver playerDriver)
        {
            switch (playerCarState)
            {
                case CarState.Finished:
                    _rewardsHandler.RewardForRace(playerDriver.DriverProfile.PositionInRace, out RaceRewardInfo info);
                    _raceUI.SetFinishValues(info.RewardMoneyAmount, info.RewardCupsAmount, info.MoneyTotal, info.GemsTotal);
                    break;
                case CarState.InShed:
                    break;
                case CarState.OnTrack:
                    break;
                case CarState.Stuck:
                    break;
                case CarState.GotHit:
                    break;
            }

            _raceUI.ChangeViewDependingOn(playerCarState);

            return Disposable.Empty;
        }

        private void NotifyRaceUI(Lootbox lootbox) => _raceUI.SetLootboxPopupValues(lootbox.Rarity);

        private void OnDestroy()
        {
            _rewardsHandler.OnRaceRewardLootboxAdded -= NotifyRaceUI;
        }

        public struct RaceRewardInfo
        {
            public int RewardMoneyAmount;
            public int RewardCupsAmount;
            public int MoneyTotal;
            public int GemsTotal;
        }
    }
}