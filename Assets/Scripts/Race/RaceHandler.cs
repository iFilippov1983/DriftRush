using RaceManager.Root;
using RaceManager.Tools;
using RaceManager.Cars;
using System;
using System.Collections.Generic;
using UnityEngine;
using RaceManager.Waypoints;
using RaceManager.UI;
using Zenject;
using RaceManager.Progress;
using UniRx;
using RaceManager.Effects;
using IInitializable = RaceManager.Root.IInitializable;

namespace RaceManager.Race
{
    public class RaceHandler : MonoBehaviour, IInitializable
    {
        [SerializeField] private MaterialsContainer _materialsContainer;
        [SerializeField] private RaceRewardsScheme _rewardsScheme;
        [SerializeField] private CarsDepot _opponentsCarsDepot;

        private CarsDepot _playerCarsDepot;
        private GameSettingsContainer _settingsContainer;
        private RaceSceneHandler _sceneHandler;
        private Profiler _profiler;
        private RaceUI _raceUI;
        private InRacePositionsHandler _positionsHandler;
        private InRaceLootboxHandler _lootboxHandler;
        private RaceLevelInitializer _raceLevelInitializer;
        private RaceLineHandler _lineHandler;
        private IRaceLevel _raceLevel;
        private RewardsHandler _rewardsHandler;
        private GameEvents _gameEvents;

        private WaypointTrack _waypointTrackMain;
        private WaypointTrack _waypointTrackEven;
        private WaypointTrack _waypointTrackOdd;
        private StartPoint[] _startPoints;
        private List<WaypointsTracker> _waypointsTrackersList;

        private bool _raceStarted;

        private bool CanStartImmediate => _profiler.CanStartImmediate;

        [Inject]
        private void Construct
            (
            RaceLevelInitializer levelInitializer, 
            RewardsHandler rewardsHandler, 
            InRacePositionsHandler positionsHandler, 
            RaceUI raceUI, 
            CarsDepot playerCarsDepot, 
            GameSettingsContainer settingsContainer,
            RaceSceneHandler sceneHandler,
            Profiler profiler,
            GameEvents gameEvents
            )
        {
            _playerCarsDepot = playerCarsDepot;
            _settingsContainer = settingsContainer;
            _sceneHandler = sceneHandler;
            _profiler = profiler;
            _positionsHandler = positionsHandler;
            _raceUI = raceUI;
            _raceLevelInitializer = levelInitializer;
            _rewardsHandler = rewardsHandler;
            _gameEvents = gameEvents;
        }

        public void Initialize()
        {
            _raceLevel = _raceLevelInitializer.GetRaceLevel();

            _startPoints = _raceLevel.StartPoints;
            _waypointTrackMain = _raceLevel.WaypointTrackMain;
            _waypointTrackEven = _raceLevel.WaypointTrackEven;
            _waypointTrackOdd = _raceLevel.WaypointTrackOdd;

            _lineHandler = new RaceLineHandler(_raceLevel.WaypointTrackMain, _raceLevel.RaceLine, _settingsContainer.UseRaceLine);
            _lootboxHandler = new InRaceLootboxHandler(_profiler);

            InitDrivers();

            _positionsHandler.StartHandling(_waypointsTrackersList);

            _rewardsHandler.OnRaceRewardLootboxAdded += NotifyRaceUI;
        }

        private void Update()
        {
            if (_raceStarted || !CanStartImmediate)
                return;

            _raceStarted = true;

            EventsHub<RaceEvent>.BroadcastNotification(RaceEvent.COUNTDOWN);
        }

        private void FixedUpdate()
        {
            _lootboxHandler.Handle();
        }

        private void InitDrivers()
        {
            _raceStarted = false;
            _waypointsTrackersList = new List<WaypointsTracker>();

            GameObject parent = new GameObject("[Drivers]");
            GameObject driverPrefab = ResourcesLoader.LoadPrefab(ResourcePath.DriverPrefab);

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
                        _profiler,
                        _settingsContainer.PlaySounds
                        );

                    _sceneHandler.HandleEffectsFor(driver, _raceLevel);

                    driver.Subscribe(_raceUI);
                    driver.Subscribe(_lineHandler);
                    driver.DriverProfile.CarState.Subscribe(cs => HandlePlayerCarState(cs, driver));

                    var selfRighting = driver.CarObject.GetComponent<CarSelfRighting>();
                    var tracker = driver.CarObject.GetComponent<WaypointsTracker>();

                    void GetToCheckpoint() 
                    {
                        selfRighting.GetToCheckpoint();
                        tracker.ResetTargetToCashedValues();
                    }

                    _raceUI.Initialize(_raceLevelInitializer, selfRighting.RightCar, GetToCheckpoint);
                    _waypointTrackMain.OnCheckpointPass += MakeGameNotification;
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
                        null,
                        _settingsContainer.PlaySounds
                        );

                    driverGo.name += $"_{i + 1}";
                }

                driverGo.transform.SetParent(parent.transform, false);
                _waypointsTrackersList.Add(driver.WaypointsTracker);
            }
        }

        private IDisposable HandlePlayerCarState(CarState playerCarState, Driver playerDriver)
        {
            switch (playerCarState)
            {
                case CarState.None:
                case CarState.CanStart:
                case CarState.OnTrack:
                    break;
                case CarState.Finished:
                    _rewardsHandler.RewardForRace(playerDriver.DriverProfile.PositionInRace, out RaceRewardInfo info);
                    _raceUI.SetFinishValues(info.RewardMoneyAmount, info.RewardCupsAmount, info.MoneyTotal, info.GemsTotal);
                    break;
            }

            _raceUI.ChangeViewDependingOn(playerCarState);

            return Disposable.Empty;
        }

        private void NotifyRaceUI(Lootbox lootbox) => _raceUI.SetLootboxPopupValues(lootbox.Rarity);

        private void MakeGameNotification() => _gameEvents.Notification.OnNext(NotificationType.Checkpoint.ToString());

        private void OnDestroy()
        {
            _rewardsHandler.OnRaceRewardLootboxAdded -= NotifyRaceUI;
            _waypointTrackMain.OnCheckpointPass -= MakeGameNotification;
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