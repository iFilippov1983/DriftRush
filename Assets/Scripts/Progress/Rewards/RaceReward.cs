using System;
using UnityEngine;

namespace RaceManager.Progress
{
    [Serializable]
    public class RaceReward : IReward
    {
        [SerializeField] private int _money;
        [SerializeField] private int _cups;

        public bool IsReceived { get; set; }
        public int Money => _money;
        public int Cups => _cups;
        public GameUnitType Type => GameUnitType.RaceReward;
        public UnitReplacementInfo? ReplacementInfo { get; set; } = null;

        public void MultiplyMoney(int multiplyer) => _money *= multiplyer;
        public void AddMoney(int value) => _money += value;
        public void AddCups(int value) => _cups += value;

        public void Reward(Profiler profiler)
        {
            profiler.AddMoney(Money);
            profiler.AddCups(Cups);
            IsReceived = true;
        }

        public void Unreward(Profiler profiler)
        { 
            profiler.AddMoney(-Money);
            profiler.AddCups(-Cups);
            IsReceived = false;
        }
    }
}
