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
        public float IncomeFactor = 1f;

        [DictionaryDrawerSettings(KeyLabel = "Car Name", ValueLabel = "Cards Amount")]
        public Dictionary<CarName, int> CarCards = new Dictionary<CarName, int>()
        {
            { CarName.SuperBull, 0 },
            { CarName.BlinkGoat, 0 },
            { CarName.HyperWolf, 0 },
            { CarName.ThaurenG86, 0 },
            { CarName.ChivalryS1, 0 },
            { CarName.Mosquito_3, 0 },
            { CarName.BearRod, 0 },
            { CarName.StealthRider, 0 }
        };
    }
}
