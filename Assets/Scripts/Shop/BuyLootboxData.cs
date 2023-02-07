using System;
using UnityEngine;

namespace RaceManager.Shop
{
    [Serializable]
    public struct BuyLootboxData
    {
        public Rarity lootboxRarity;
        public int costInGems;
        public Sprite lootboxSprite;
    }
}
