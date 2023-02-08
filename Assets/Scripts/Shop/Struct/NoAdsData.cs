using System;
using UnityEngine;

namespace RaceManager.Shop
{
    [Serializable]
    public struct NoAdsData
    {
        public float offerCost;

        public int bonusAmount_1;
        public Sprite bonusSprite_1;

        public int bonusAmount_2;
        public Sprite bonusSprite_2;
    }
}
