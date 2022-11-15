using RaceManager.Cars;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

namespace RaceManager.Progress
{
    [Serializable]
    public class Currency
    {
        public int Money;
        public int Cups;
        public int Gems;

        [DictionaryDrawerSettings(KeyLabel = "Car Rarity", ValueLabel = "Cards Amount")]
        public Dictionary<Rarity, int> CarCards = new Dictionary<Rarity, int>()
        {
            { Rarity.Common, 0 },
            { Rarity.Uncommon, 0 },
            { Rarity.Rare, 0 },
            { Rarity.Epic, 0 },
            { Rarity.Legendary, 0 }
        };
    }
}
