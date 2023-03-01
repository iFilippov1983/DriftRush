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
        private const int StopDriftCountValue = -1;

        private int _scoresDrift;
        private int _scoresBump;
        private int _scoresCrush;

        private float _driftDistanceCounter;
        private float _driftPauseTime;

        private Car _car;
        private RaceRewardsScheme _rewardsScheme;

        private Dictionary<string, float> _collidingObects = new Dictionary<string, float>();

        public Subject<RaceScoresData> ScoresCount;
        public Subject<RaceScoresData> ExtraScoresCount;

        public RaceScoresCounter(Car car, RaceRewardsScheme rewardsScheme)
        {
            _car = car;
            _rewardsScheme = rewardsScheme;
            _driftPauseTime = AvailableDriftPause;

            ScoresCount = new Subject<RaceScoresData>();
            ExtraScoresCount = new Subject<RaceScoresData>();

            _car.CollisionAction += CountExtraScores;
            _car.CollisionStayAction += ResetCollisionTimer;
        }

        private float DriftFactor => _rewardsScheme.DriftFactor;
        private float AvailableDriftPause => _rewardsScheme.AvailableDriftPause;
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

        public void CountScores()
        {
            RaceScoresData data;

            if (CarIsSlipping)
            {
                _driftPauseTime = AvailableDriftPause;
                _driftDistanceCounter += _car.CurrentSpeed * Time.fixedDeltaTime;

                if (_driftDistanceCounter > MinDriftDistanceValue)
                {
                    int scoresValue = Mathf.RoundToInt(_driftDistanceCounter * DriftFactor);

                    data = new RaceScoresData()
                    {
                        ScoresType = RaceScoresType.Drift,
                        CurrentScoresValue = scoresValue,
                        TotalScoresValue = _scoresDrift,
                        Timer = 0
                    };

                    ScoresCount.OnNext(data);
                }
            }
            else
            {
                _driftPauseTime -= Time.fixedDeltaTime;

                int pauseTimerValue = Mathf.CeilToInt(_driftPauseTime);
                int lastDriftValue = Mathf.RoundToInt(_driftDistanceCounter * DriftFactor);

                data = new RaceScoresData()
                {
                    ScoresType = RaceScoresType.Drift,
                    CurrentScoresValue = lastDriftValue
                };

                if (_driftPauseTime < 0 && lastDriftValue > 0)
                {
                    _scoresDrift += lastDriftValue;
                    _driftDistanceCounter = 0f;

                    data.Timer = StopDriftCountValue;
                }
                else
                {
                    data.Timer = pauseTimerValue;
                }

                data.TotalScoresValue = _scoresDrift;

                ScoresCount.OnNext(data);
            }
        }

        private void CountExtraScores(Car car, Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out ICountableCollision countable))
            {
                if (_collidingObects.ContainsKey(countable.ID))
                {
                    if ((Time.time - _collidingObects[countable.ID]) < MinCollisionInterval)
                        return;
                }
                else
                {
                    _collidingObects.Add(countable.ID, 0f);
                }

                int colLayer = countable.Layer;
                int carLayer = car.Layer;
                int scores = 0;
                RaceScoresData data = default;

                if (colLayer == carLayer)
                {
                    scores = Mathf.RoundToInt(BumpScores);
                    _scoresBump += scores;

                    data = new RaceScoresData()
                    {
                        ScoresType = RaceScoresType.Bump,
                        CurrentScoresValue = scores,
                        TotalScoresValue = _scoresBump
                    };
                }
                else if (colLayer == LayerMask.NameToLayer(Layer.Crushable))
                {
                    scores = Mathf.RoundToInt(CrushScores);
                    _scoresCrush += scores;

                    data = new RaceScoresData()
                    {
                        ScoresType = RaceScoresType.Crush,
                        CurrentScoresValue = scores,
                        TotalScoresValue = _scoresCrush
                    };
                }

                if(scores != 0)
                    ExtraScoresCount.OnNext(data);

                _collidingObects[countable.ID] = Time.time;
            }
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
            _car.CollisionAction -= CountExtraScores;
            _car.CollisionStayAction -= ResetCollisionTimer;
        }
    }
}