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

        public Sprite GetCarSprite(CarName carName)
        {
            CarSpriteHolder holder = _carSprites.Find(h => h.CarName == carName);
            return holder.CarSprite;
        }

        [Serializable]
        public class CarSpriteHolder
        { 
            public CarName CarName;
            public Sprite CarSprite;
        }
    }
}
