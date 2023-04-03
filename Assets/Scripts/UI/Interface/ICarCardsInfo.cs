using RaceManager.Cars;
using RaceManager.Progress;
using UnityEngine;

namespace RaceManager.UI
{
    public interface ICarCardsInfo
    { 
        public CarName CarName { get; }
        public Rarity CarRarity { get; }
        public int CardsAmount { get; }
        public UnitReplacementInfo? ReplacementInfo { get; }

        public Color CardColor { get; }
        public Sprite CarSprite { get; }
    }
}

