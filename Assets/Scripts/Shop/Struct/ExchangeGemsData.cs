using RaceManager.Progress;
using Sirenix.OdinInspector;
using System;

namespace RaceManager.Shop
{
    [Serializable]
    public class ExchangeGemsData
    {
        [ReadOnly]
        public readonly RewardType Type = RewardType.Money;
        public int exchangeCostInGems;
        public int moneyAmount;
    }
}
