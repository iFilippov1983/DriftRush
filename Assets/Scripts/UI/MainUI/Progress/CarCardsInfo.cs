using RaceManager.Cars;
using RaceManager.Progress;
using UnityEngine;

namespace RaceManager.UI
{
    public struct CarCardsInfo : ICarCardsInfo
    {
        public CarName CarName { get; set; }
        public Rarity CarRarity { get; set; }
        public int CardsAmount { get; set; }
        public UnitReplacementInfo? ReplacementInfo { get; set; }
        public Color CardColor { get; set; }
        public Sprite CarSprite { get; set; }
    }
}

