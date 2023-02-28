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
        private const int MinDriftDistanceValue = 1;
        private const int StopDriftCountValue = -1;

        private int _scoresDrift;
        private int _scoresBump;
        private int _scoresCrush;

        private float _driftDistanceCounter;
        private float _driftPauseTime;

        private Car _car;
        private RaceRewardsScheme _rewardsScheme;

        private Dictionary<string, float> _collidingObects = new Dictionary<string, float>();

        public int DriftScoresTotal => _scoresDrift;
        public int BumpScoresTotal => _scoresBump;
        public int CrushScoresTotal => _scoresCrush;

        public Subject<RaceScoresInfo> ScoresCount;
        public Subject<RaceScoresInfo> ExtraScoresCount;

        public RaceScoresCounter(Car car, RaceRewardsScheme rewardsScheme)
        {
            _car = car;
            _rewardsScheme = rewardsScheme;
            _driftPauseTime = AvailableDriftPause;

            ScoresCount = new Subject<RaceScoresInfo>();
            ExtraScoresCount = new Subject<RaceScoresInfo>();

            _car.CollisionAction += CountExtraScores;
        }

        private float DriftFactor => _rewardsScheme.DriftFactor;
        private float AvailableDriftPause => _rewardsScheme.AvailableDriftPause;
        private float BumpScores => _rewardsScheme.BumpScores;
        private float MinCollisionInterval => _rewardsScheme.MinCollisionInterval;
        private float CrushScores => _rewardsScheme.CrushScores;
        private Wheel[] CarWheels => _car.Wheels;

        private bool CarIsSlipping
        {
            get 
            {
                foreach (var w in CarWheels)
                { 
                    if(w.HasSideSlip)
                        return true;
                }
                return false;
            }
        }

        public void CountScores()
        {
            RaceScoresInfo info;

            if (CarIsSlipping)
            {
                _driftPauseTime = AvailableDriftPause;
                _driftDistanceCounter += _car.CurrentSpeed * Time.fixedDeltaTime;

                if (_driftDistanceCounter > MinDriftDistanceValue)
                {
                    int scoresValue = Mathf.RoundToInt(_driftDistanceCounter * DriftFactor);

                    info = new RaceScoresInfo() 
                    { 
                        ScoresType = RaceScoresType.Drift,
                        CurrentScoresValue = scoresValue,
                        TotalScoresValue = _scoresDrift,
                        Timer = 0
                    };

                    ScoresCount.OnNext(info);
                }
            }
            else
            {
                _driftPauseTime -= Time.fixedDeltaTime;

                int pauseTimerValue = Mathf.RoundToInt(_driftPauseTime);
                int lastDriftValue = Mathf.RoundToInt(_driftDistanceCounter * DriftFactor);

                info = new RaceScoresInfo()
                {
                    ScoresType = RaceScoresType.Drift,
                    CurrentScoresValue = lastDriftValue,
                    TotalScoresValue = _scoresDrift
                };

                if (_driftPauseTime < 0 && lastDriftValue > 0)
                {
                    _scoresDrift += lastDriftValue;
                    _driftDistanceCounter = 0f;

                    info.TotalScoresValue = _scoresDrift;
                    info.Timer = StopDriftCountValue;
                }
                else
                {
                    info.Timer = pauseTimerValue;
                }

                ScoresCount.OnNext(info);
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
                    _collidingObects.Add(countable.ID, 0);
                }

                int colLayer = countable.Layer;
                int carLayer = car.Layer;
                int scores = 0;
                RaceScoresInfo info = default;

                if (colLayer == carLayer)
                {
                    scores = Mathf.RoundToInt(BumpScores);
                    _scoresBump += scores;

                    info = new RaceScoresInfo()
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

                    info = new RaceScoresInfo()
                    {
                        ScoresType = RaceScoresType.Crush,
                        CurrentScoresValue = scores,
                        TotalScoresValue = _scoresCrush
                    };
                }

                if(scores != 0)
                    ExtraScoresCount.OnNext(info);

                _collidingObects[countable.ID] = Time.time;
            }
        }

        public void Dispose()
        {
            _car.CollisionAction -= CountExtraScores;
        }
    }
}