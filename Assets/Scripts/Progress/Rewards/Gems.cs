using System;
using UnityEngine;

namespace RaceManager.Progress
{
    [Serializable]
    public class Gems : IReward
    {
        [SerializeField] private int _gems;

        public bool IsReceived { get; set; }
        public GameUnitType Type => GameUnitType.Gems;
        public int GemsAmount => _gems;
        public UnitReplacementInfo? ReplacementInfo { get; set; } = null;

        public void Reward(Profiler profiler)
        {
            profiler.AddGems(_gems);
            IsReceived = true;
        }
    }
}
