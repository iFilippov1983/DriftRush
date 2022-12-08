using RaceManager.Cars;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Progress
{
    [Serializable]
    [CreateAssetMenu(menuName = "Progress/CarsUpgradeScheme", fileName = "CarsUpgradeScheme", order = 1)]
    public class CarsUpgradeScheme : SerializedScriptableObject
    {
        [SerializeField]
        [DictionaryDrawerSettings(KeyLabel = "Car Rarity", ValueLabel = "Upgrades")]
        public Dictionary<Rarity, List<CarUpgrade>> UpgradesSceme = new Dictionary<Rarity, List<CarUpgrade>>();

        [Serializable]
        public class CarUpgrade
        {
            public Rank NecesseryRank;
            public int Price;
            public float StatsToAdd;
        }
    }
}

