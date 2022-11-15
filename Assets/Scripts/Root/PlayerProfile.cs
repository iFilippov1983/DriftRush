using Newtonsoft.Json;
using RaceManager.Progress;
using RaceManager.Race;
using System;
using UnityEditor;

namespace RaceManager.Root
{
    [Serializable]
    public class PlayerProfile : ISaveable
    {
        private const int RacesCounterMax = 3;

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

        public void CountRace() => RacesCounter++;

        public class SaveData
        {
            public PositionInRace lastInRacePosition;
            public Currency currency;
            public LevelName nextLevelPrefabToLoad;
            public int racesCounter;
        }

        public Type DataType() => typeof(SaveData);

        public void Load(object data)
        {
            SaveData saveData = (SaveData)data;
            LastInRacePosition = saveData.lastInRacePosition;
            Currency = saveData.currency;
            NextLevelPrefabToLoad = saveData.nextLevelPrefabToLoad;
            RacesCounter = saveData.racesCounter;
        }

        public object Save()
        {
            return new SaveData
            {
                lastInRacePosition = LastInRacePosition,
                currency = Currency,
                nextLevelPrefabToLoad = NextLevelPrefabToLoad,
                racesCounter = RacesCounter
            };
        }
    }
}
