using Newtonsoft.Json;
using RaceManager.Cars;
using RaceManager.Progress;
using RaceManager.Race;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace RaceManager.Root
{
    [Serializable]
    public class PlayerProfile : ISaveable
    {
        public const int VictoriesCounterMax = 3;
        public const int LootboxesAmountMax = 4;

        [JsonProperty, ShowInInspector, ReadOnly]
        private int _victoriesCounter = 0;

        [JsonProperty, ShowInInspector, ReadOnly]
        private DateTime _lastSaveTime = DateTime.UtcNow;

        [JsonProperty, ShowInInspector, ReadOnly]
        private Currency _currency = new Currency();

        [JsonProperty, SerializeField]
        private LevelName _nextLevelPrefabToLoad = LevelName.Level_0_test;

        [JsonProperty, SerializeField]
        private PositionInRace _lastInRacePosition = PositionInRace.DNF;

        [JsonProperty, SerializeField]
        private List<Lootbox> _lootboxes = new List<Lootbox>();

        [JsonProperty, SerializeField]
        private List<LevelName> _availableLevels = new List<LevelName>();

        [JsonProperty]
        public int VictoriesCounter => _victoriesCounter;
        public int Money => _currency.Money;
        public int Gems => _currency.Gems;
        public int Cups => _currency.Cups;
        public float IncomeFactor => _currency.IncomeFactor;

        public DateTime LastSaveTime => _lastSaveTime;
        public LevelName NextLevelPrefabToLoad => _nextLevelPrefabToLoad;
        public PositionInRace LastInRacePosition => _lastInRacePosition;

        public bool CanGetLootbox => _lootboxes.Count < LootboxesAmountMax;
        public bool WillGetLootboxForVictiories => VictoriesCounter == VictoriesCounterMax;

        public void AddMoney(IProfiler profiler) => _currency.Money += profiler.Money;
        public void AddCups(IProfiler profiler) => _currency.Cups += profiler.Cups;
        public void AddGems(IProfiler profiler) => _currency.Gems += profiler.Gems;
        public void AddLootbox(IProfiler profiler) => _lootboxes.Add(profiler.LootboxToAdd);
        public void AddCards(IProfiler profiler) => _currency.CarCards[profiler.CarName] += profiler.CardsAmount;
        public void AddLevel(IProfiler profiler) => _availableLevels.Add(profiler.LevelName);

        public void SubstractMoney(IProfiler profiler) => _currency.Money -= profiler.MoneyCost;
        public void SubstractGems(IProfiler profiler) => _currency.Gems -= profiler.GemsCost;

        public void GiveLootboxesTo(IProfiler profiler) => profiler.SetLootboxList(_lootboxes);
        public void TakeLooboxesFrom(IProfiler profiler) => _lootboxes = profiler.Lootboxes;

        public void SetIcomeFactor(IProfiler profiler) => _currency.IncomeFactor = profiler.IncomeFactor;
        public void SetNextLevelFrom(IProfiler profiler) => _nextLevelPrefabToLoad = profiler.LevelName;
        public void SetLastInRacePosition(IProfiler profiler) => _lastInRacePosition = profiler.LastInRacePosition;
        public void SetVictoryCounter(IProfiler profiler) => _victoriesCounter = profiler.VictoriesCounter;

        public Type DataType() => typeof(SaveData);

        public void Load(object data)
        {
            SaveData saveData = (SaveData)data;

            _lastInRacePosition = saveData.lastInRacePosition;
            _currency = saveData.currency;
            _nextLevelPrefabToLoad = saveData.nextLevelPrefabToLoad;
            _victoriesCounter = saveData.racesCounter;

            _lastSaveTime = ParseDateTime(saveData.lastSaveDateTimeString, DateTime.UtcNow);
            _availableLevels = saveData.availableLevels;

            foreach (var lootboxData in saveData.lootboxesData)
            {
                Lootbox lootbox = new Lootbox(lootboxData.Id, lootboxData.Rarity, lootboxData.TimeToOpenLeft);
                lootbox.OpenTimerActivated = lootboxData.OpenTimerActivated;
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
                            Id = lootbox.Id,
                            Rarity = lootbox.LootboxModel.Rarity,
                            TimeToOpenLeft = lootbox.TimeToOpenLeft,
                            OpenTimerActivated = lootbox.OpenTimerActivated
                        }
                    );
            }

            _lastSaveTime = DateTime.UtcNow;

            return new SaveData
            {
                lastInRacePosition = LastInRacePosition,
                currency = _currency,
                nextLevelPrefabToLoad = NextLevelPrefabToLoad,
                racesCounter = VictoriesCounter,
                lastSaveDateTimeString = _lastSaveTime.ToString("u", CultureInfo.InvariantCulture),

                availableLevels = _availableLevels,
                lootboxesData = lootboxesDataList
            };
        }

        private DateTime ParseDateTime(string loadedLastSaveTime, DateTime defaultValue)
        {
            if (loadedLastSaveTime != string.Empty)
            {
                DateTime result = DateTime.ParseExact(loadedLastSaveTime, "u", CultureInfo.InvariantCulture);
                return result;
            }
            else
                return defaultValue;
        }

        public class SaveData
        {
            public PositionInRace lastInRacePosition;
            public Currency currency;
            public LevelName nextLevelPrefabToLoad;
            public int racesCounter;
            public string lastSaveDateTimeString;

            public List<LevelName> availableLevels;
            public List<LootboxData> lootboxesData;
        }

        public class LootboxData
        {
            public string Id;
            public Rarity Rarity;
            public float TimeToOpenLeft;
            public bool OpenTimerActivated;
        }
    }
}
