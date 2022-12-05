using RaceManager.Root;
using RaceManager.Tools;
using UnityEngine;

namespace RaceManager.Race
{
    public class RaceLevelInitializer
    { 
        private IRaceLevel _raceLevel;
        private PlayerProfile _playerProfile;

        public IRaceLevel GetRaceLevel()
        {
            if (_raceLevel == null)
            {
                string path = _playerProfile.NextLevelPrefabToLoad.ToString();
                //Debug.Log("Prefab: " + _playerProfile.NextLevelPrefabToLoad);

                _raceLevel = InitializeLevel(path);
            }
            return _raceLevel;
        }

        public RaceLevelInitializer(PlayerProfile playerProfile)
        {
            _playerProfile = playerProfile;
        }

        private IRaceLevel InitializeLevel(string path)
        {
            IRaceLevel level = ResourcesLoader.LoadAndInstantiate<IRaceLevel>(path, new GameObject("Level").transform);

            if (level.GetType().Equals(typeof(RaceLevel)))
            {
                RaceLevel raceLevel = (RaceLevel)level;
                var configurations = raceLevel.Configurations;

                foreach (var trackConfiguration in configurations)
                    trackConfiguration.SetActive(false);

                var c = configurations[Random.Range(0, configurations.Count)];
                c.SetActive(true);

                foreach (var active in c.Actives)
                    active.SetActive(true);

                foreach (var inactive in c.Inactives)
                    inactive.SetActive(false);

                raceLevel.SetCurrentConfiguration(c);

                Debug.Log($"Race level configuration loaded => [{c.name}]");

                return raceLevel;
            }

            return level;
        }
    }
}