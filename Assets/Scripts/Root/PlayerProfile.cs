using Newtonsoft.Json;
using RaceManager.Progress;
using RaceManager.Race;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RaceManager.Root
{
    [Serializable]
    public class PlayerProfile : ISaveable
    {
        private const int RacesCounterMax = 3;
        private const int LootboxesAmountMax = 4;

        [JsonProperty]
        [SerializeField]
        private List<LootboxModel> _lootboxes = new List<LootboxModel>();

        [JsonProperty]
        [SerializeField]
        private List<LevelName> _availableLevels = new List<LevelName>();

        public PositionInRace LastInRacePosition = PositionInRace.DNF;
        public Currency Currency = new Currency();
        public LevelName NextLevelPrefabToLoad = LevelName.Level_0_test;

        [JsonProperty]
        private int _racesCounter = 0;
        [JsonProperty]
        private int _lootboxesCounter = 0;

        [JsonProperty]
        public int RacesCounter
        {
            get => _racesCounter;
            private set 
            { 
                _racesCounter = value;
                if(_racesCounter > RacesCounterMax)
                    _racesCounter = 1;
            }
        }

        public int LootboxesCounter { get => _lootboxesCounter; private set { _lootboxesCounter = value; }  }
        public bool CanGetLootbox => _lootboxes.Count < LootboxesAmountMax;

        public void AddLevel(LevelName levelName) => _availableLevels.Add(levelName);

        public void CountRace() => RacesCounter++;

        public void AddLootbox(LootboxModel lootbox)
        {
            if (CanGetLootbox)
            {
                _lootboxes.Add(lootbox);
                _lootboxesCounter++;
            }  
        }

        public List<LootboxModel> GetOpenLootboxes()
        { 
            var list = new List<LootboxModel>();
            foreach (var lootbox in _lootboxes)
            { 
                if (lootbox.IsOpen)
                {
                    list.Add(lootbox);
                    _lootboxesCounter--;
                }
                    
            }
            return list;
        }

        public class SaveData
        {
            public PositionInRace lastInRacePosition;
            public Currency currency;
            public LevelName nextLevelPrefabToLoad;
            public int racesCounter;

            public List<LootboxModel> lootboxes;
            public List<LevelName> availableLevels;
        }

        public Type DataType() => typeof(SaveData);

        public void Load(object data)
        {
            SaveData saveData = (SaveData)data;
            LastInRacePosition = saveData.lastInRacePosition;
            Currency = saveData.currency;
            NextLevelPrefabToLoad = saveData.nextLevelPrefabToLoad;
            RacesCounter = saveData.racesCounter;
            LootboxesCounter = saveData.lootboxes.Count;

            _lootboxes = saveData.lootboxes;
            _availableLevels = saveData.availableLevels;
        }

        public object Save()
        {
            return new SaveData
            {
                lastInRacePosition = LastInRacePosition,
                currency = Currency,
                nextLevelPrefabToLoad = NextLevelPrefabToLoad,
                racesCounter = RacesCounter,

                lootboxes = _lootboxes,
                availableLevels = _availableLevels
            };
        }
    }
}
