using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Cars
{
    [Serializable]
    [CreateAssetMenu(menuName = "Cars/SpritesContainer", fileName = "SpritesContainer", order = 1)]
    public class SpritesContainer : ScriptableObject
    {
        [SerializeField]
        private List<CarSpriteHolder> _carSprites = new List<CarSpriteHolder>();

        public Sprite GetCarSprite(CarName carName)
        {
            CarSpriteHolder holder = _carSprites.Find(h => h.CarName == carName);
            return holder.Sprite;
        }

        [Serializable]
        public class CarSpriteHolder
        { 
            public CarName CarName;
            public Sprite Sprite;
        }
    }
}
