using RaceManager.Cars;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.UI
{
    [SerializeField]
    [CreateAssetMenu(menuName = "Containers/SpritesContainerCarCards", fileName = "SpritesContainerCarCards", order = 1)]
    public class SpritesContainerCarCards : SerializedScriptableObject
    {
        [SerializeField]
        private List<CardSpriteHolder> _spriteHolders = new List<CardSpriteHolder>();

        public Sprite GetCardSprite(CarName carName)
        {
            CardSpriteHolder holder = _spriteHolders.Find(x => x.CarName == carName);
            return holder.CardIcon;
        }

        [Serializable]
        public class CardSpriteHolder
        {
            public CarName CarName;
            public Sprite CardIcon;
        }
    }
}
