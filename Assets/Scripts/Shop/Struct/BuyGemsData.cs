using RaceManager.Progress;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace RaceManager.Shop
{
    [Serializable]
    public class BuyGemsData
    {
        [ReadOnly]
        public readonly RewardType Type = RewardType.Gems;
        public float offerCost;
        public int gemsAmount;
    }
}
