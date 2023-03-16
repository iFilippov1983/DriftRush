using RaceManager.Root;
using RaceManager.Tools;
using RaceManager.Cars;
using RaceManager.Cameras;
using RaceManager.Progress;
using System;
using System.Collections.Generic;
using UnityEngine;
using RaceManager.Waypoints;
using RaceManager.UI;
using Zenject;
using UniRx;
using UniRx.Triggers;
using RaceManager.Effects;
using IInitializable = RaceManager.Root.IInitializable;
using System.Threading.Tasks;

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
        private RaceScoresCounter _scoresCounter;
        private RaceLevelInitializer _raceLevelInitializer;
        private RaceLineHandler _lineHandler;
        private IRaceLevel _raceLevel;
        private RewardsHandler _rewardsHandler;
        private Profiler _profiler;
        private RaceUI _raceUI;
        private InRacePositionsHandler _positionsHandler;
        private InRaceLootboxHandler _lootboxHandler;
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
            MakeSubscriptions();

            _positionsHandler.StartHandling(_waypointsTrackersList);
        }

        private void MakeSubscriptions()
        {
            this.FixedUpdateAsObservable()
                .Subscribe(_ =>
                {
                    _lootboxHandler.Handle();
                    _scoresCounter.CountScores();
                })
                .AddTo(this);

            this.UpdateAsObservable()
                .Where(_ => _raceUI.RaceFinished == false)
                .Subscribe(_ => 
                {
                    _raceUI.ShowSpeed();
                    _raceUI.HandlePositionIndication();
                    _raceUI.HandleProgressBar();
                })
                .AddTo(this);

            this.UpdateAsObservable()
                .Where(_ => Input.GetMouseButton(0) && _raceUI.RaceFinished == false)
                .Subscribe(_ => 
                {
                    _raceUI.HandleAcceleration(true);
                })
                .AddTo(this);

            this.UpdateAsObservable()
                .Where(_ => Input.GetMouseButtonUp(0) && _raceUI.RaceFinished == false)
                .Subscribe(_ =>
                {
                    _raceUI.HandleAcceleration(false);
                })
                .AddTo(this);

            this.UpdateAsObservable()
                .Where(_ => !_raceStarted && CanStartImmediate)
                .Take(1)
                .Subscribe(_ =>
                {
                    _raceStarted = true;
                    EventsHub<RaceEvent>.BroadcastNotification(RaceEvent.COUNTDOWN);
                })
                .AddTo(this);

            _scoresCounter.ScoresCount
                .Where(data => data.CurrentScoresValue > 0)
                .Subscribe(data =>
                {
                    bool showPause = data.Timer > 0;
                    bool showScores = data.Timer >= 0;

                    _raceUI.ShowPause(showPause, data.Timer);
                    _raceUI.ShowScores(showScores, data.CurrentScoresValue);

                    int totalScores = data.CurrentScoresValue > data.TotalScoresValue
                    ? data.CurrentScoresValue 
                    : data.TotalScoresValue;

                    _rewardsHandler.SetMoneyReward(data.ScoresType, totalScores);
                })
                .AddTo(this);

            _scoresCounter.ExtraScoresCount
                .Subscribe(data =>
                {
                    _raceUI.ShowExtraScores(data.ScoresType, data.CurrentScoresValue);
                    _rewardsHandler.SetMoneyReward(data.ScoresType, data.TotalScoresValue);
                })
                .AddTo(this);

            _gameEvents.Notification
                .Where(s => s == NotificationType.Checkpoint.ToString())
                .Subscribe(s => 
                {
                    _lineHandler.StartHandling();
                })
                .AddTo(this);

            _raceUI.OnAdsInit += ShowAds;
            _rewardsHandler.OnRaceRewardLootboxAdded += SetRaceUI;
            _waypointTrackMain.OnCheckpointPass += MakeCheckpointNotification;
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

                    _sceneHandler.HandleSceneFor(driver, _raceLevel);

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

                    _scoresCounter = new RaceScoresCounter(driver.Car, _rewardsScheme);
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
                    _rewardsHandler.RewardForRaceInit(playerDriver.DriverProfile.PositionInRace, out RaceRewardInfo info);
                    _raceUI.SetFinishValues(info);
                    _lineHandler.StopHandling();
                    break;
                case CarState.Stopped:
                    Singleton<RaceCamerasHandler>.Instance.SetCameraToFinalPosition();
                    break;
            }

            _raceUI.ChangeViewDependingOn(playerCarState);

            return Disposable.Empty;
        }

        private async void ShowAds()
        {
            //TODO: implement cases: complete/fail

            await Task.Delay(1000);

            _rewardsHandler.RewardForRaceMoneyMultiplyed();
            _raceUI.OnAdsRewardAction();
        }

        private void SetRaceUI(Lootbox lootbox) => _raceUI.SetLootboxToGrant(lootbox.Rarity);

        private void MakeCheckpointNotification() => _gameEvents.Notification.OnNext(NotificationType.Checkpoint.ToString());

        private void OnDestroy()
        {
            _raceUI.OnAdsInit -= ShowAds;
            _rewardsHandler.OnRaceRewardLootboxAdded -= SetRaceUI;
            _waypointTrackMain.OnCheckpointPass -= MakeCheckpointNotification;

            _scoresCounter.Dispose();
        }
    }
}