using Newtonsoft.Json;
using RaceManager.Cars;
using RaceManager.Progress;
using RaceManager.Race;
using System;
using System.Collections.Generic;
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
        private List<Lootbox> _lootboxes = new List<Lootbox>();

        [JsonProperty]
        [SerializeField]
        private List<LevelName> _availableLevels = new List<LevelName>();

        public PositionInRace LastInRacePosition = PositionInRace.DNF;
        public Currency Currency = new Currency();
        public LevelName NextLevelPrefabToLoad = LevelName.Level_0_test;

        [JsonProperty]
        private int _racesCounter = 0;

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

        public bool CanGetLootbox => _lootboxes.Count < LootboxesAmountMax;

        public void AddLevel(LevelName levelName) => _availableLevels.Add(levelName);

        public void CountRace() => RacesCounter++;

        public void AddLootbox(Rarity rarity)
        {
            Lootbox lootbox = new Lootbox(rarity);
            _lootboxes.Add(lootbox);
        }

        public Type DataType() => typeof(SaveData);

        public void Load(object data)
        {
            SaveData saveData = (SaveData)data;

            LastInRacePosition = saveData.lastInRacePosition;
            Currency = saveData.currency;
            NextLevelPrefabToLoad = saveData.nextLevelPrefabToLoad;
            RacesCounter = saveData.racesCounter;

            _availableLevels = saveData.availableLevels;

            foreach (var lootboxData in saveData.lootboxesData)
            {
                Lootbox lootbox = new Lootbox(lootboxData.Rarity)
                {
                    CurrentTimeToOpen = lootboxData.CurrentTimeToOpen
                };
                _lootboxes.Add(lootbox);
            }
        }

        public object Save()
        {
            var lootboxesDataList = new List<LootboxData>();

            foreach (var lootbox in _lootboxes)
            {
                lootboxesDataList.Add
                    (
                        new LootboxData()
                        {
                            Rarity = lootbox.LootboxModel.Rarity,
                            CurrentTimeToOpen = lootbox.CurrentTimeToOpen
                        }
                    );
            }

            return new SaveData
            {
                lastInRacePosition = LastInRacePosition,
                currency = Currency,
                nextLevelPrefabToLoad = NextLevelPrefabToLoad,
                racesCounter = RacesCounter,

                availableLevels = _availableLevels,
                lootboxesData = lootboxesDataList
            };
        }

        public class SaveData
        {
            public PositionInRace lastInRacePosition;
            public Currency currency;
            public LevelName nextLevelPrefabToLoad;
            public int racesCounter;

            public List<LevelName> availableLevels;
            public List<LootboxData> lootboxesData;
        }

        public class LootboxData
        { 
            public Rarity Rarity;
            public float CurrentTimeToOpen;
        }
    }
}
