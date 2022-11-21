using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Progress
{
    [Serializable]
    public class ProgressStep
    {
        [SerializeField] private bool _needsBigPrefab;
        [SerializeField] private bool _isLast;
        private bool _rewardsReceived;


        public List<IReward> Rewards = new List<IReward>();

        public bool IsReached { get; set; }
        public bool RewardsReceived 
        {
            get 
            { 
                foreach (var reward in Rewards)
                    _rewardsReceived = reward.IsReceived;

                return _rewardsReceived;
            }
        }

        public bool BigPrefab => _needsBigPrefab;
        public bool IsLast => _isLast;
    }
}

