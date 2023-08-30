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

        #region Minor variables

        private DriftScoresData m_DriftScoresData;
        private DriftScoresData m_DriftScoresDataImmediate;

        private CollisionScoresData m_CollisionScoresData;

        private ICountableCollision m_CountableCollision;
        private ICountableCollision m_CountableCollisionReset;

        private TotalScoreData m_TotalScoreData;

        private int m_LastDriftValue;

        #endregion

        public bool CanCount { get; set; } = true;

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

            m_DriftScoresData = new DriftScoresData();
            m_CollisionScoresData = new CollisionScoresData();
            m_TotalScoreData = new TotalScoreData();

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
            if (!CanCount)
                return;

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

                    m_DriftScoresData.Reset();
                    m_DriftScoresData.CurrentScoresValue = Mathf.RoundToInt(_driftDistanceCounter * DriftFactorMin);
                    m_DriftScoresData.TotalScoresValue = _scoresDrift;
                    m_DriftScoresData.ScoresFactorThisType = _driftFactor;
                    m_DriftScoresData.ScoresCountTime = _driftCountTime;
                    m_DriftScoresData.isDrifting = true;

                    //m_DriftScoresData = new DriftScoresData()
                    //{
                    //    CurrentScoresValue = Mathf.RoundToInt(_driftDistanceCounter * DriftFactorMin),
                    //    TotalScoresValue = _scoresDrift,
                    //    ScoresFactorThisType = _driftFactor,
                    //    ScoresCountTime = _driftCountTime,
                    //    isDrifting = true
                    //};

                    DriftScoresCount.OnNext(m_DriftScoresData);
                }
            }
            else
            {
                _driftCountTime -= Time.fixedDeltaTime;

                m_LastDriftValue = Mathf.RoundToInt(_driftDistanceCounter * DriftFactorMin);

                m_DriftScoresData.Reset();
                m_DriftScoresData.ScoresFactorThisType = _driftFactor;

                //m_DriftScoresData = new DriftScoresData()
                //{
                //    ScoresFactorThisType = _driftFactor
                //};

                if (_driftCountTime < 0 && m_LastDriftValue > 0)
                {
                    _driftTime = 0;

                    m_LastDriftValue = Mathf.RoundToInt(m_LastDriftValue * _driftFactor) ;
                    _scoresDrift += m_LastDriftValue;
                    _driftDistanceCounter = 0f;

                    m_DriftScoresData.CurrentScoresValue = m_LastDriftValue;
                    m_DriftScoresData.isDrifting = false;

                    _driftFactor = DriftFactorMin;

                    //Debug.Log($"Drift scores counted: {lastDriftValue} => Total: {_scoresDrift}");

                    CountTotalScores();
                }

                m_DriftScoresData.TotalScoresValue = _scoresDrift;

                DriftScoresCount.OnNext(m_DriftScoresData);

                _showTotalTime -= Time.fixedDeltaTime;
            }
        }

        public void CountDriftScoresImmediate()
        {
            int lastDriftValue = Mathf.RoundToInt(_driftDistanceCounter * DriftFactorMin);

            m_DriftScoresDataImmediate = new DriftScoresData()
            {
                ScoresFactorThisType = _driftFactor
            };

            if (lastDriftValue > 0)
            {
                lastDriftValue = Mathf.RoundToInt(lastDriftValue * _driftFactor);
                _scoresDrift += lastDriftValue;
                _driftDistanceCounter = 0f;

                m_DriftScoresDataImmediate.CurrentScoresValue = lastDriftValue;
                m_DriftScoresDataImmediate.isDrifting = false;

                _driftFactor = DriftFactorMin;

                //Debug.Log($"Drift scores counted IMMEDIATE: {lastDriftValue} => Total: {_scoresDrift}");
            }

            m_DriftScoresDataImmediate.TotalScoresValue = _scoresDrift;

            DriftScoresCount.OnNext(m_DriftScoresDataImmediate);
        }

        private void CountCollisionScores(Car car, Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out m_CountableCollision) == false || !CanCount)
                return;

            if (_collidingObects.ContainsKey(m_CountableCollision.ID))
            {
                float lastCollisionTime = _collidingObects[m_CountableCollision.ID];
                if ((Time.time - lastCollisionTime) < MinCollisionInterval)
                    return;
            }
            else
            {
                _collidingObects.Add(m_CountableCollision.ID, 0f);
            }

            int colLayer = m_CountableCollision.Layer;
            int carLayer = car.Layer;
            int scores = 0;
            m_CollisionScoresData = default;

            if (colLayer == carLayer)
            {
                scores = Mathf.RoundToInt(BumpScores);
                _scoresBump += scores;

                m_CollisionScoresData.ScoresType = RaceScoresType.Bump;
                m_CollisionScoresData.CurrentScoresThisTypeValue = scores;
                m_CollisionScoresData.TotalScoresThisTypeValue = _scoresBump;

                //m_CollisionScoresData = new CollisionScoresData()
                //{
                //    ScoresType = RaceScoresType.Bump,
                //    CurrentScoresThisTypeValue = scores,
                //    TotalScoresThisTypeValue = _scoresBump
                //};
            }
            else if (colLayer == LayerMask.NameToLayer(Layer.Crushable))
            {
                scores = Mathf.RoundToInt(CrushScores);
                _scoresCrush += scores;

                m_CollisionScoresData.ScoresType = RaceScoresType.Crush;
                m_CollisionScoresData.CurrentScoresThisTypeValue = scores;
                m_CollisionScoresData.TotalScoresThisTypeValue = _scoresCrush;

                //m_CollisionScoresData = new CollisionScoresData()
                //{
                //    ScoresType = RaceScoresType.Crush,
                //    CurrentScoresThisTypeValue = scores,
                //    TotalScoresThisTypeValue = _scoresCrush,
                //};
            }

            if (scores != 0)
            {
                _showTotalTime = ShowScoresTime;
                CollisionScoresCount.OnNext(m_CollisionScoresData);
            }

            _collidingObects[m_CountableCollision.ID] = Time.time;
        }

        private void CountTotalScores()
        {
            int total = _scoresDrift + _scoresBump + _scoresCrush;
            
            bool newTotal = _scoresTotal != total;
            _scoresTotal = total;

            if (newTotal)
                _showTotalTime = ShowScoresTime;

            int showTotalTimerValue = Mathf.CeilToInt(_showTotalTime);

            m_TotalScoreData.Value = _scoresTotal;
            m_TotalScoreData.Timer = showTotalTimerValue;
            m_TotalScoreData.ShowScores = _showTotalTime > 0 && newTotal;

            //m_TotalScoreData = new TotalScoreData()
            //{
            //    Value = _scoresTotal,
            //    Timer = showTotalTimerValue,
            //    ShowScores = _showTotalTime > 0 && newTotal
            //};

            TotalScoresCount.OnNext(m_TotalScoreData);
        }

        private void ResetCollisionTimer(Car car, Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out m_CountableCollisionReset))
            {
                _collidingObects[m_CountableCollisionReset.ID] = Time.time;
            }
        }

        public void Dispose()
        {
            _car.CollisionAction -= CountCollisionScores;
            _car.CollisionStayAction -= ResetCollisionTimer;
        }
    }
}