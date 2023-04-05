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

        [JsonIgnore]
        public List<CarRank> Ranks => _ranks;

        [ShowInInspector, ReadOnly]
        public CarRank CurrentRank
        {
            get
            {
                try
                {
                    _currentRank = _ranks.First(s => s.IsGranted == false);
                }
                catch (Exception)
                {
                    _currentRank = _ranks.Last(s => s.IsGranted == true);
                }

                return _currentRank;
            }
        }

        public int CurrentRankPointsForAccess => CurrentRank.PointsForAccess;

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
                var step = CurrentRank.Rank == Rank.Rank_1 && CurrentRank.IsGranted
                    ? CurrentRank
                    : _ranks?.Find(s => s.Rank == Rank.Rank_1 && s.IsGranted);

                return step != null;
            }
        }

        public float RanksWeightTotal
        {
            get 
            {
                float weight = 0;
                foreach (var rank in _ranks)
                    weight += rank.AvailableTunePercentage;

                return weight;
            }
        }

        public bool AllRanksGranted => _ranks.TrueForAll(r => r.IsGranted == true);

        public bool AllRanksReached => _ranks.TrueForAll(r => r.IsReached == true);

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
