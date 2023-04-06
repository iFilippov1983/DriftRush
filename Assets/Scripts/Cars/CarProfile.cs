using Newtonsoft.Json;
using RaceManager.Tools;
using Sirenix.OdinInspector;
using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace RaceManager.Cars
{
    [Serializable]
    public class CarProfile
    {
        [Title("***", titleAlignment: TitleAlignments.Centered)]
        public CarName CarName;
        public CarRankingScheme RankingScheme;
        public Speed Speed;
        public Handling Handling;
        public Acceleration Acceleration;
        public Friction Friction;
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
            public Rarity Rarity;

            [Space]
            [Title("Factors Amount")]
            [JsonProperty]
            [SerializeField]
            private int _factorsMaxTotal;
            [JsonProperty]
            [SerializeField]
            private int _initialFactorsProgress;
            [ReadOnly]
            public int FactorsMaxCurrent;
            public int CurrentFactorsProgress;

            [Space]
            [Title("Speed")]
            [ReadOnly]
            public int MinSpeedFactor = 1;
            public int MaxSpeedFactor;
            public int CurrentSpeedFactor;

            [Title("Handling")]
            [ReadOnly]
            public int MinHandlingFactor = 1;
            public int MaxHandlingFactor;
            public int CurrentHandlingFactor;

            [Title("Acceleration")]
            [ReadOnly]
            public int MinAccelerationFactor = 1;
            public int MaxAccelerationFactor;
            public int CurrentAccelerationFactor;

            [Title("Friction")]
            [ReadOnly]
            public int MinFrictionFactor = 1;
            public int MaxFrictionFactor;
            public int CurrentFrictionFactor;

            [Title("Durability")]
            [ReadOnly]
            public int MinDurabilityFactor = 1;
            public int MaxDurabilityFactor;
            public int CurrentDurabilityFactor;

            [ReadOnly, ShowInInspector]
            public int AvailableFactorsToUse => CurrentFactorsProgress - CurrentSpeedFactor - CurrentHandlingFactor - CurrentAccelerationFactor - CurrentFrictionFactor; //- CurrentDurabilityFactor

            public int FactorsMaxTotal => _factorsMaxTotal;
            public int InitialFactorsProgress => _initialFactorsProgress;
        }
    }
}
