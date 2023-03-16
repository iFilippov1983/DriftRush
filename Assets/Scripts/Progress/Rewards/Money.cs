using System;
using UnityEngine;

namespace RaceManager.Progress
{
    [Serializable]
    public class Money : IReward
    {
        [SerializeField] private int _money;

        public Money(int moneyAmount)
        {
            _money = moneyAmount;
        }

        public bool IsReceived { get; set; }
        public GameUnitType Type => GameUnitType.Money;
        public int MoneyAmount => _money;
        public UnitReplacementInfo? ReplacementInfo { get; set; } = null;

        public void Reward(Profiler profiler)
        {
            profiler.AddMoney(MoneyAmount, true);
            IsReceived = true;
        }
    }
}
