using RaceManager.Progress;
using Sirenix.OdinInspector;
using System;

namespace RaceManager.Shop
{
    [Serializable]
    public class ExchangeGemsData
    {
        [ReadOnly]
        public readonly GameUnitType Type = GameUnitType.Money;
        public int exchangeCostInGems;
        public int moneyAmount;
    }
}
