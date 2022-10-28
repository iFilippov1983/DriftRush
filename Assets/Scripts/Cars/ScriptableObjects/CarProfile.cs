using Newtonsoft.Json;
using RaceManager.Tools;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Cars
{
    [Serializable]
    [CreateAssetMenu(menuName = "Cars/CarProfile", fileName = "CarProfile", order = 1)]
    public class CarProfile : ScriptableObject
    {
        public CarName CarName;
        public Characteristics CarCharacteristics;
        public CarConfig CarConfig;
        public CarVisualContainer CarVisualContainer;

        private GameObject _prefab;
        private Car _car;
        private CarVisual _carVisual;

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

        [JsonIgnore]
        public Car Car
        {
            get
            {
                if (_car == null)
                    _car = Prefab.GetComponent<Car>();
                return _car;
            }
        }

        [JsonIgnore]
        public CarVisual CarVisual
        {
            get
            {
                if (_carVisual == null)
                    _carVisual = Prefab.GetComponent<CarVisual>();
                return _carVisual;
            }
        }

        [Serializable]
        public class Characteristics
        {
            public CarRarity Rarity;
            public bool isAvailable;

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
            public int CurrentFactorsProgress => CurrentSpeedFactor + CurrentMobilityFactor + CurrentDurabilityFactor + CurrentAccelerationFactor;

            [ReadOnly, ShowInInspector]
            public int FactorsTotal => MaxSpeedFactor + MaxMobilityFactor + MaxDurabilityFactor + MaxAccelerationFactor;
        }
    }
}
