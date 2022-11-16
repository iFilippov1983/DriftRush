using Newtonsoft.Json;
using RaceManager.Cars;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RaceManager.Progress
{
    [Serializable]
    public class Currency
    {
        [JsonProperty]
        [SerializeField] 
        private int _money;

        [JsonProperty]
        [SerializeField] 
        private int _cups;

        [JsonProperty]
        [SerializeField] 
        private int _gems;

        public int Money
        { 
            get => _money;
            set 
            {
                float v = value * IncomeFactor;
                if(int.MaxValue > v && v > int.MinValue)
                    _money = (int)v;
            }
        }
        public int Cups;
        public int Gems;
        public float IncomeFactor = 1f;

        [DictionaryDrawerSettings(KeyLabel = "Car Name", ValueLabel = "Cards Amount")]
        public Dictionary<CarName, int> CarCards = new Dictionary<CarName, int>()
        {
            { CarName.ToyotaSupra, 0 },
            { CarName.FordMustang, 0 },
            { CarName.DodgeTRX, 0 },
            { CarName.NissanSilvia, 0 },
            { CarName.Porche911, 0 },
            { CarName.Ferrari488, 0 },
            { CarName.TeslaRoadster, 0 }
        };
    }
}
