using RaceManager.Progress;
using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace RaceManager.Shop
{
    [Serializable]
    public class NoAdsData
    {
        [Serializable]
        public struct BonusContent
        {
            public RewardType Type;
            public int Amount;
        }

        public float offerCost;

        public List<BonusContent> BonusContents = new List<BonusContent>();
    }
}
