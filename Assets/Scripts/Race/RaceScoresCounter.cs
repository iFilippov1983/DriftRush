using RaceManager.Cars;
using RaceManager.Progress;
using System;
using UnityEngine;
using UniRx;
using RaceManager.Tools;
using System.Collections.Generic;

namespace RaceManager.Race
{
    public class RaceScoresCounter : IDisposable
    {
        private int _scoresDrift;
        private int _scoresBump;
        private int _scoresCrush;
        private int _scoresTotal;

        private float _driftDistanceCounter;
        private float _showTotalTime;
        private float _driftCountTime;
        private float _driftTime;
        private float _driftFactor;

        private Car _car;
        private RaceRewardsScheme _rewardsScheme;

        private Dictionary<string, float> _collidingObects = new Dictionary<string, float>();

        public Subject<TotalScoreData> TotalScoresCount;
        public Subject<DriftScoresData> DriftScoresCount;
        public Subject<CollisionScoresData> CollisionScoresCount;

        public RaceScoresCounter(Car car, RaceRewardsScheme rewardsScheme)
        {
            _car = car;
            _rewardsScheme = rewardsScheme;
            _showTotalTime = ShowScoresTime;
            _driftFactor = DriftFactorMin;
            _driftTime = 0f;

            TotalScoresCount = new Subject<TotalScoreData>();
            DriftScoresCount = new Subject<DriftScoresData>();
            CollisionScoresCount = new Subject<CollisionScoresData>();

            _car.CollisionAction += CountCollisionScores;
            _car.CollisionStayAction += ResetCollisionTimer;
        }

        private float DriftFactorMin => _rewardsScheme.DriftFactorMin;
        private float DriftFactorMax => _rewardsScheme.DriftFactorMax;
        private float DriftFactorInreaseStep => _rewardsScheme.DriftFactorIncreaseStep;
        private float DriftFactorInreaseTime => _rewardsScheme.DrifFactorIncreaseTime;
        private float DriftCountTime => _rewardsScheme.DriftCountTime;
        private float ShowScoresTime => _rewardsScheme.ShowScoresDuration;
        private float MinDriftDistanceValue => _rewardsScheme.MinDriftDistanceValue;
        private float BumpScores => _rewardsScheme.BumpScores;
        private float MinCollisionInterval => _rewardsScheme.MinCollisionInterval;
        private float CrushScores => _rewardsScheme.CrushScores;

        private bool CarIsSlipping
        {
            get 
            {
                foreach (var w in _car.Wheels)
                { 
                    if(w.HasSideSlip)
                        return true;
                }
                return false;
            }
        }

        public void CountDriftScores()
        {
            DriftScoresData data;

            if (CarIsSlipping)
            {
                _showTotalTime = ShowScoresTime;
                _driftCountTime = DriftCountTime;
                _driftTime += Time.fixedDeltaTime;
                _driftDistanceCounter += _car.CurrentSpeed * Time.fixedDeltaTime;

                if (_driftDistanceCounter > MinDriftDistanceValue)
                {
                    if (_driftTime > DriftFactorInreaseTime)
                    {
                        _driftTime = 0;
                        _driftFactor += DriftFactorInreaseStep;

                        if(_driftFactor > DriftFactorMax)
                            _driftFactor = DriftFactorMax;
                    }

                    int scoresValue = Mathf.RoundToInt(_driftDistanceCounter * DriftFactorMin);

                    data = new DriftScoresData()
                    {
                        CurrentScoresValue = scoresValue,
                        TotalScoresValue = _scoresDrift,
                        ScoresFactorThisType = _driftFactor,
                        ScoresCountTime = _driftCountTime,
                        isDrifting = true
                    };

                    DriftScoresCount.OnNext(data);
                }
            }
            else
            {
                _driftCountTime -= Time.fixedDeltaTime;
                
                int lastDriftValue = Mathf.RoundToInt(_driftDistanceCounter * DriftFactorMin);

                data = new DriftScoresData()
                {
                    ScoresFactorThisType = _driftFactor
                };

                if (_driftCountTime < 0 && lastDriftValue > 0)
                {
                    _driftTime = 0;

                    lastDriftValue = Mathf.RoundToInt(lastDriftValue * _driftFactor) ;
                    _scoresDrift += lastDriftValue;
                    _driftDistanceCounter = 0f;

                    data.CurrentScoresValue = lastDriftValue;
                    data.isDrifting = false;

                    _driftFactor = DriftFactorMin;

                    //Debug.Log($"Drift scores counted: {lastDriftValue} => Total: {_scoresDrift}");

                    CountTotalScores();
                }

                data.TotalScoresValue = _scoresDrift;

                DriftScoresCount.OnNext(data);

                _showTotalTime -= Time.fixedDeltaTime;
            }
        }

        public void CountDriftScoresImmediate()
        {
            int lastDriftValue = Mathf.RoundToInt(_driftDistanceCounter * DriftFactorMin);

            DriftScoresData data = new DriftScoresData()
            {
                ScoresFactorThisType = _driftFactor
            };

            if (lastDriftValue > 0)
            {
                lastDriftValue = Mathf.RoundToInt(lastDriftValue * _driftFactor);
                _scoresDrift += lastDriftValue;
                _driftDistanceCounter = 0f;

                data.CurrentScoresValue = lastDriftValue;
                data.isDrifting = false;

                _driftFactor = DriftFactorMin;

                Debug.Log($"Drift scores counted IMMEDIATE: {lastDriftValue} => Total: {_scoresDrift}");
            }

            data.TotalScoresValue = _scoresDrift;

            DriftScoresCount.OnNext(data);
        }

        private void CountCollisionScores(Car car, Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out ICountableCollision countable) == false)
                return;

            if (_collidingObects.ContainsKey(countable.ID))
            {
                float lastCollisionTime = _collidingObects[countable.ID];
                if ((Time.time - lastCollisionTime) < MinCollisionInterval)
                    return;
            }
            else
            {
                _collidingObects.Add(countable.ID, 0f);
            }

            int colLayer = countable.Layer;
            int carLayer = car.Layer;
            int scores = 0;
            CollisionScoresData data = default;

            if (colLayer == carLayer)
            {
                scores = Mathf.RoundToInt(BumpScores);
                _scoresBump += scores;

                data = new CollisionScoresData()
                {
                    ScoresType = RaceScoresType.Bump,
                    CurrentScoresThisTypeValue = scores,
                    TotalScoresThisTypeValue = _scoresBump
                };
            }
            else if (colLayer == LayerMask.NameToLayer(Layer.Crushable))
            {
                scores = Mathf.RoundToInt(CrushScores);
                _scoresCrush += scores;

                data = new CollisionScoresData()
                {
                    ScoresType = RaceScoresType.Crush,
                    CurrentScoresThisTypeValue = scores,
                    TotalScoresThisTypeValue = _scoresCrush,
                };
            }

            if (scores != 0)
            {
                _showTotalTime = ShowScoresTime;
                CollisionScoresCount.OnNext(data);
            }

            _collidingObects[countable.ID] = Time.time;
        }

        private void CountTotalScores()
        {
            int total = _scoresDrift + _scoresBump + _scoresCrush;
            
            bool newTotal = _scoresTotal != total;
            _scoresTotal = total;

            if (newTotal)
                _showTotalTime = ShowScoresTime;

            int showTotalTimerValue = Mathf.CeilToInt(_showTotalTime);

            TotalScoreData totalData = new TotalScoreData()
            {
                Value = _scoresTotal,
                Timer = showTotalTimerValue,
                ShowScores = _showTotalTime > 0 && newTotal
            };

            TotalScoresCount.OnNext(totalData);
        }

        private void ResetCollisionTimer(Car car, Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out ICountableCollision countable))
            {
                _collidingObects[countable.ID] = Time.time;
            }
        }

        public void Dispose()
        {
            _car.CollisionAction -= CountCollisionScores;
            _car.CollisionStayAction -= ResetCollisionTimer;
        }
    }
}