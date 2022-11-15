using RaceManager.Root;
using RaceManager.Tools;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

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
                    //string path = string.Concat(ResourcePath.LevelsPrefabsFolder, _playerProfile.nextLevelPrefabToLoad);
                    //string path = string.Concat(ResourcePath.LevelsPrefabsFolder, "Level_0_test");
                    string path = _playerProfile.NextLevelPrefabToLoad.ToString();
                    Debug.Log("Prefab: " + _playerProfile.NextLevelPrefabToLoad);

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