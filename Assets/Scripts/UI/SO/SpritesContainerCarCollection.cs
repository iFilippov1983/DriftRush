using RaceManager.Cars;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.UI
{
    [Serializable]
    [CreateAssetMenu(menuName = "Containers/SpritesContainerCarCollection", fileName = "SpritesContainerCarCollection", order = 1)]
    public class SpritesContainerCarCollection : SerializedScriptableObject
    {
        [SerializeField]
        private List<CarSpriteHolder> _carSprites = new List<CarSpriteHolder>();
        [Space(20)]

        [SerializeField]
        private List<CarRarityColor> _cardColors = new List<CarRarityColor>();

        public Sprite GetCarSprite(CarName carName, bool locked = false)
        {
            CarSpriteHolder holder = _carSprites.Find(h => h.CarName == carName);

            Sprite sprite = locked
                ? holder.CarLockedSprite
                : holder.CarSprite;

            return sprite;
        }

        public Color GetCarRarityColor(Rarity carRarity)
        {
            CarRarityColor color = _cardColors.Find(h => h.CarRarity == carRarity);
            return color.RarityColor;
        }

        [Serializable]
        public class CarSpriteHolder
        { 
            public CarName CarName;
            public Sprite CarSprite;
            public Sprite CarLockedSprite;
        }

        [Serializable]
        public class CarRarityColor
        {
            public Rarity CarRarity;
            public Color RarityColor;
        }
    }
}
