using Newtonsoft.Json;
using RaceManager.Tools;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Cars
{
    [Serializable]
    public class CarProfile
    {
        public CarName CarName;
        public Speed Speed;
        public Mobility Mobility;
        public Acceleration Acceleration;
        public Durability Durability;
        public Characteristics CarCharacteristics;
        public CarConfig CarConfig;
        public CarConfigVisual CarConfigVisual;

        private GameObject _prefab;

        [JsonIgnore]
        public GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                    _prefab = ResourcesLoader.LoadPrefab(ResourcePath.CarPrefabsFolder + CarName.ToString());
                return _prefab;
            }
        }

        [Serializable]
        public class Characteristics
        {
            public CarRarity Rarity;
            public bool isAvailable;

            [JsonProperty]
            [SerializeField]
            private int _factorsMaxTotal;
            [JsonProperty]
            private int _availableFactorsToUse;

            public int CurrentFactorsProgress;

            [Space]
            [Title("Speed")]
            [ReadOnly]
            public int MinSpeedFactor = 1;
            public int MaxSpeedFactor;
            public int CurrentSpeedFactor;

            [Title("Mobility")]
            [ReadOnly]
            public int MinMobilityFactor = 1;
            public int MaxMobilityFactor;
            public int CurrentMobilityFactor;

            [Title("Durability")]
            [ReadOnly]
            public int MinDurabilityFactor = 1;  
            public int MaxDurabilityFactor;
            public int CurrentDurabilityFactor;

            [Title("Acceleration")]
            [ReadOnly]
            public int MinAccelerationFactor = 1;
            public int MaxAccelerationFactor;
            public int CurrentAccelerationFactor;

            [ReadOnly, ShowInInspector]
            public int AvailableFactorsToUse => CurrentFactorsProgress - CurrentSpeedFactor - CurrentMobilityFactor - CurrentDurabilityFactor - CurrentAccelerationFactor;
            public int FactorsMaxTotal => _factorsMaxTotal;
        }
    }
}
