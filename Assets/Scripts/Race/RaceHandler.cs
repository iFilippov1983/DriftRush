using RaceManager.Root;
using RaceManager.Tools;
using RaceManager.Cars;
using RaceManager.Cameras;
using RaceManager.Progress;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using RaceManager.Waypoints;
using RaceManager.UI;
using Zenject;
using UniRx;
using UniRx.Triggers;
using RaceManager.Effects;
using IInitializable = RaceManager.Root.IInitializable;

namespace RaceManager.Race
{
    public class RaceHandler : MonoBehaviour, IInitializable
    {
        [SerializeField] private MaterialsContainer _materialsContainer;
        [SerializeField] private RaceRewardsScheme _rewardsScheme;
        [SerializeField] private CarsDepot _opponentsCarsDepot;

        private OpponentsCarTuner _opponentsCarTuner;
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
        private bool _adjustOpponents;

        private float _lastDriftFactor = 1f;

        private bool CanStartImmediate => _profiler.CanStartImmediate;

        [Inject]
        private void Construct
            (
            OpponentsCarTuner opponentsCarTuner,
            CarsDepot playerCarsDepot,
            GameSettingsContainer settingsContainer,
            RaceSceneHandler sceneHandler,
            Profiler profiler,
            InRacePositionsHandler positionsHandler,
            RaceUI raceUI,
            RewardsHandler rewardsHandler,
            GameEvents gameEvents
            )
        {
            _opponentsCarTuner = opponentsCarTuner;
            _playerCarsDepot = playerCarsDepot;
            _settingsContainer = settingsContainer;
            _sceneHandler = sceneHandler;
            _profiler = profiler;
            _positionsHandler = positionsHandler;
            _raceUI = raceUI;
            _rewardsHandler = rewardsHandler;
            _gameEvents = gameEvents;
        }

        public void Initialize()
        {
            InitRaceLevel();
            InitLootboxHandler();
            InitLineHandler();
            InitOpponentsTuner();
            InitDrivers();
            MakeSubscriptions();

            _positionsHandler.StartHandling(_waypointsTrackersList);
        }

        private void InitRaceLevel()
        {
            var builder = GetLevelBuilder();
            _raceLevelInitializer = new RaceLevelInitializer(_profiler, builder);

            if (_profiler.RacesTotalCounter > 0)
            {
                _raceUI.ShowSimpleFinish = false;
                _raceLevelInitializer.MakeCommonRaceRunLevel();
            }
            else 
            {
                _raceUI.ShowSimpleFinish = true;
                _raceLevelInitializer.MakeInitialLevel();
            }

            _raceLevel = builder.GetResult();
            //_raceLevel = _raceLevelInitializer.GetRaceLevel();

            _startPoints = _raceLevel.StartPoints;
            _waypointTrackMain = _raceLevel.WaypointTrackMain;
            _waypointTrackEven = _raceLevel.WaypointTrackEven;
            _waypointTrackOdd = _raceLevel.WaypointTrackOdd;
        }

        private void InitLootboxHandler()
        {
            _lootboxHandler = new InRaceLootboxHandler(_profiler);
        }

        private void InitLineHandler()
        {
            _lineHandler = new RaceLineHandler(_raceLevel.WaypointTrackMain, _raceLevel.RaceLine, _settingsContainer.UseRaceLine);
        }

        private void InitOpponentsTuner()
        {
            _adjustOpponents = _opponentsCarTuner.CanAdjust || _opponentsCarTuner.CanAdjustThreshold > _profiler.GetVictoriesTotalCount();

            if (_adjustOpponents)
            {
                bool needPercentageGrade = _profiler.GetLastInRacePosition() == PositionInRace.First;
                _opponentsCarTuner.Initialize(_opponentsCarsDepot, needPercentageGrade);
            }
                
        }

        private void InitDrivers()
        {
            _raceStarted = false;
            _waypointsTrackersList = new List<WaypointsTracker>();

            GameObject parent = new GameObject("[Drivers]");
            GameObject driverPrefab = ResourcesLoader.LoadPrefab(ResourcePath.DriverPrefab);

            for (int i = 0; i < _startPoints.Length; i++)
            {
                if (_startPoints[i].isAvailable == false)
                    continue;

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

                    int participantsCount = 0;
                    foreach (var sp in _startPoints)
                    { 
                        if(sp.isAvailable)
                            participantsCount++;
                    }

                    //_raceUI.Initialize(_raceLevelInitializer, selfRighting.RightCar, GetToCheckpoint);
                    //_raceUI.Initialize(_startPoints.Length, selfRighting.RightCar, GetToCheckpoint);
                    _raceUI.Initialize(participantsCount, selfRighting.RightCar, GetToCheckpoint);

                    _scoresCounter = new RaceScoresCounter(driver.Car, _rewardsScheme);
                }
                else
                {
                    WaypointTrack track = (i % 2) == 0 ? _waypointTrackEven : _waypointTrackOdd;

                    if (_adjustOpponents)
                        _opponentsCarTuner.AdjustOpponentsCarDepot();

                    driver.Initialize
                        (
                        _startPoints[i].Type, 
                        _opponentsCarsDepot, 
                        track, 
                        _materialsContainer, 
                        null,
                        _settingsContainer.PlaySounds
                        );

                    driver.TrackRecommendedSpeed();

                    _opponentsCarTuner.AdjustOpponentsCarView(driver.CarVisual);

                    driverGo.name += $"_{i + 1}";
                }

                driverGo.transform.SetParent(parent.transform, false);
                _waypointsTrackersList.Add(driver.WaypointsTracker);
            }
        }

        private void MakeSubscriptions()
        {
            this.FixedUpdateAsObservable()
                .Subscribe(_ =>
                {
                    _lootboxHandler.Handle();
                    _scoresCounter.CountDriftScores();
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
                    _scoresCounter.CanCount = true;
                    EventsHub<RaceEvent>.BroadcastNotification(RaceEvent.COUNTDOWN);
                })
                .AddTo(this);

            _scoresCounter.TotalScoresCount
                .Where(data => data.Value > 0)
                .Subscribe(data =>
                {
                    _raceUI.ShowScoresTotal(data.ShowScores, data.Value, data.Timer).AddTo(_raceUI);
                    _raceUI.ShowPause(data.ShowScores, data.Timer);
                })
                .AddTo(this);

            _scoresCounter.DriftScoresCount
                .Where(data => data.CurrentScoresValue > 0)
                .Subscribe(data =>
                {
                    bool animateFactor = data.ScoresFactorThisType != _lastDriftFactor;

                    _raceUI.ShowDriftScores
                        (
                        data.isDrifting,
                        data.CurrentScoresValue,
                        data.ScoresFactorThisType,
                        data.ScoresCountTime,
                        animateFactor
                        )
                        .AddTo(_raceUI);

                    int totalScoresThisType = data.CurrentScoresValue > data.TotalScoresValue
                        ? data.CurrentScoresValue
                        : data.TotalScoresValue;

                    _rewardsHandler.SetMoneyReward(RaceScoresType.Drift, totalScoresThisType);

                    _lastDriftFactor = data.ScoresFactorThisType;

                    _sceneHandler.HandleDriftScoresCountEffect();
                })
                .AddTo(this);

            _scoresCounter.CollisionScoresCount
                .Subscribe(data =>
                {
                    _raceUI.ShowCollisionScores(data.ScoresType, data.CurrentScoresThisTypeValue);
                    _rewardsHandler.SetMoneyReward(data.ScoresType, data.TotalScoresThisTypeValue);
                })
                .AddTo(this);

            _gameEvents.Notification
                .Where(s => s == NotificationType.RaceLine.ToString())
                .Subscribe(s =>
                {
                    _lineHandler.StartHandling();
                })
                .AddTo(this);

            _raceUI.OnAdsInit += ShowAds;
            _rewardsHandler.OnRaceRewardLootboxAdded += SetRaceUI;
            _waypointTrackMain.OnWaypointPassedNotification += MakeNotification;
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
                    _scoresCounter.CountDriftScoresImmediate();
                    _scoresCounter.CanCount = false;
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

            await Task.Delay(1000);//To fake ad period

            _rewardsHandler.RewardForRaceMoneyMultiplyed();
            _raceUI.OnAdsRewardAction();
        }

        private IRaceLevelBuilder GetLevelBuilder()
        {
            //TODO: return builder type depending on race type

            return new CommonRaceRunLevelBuilder();
        }

        private void SetRaceUI(Lootbox lootbox) => _raceUI.SetLootboxToGrant(lootbox.Rarity);
        private void MakeNotification(NotificationType notification) => _gameEvents.Notification.OnNext(notification.ToString());

        private void OnDestroy()
        {
            _raceUI.OnAdsInit -= ShowAds;
            _rewardsHandler.OnRaceRewardLootboxAdded -= SetRaceUI;
            _waypointTrackMain.OnWaypointPassedNotification -= MakeNotification;

            _scoresCounter.Dispose();
        }
    }
}