using System;
using UnityEngine;

namespace RaceManager.Progress
{
    [Serializable]
    public class IncomeBonus : IReward
    {
        [Tooltip("Percentage of income to add to income factor")]
        [SerializeField] private float _incomeBonusInPercents;

        public bool IsReceived { get; set; }
        public GameUnitType Type => GameUnitType.IncomeBonus;
        public float BonusValue => _incomeBonusInPercents;
        public UnitReplacementInfo? ReplacementInfo { get; set; } = null;

        public void Reward(Profiler profiler)
        {
            profiler.ModifyIcomeFactor(_incomeBonusInPercents);
            IsReceived = true;
        }
    }
}
