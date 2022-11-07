using RaceManager.Root;
using RaceManager.Tools;
using UnityEngine;

namespace RaceManager.Race
{
    public class RaceLevelInitializer
    { 
        private RaceLevelView _raceLevelView;
        private PlayerProfile _playerProfile;

        public RaceLevelView RaceLevel
        {
            get
            {
                if (_raceLevelView == null)
                {
                    string path = string.Concat(ResourcePath.LevelsPrefabsFolder, _playerProfile.nextLevelPrefabToLoad);

                    Debug.Log("Prefab: " + _playerProfile.nextLevelPrefabToLoad);

                    _raceLevelView = InitializeLevel(path);
                }
                return _raceLevelView;
            }
        }

        public RaceLevelInitializer(PlayerProfile playerProfile)
        {
            _playerProfile = playerProfile;
        }

        private RaceLevelView InitializeLevel(string path)
        {
            return ResourcesLoader.LoadAndInstantiate<RaceLevelView>(path, new GameObject("Level").transform);
        }
    }
}