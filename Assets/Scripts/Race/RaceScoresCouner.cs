using RaceManager.Cars;
using RaceManager.Progress;
using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace RaceManager.Race
{
    public class RaceScoresCouner
    {
        private const int MinDriftDistanceValue = 1;
        private const int StopDriftCountValue = -1;

        private int _scoresDrift;
        private int _scoresCarHit;
        private int _scoresCrash;
        private int _scoresTotal;

        private float _driftDistanceCounter;
        private float _driftPauseTime;

        private Car _car;
        private RaceRewardsScheme _rewardsScheme;

        public Subject<(int scores, int timer)> DriftScoresCount;

        public RaceScoresCouner(Car car, RaceRewardsScheme rewardsScheme)
        {
            _car = car;
            _rewardsScheme = rewardsScheme;
            _driftPauseTime = AvailableDriftPause;

            DriftScoresCount = new Subject<(int scores, int timer)>();
        }

        private Rigidbody CarRb => _car.RB;
        private float DriftFactor => _rewardsScheme.DriftFactor;
        private float AvailableDriftPause => _rewardsScheme.AvailableDriftPause;
        private float OpponentHitFactor => _rewardsScheme.OpponentHitFactor;
        private float CrashHitFactor => _rewardsScheme.CrashHitFactor;
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

        public void CountDriftScores()
        {
            if (CarIsSlipping)
            {
                _driftPauseTime = AvailableDriftPause;
                _driftDistanceCounter += _car.CurrentSpeed * Time.fixedDeltaTime;

                if (_driftDistanceCounter > MinDriftDistanceValue)
                {
                    int driftValue = Mathf.RoundToInt(_driftDistanceCounter * _rewardsScheme.DriftFactor);
                    DriftScoresCount.OnNext((scores: driftValue, timer: 0));

                    //$"Drift value => {driftValue}".Log(Logger.ColorBlue);
                }
            }
            else
            {
                _driftPauseTime -= Time.fixedDeltaTime;

                int pauseTimerValue = Mathf.RoundToInt(_driftPauseTime);
                int lastDriftValue = Mathf.RoundToInt(_driftDistanceCounter * _rewardsScheme.DriftFactor);

                DriftScoresCount.OnNext((scores: lastDriftValue, timer: pauseTimerValue));
                //Debug.Log($"[Drift Pause Timer: {pauseTimerValue}]");

                if (_driftPauseTime < 0 && lastDriftValue > 0)
                {
                    _scoresDrift += lastDriftValue;
                    _driftDistanceCounter = 0f;

                    DriftScoresCount.OnNext((scores: _scoresDrift, timer: StopDriftCountValue));

                    //$"Drift Scores Value => {_scoresDrift}".Log(Logger.ColorGreen);
                }
            }
        }
    }
}