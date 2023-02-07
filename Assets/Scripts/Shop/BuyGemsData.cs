using System;
using UnityEngine;

namespace RaceManager.Shop
{
    [Serializable]
    public struct BuyGemsData
    {
        public float offerCost;

        public int gemsAmount;
        public Sprite gemsAmountSprite;
    }
}
