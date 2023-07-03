using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Cars
{
    [Serializable]
    public class CarRankingScheme
    {
        [JsonProperty]
        [SerializeField]
        private List<CarRank> _ranks = new List<CarRank>();

        private CarRank _currentRank;
        private CarRank _nextRank;

        [JsonIgnore]
        public List<CarRank> Ranks => _ranks;

        //[ShowInInspector, ReadOnly]
        //public CarRank GetCurrentRank
        //{
        //    get
        //    {
        //        try
        //        {
        //            _currentRank = _ranks.First(s => s.IsGranted == false);
        //        }
        //        catch (Exception)
        //        {
        //            _currentRank = _ranks.Last(s => s.IsGranted == true);
        //        }

        //        return _currentRank;
        //    }
        //}

        public int NextRankPointsForAccess => GetNextRank().PointsForAccess;

        public int RankPointsTotalForCar
        {
            get 
            {
                int result = 0;
                foreach (CarRank rank in _ranks) 
                { 
                    result += rank.PointsForAccess;
                }
                return result;
            }
        }

        public bool CarIsAvailable
        {
            get
            {
                foreach (var rank in _ranks)
                { 
                    if(rank.Rank == Rank.Rank_1 && rank.IsGranted)
                        return true;
                }

                return false;
            }
        }

        public bool AllRanksGranted => _ranks.TrueForAll(r => r.IsGranted == true);

        public bool AllRanksReached => _ranks.TrueForAll(r => r.IsReached == true);

        public CarRank GetCurrentRank()
        {
            _currentRank = null;

            try
            {
                _currentRank = _ranks.Last(r => r.IsGranted == true);
            }
            catch
            {
                foreach (var rank in _ranks)
                {
                    if (rank.IsReached)
                    {
                        _currentRank = rank;
                        break;
                    }
                }

                if (_currentRank == null)
                    _currentRank = _ranks[0];
            }

            return _currentRank;
        }

        public CarRank GetNextRank()
        {
            try
            {
                _nextRank = _ranks.First(s => s.IsGranted == false);
            }
            catch
            {
                GetCurrentRank();
            }

            return _nextRank;
        }

        [Serializable]
        public class CarRank
        {
            [Space(15)]
            public Rank Rank;
            [Space(15)]
            public int PointsForAccess;
            public int AccessCost;
            [Range(0f, 1f)]
            public float AvailableTunePercentage;
            public bool IsGranted;
            public bool IsReached;
            [Space(15)]
            public int UpgradeCostBase;
            public int UpgradeCostCurrent;
            public float StatsToAddPerUpgrade;
            [Range(0f, 1f)]
            public float CurrentCostGrowth;
        }
    }

}
