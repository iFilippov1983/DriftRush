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
            { CarName.ToyotaSupra, 0 },
            { CarName.FordMustang, 0 },
            { CarName.DodgeTrx, 0 },
            { CarName.NissanSilvia, 0 },
            { CarName.Porche911, 0 },
            { CarName.Ferrari488, 0 },
            { CarName.TeslaRoadster, 0 },
            { CarName.DodgeCharger, 0 }
        };
    }
}
