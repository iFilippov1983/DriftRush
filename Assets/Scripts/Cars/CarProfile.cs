using Newtonsoft.Json;
using RaceManager.Tools;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 649
namespace RaceManager.Cars
{
    [Serializable]
    public class CarProfile
    {
        public CarName CarName;
        public AccessibilityProgress Accessibility;
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
        public class AccessibilityProgress
        {
            [JsonProperty]
            [SerializeField] private int _pointsToAccess = 100;
            [JsonProperty]
            [SerializeField] private int _currentPointsAmount = 0;

            public int PointsToAccess => _pointsToAccess;
            public int CurrentPointsAmount => _currentPointsAmount;

            public void AddProgressPoints(int value)
            { 
                _currentPointsAmount += value;
                if(_currentPointsAmount > _pointsToAccess)
                    _currentPointsAmount = _pointsToAccess;
            }

            [ShowInInspector]
            public bool IsAvailable
            {
                get => _currentPointsAmount == _pointsToAccess;
                set 
                { 
                    if(value == true)
                        _currentPointsAmount = _pointsToAccess;
                }
            }
        }

        [Serializable]
        public class Characteristics
        {
            public Rarity Rarity;
            //public bool isAvailable;

            [JsonProperty]
            [SerializeField]
            private int _factorsMaxTotal;

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
