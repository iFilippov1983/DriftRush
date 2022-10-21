using RaceManager.Tools;
using UnityEngine;

namespace RaceManager.Race
{
    public class RaceLevelInitializer
    { 
        private RaceLevelView _raceLevelView;
        
        public RaceLevelView RaceLevel => _raceLevelView;


        //TODO: take loading instructions
        public RaceLevelInitializer()
        {
            string path = ResourcePath.Level_0_test;
            _raceLevelView = InitializeLevel(path);
        }

        private RaceLevelView InitializeLevel(string path)
        {
            return ResourcesLoader.LoadAndInstantiate<RaceLevelView>(path, new GameObject("Level").transform);
        }
    }
}