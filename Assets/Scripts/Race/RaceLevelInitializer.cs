using RaceManager.Root;
using RaceManager.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Race
{
    public class RaceLevelInitializer
    { 
        private IRaceLevel _raceLevel;
        private IProfiler _profiler;

        public LevelName LevelName { get; private set; }

        public RaceLevelInitializer(IProfiler profiler)
        {
            _profiler = profiler;
        }

        public IRaceLevel GetRaceLevel()
        {
            if (_raceLevel == null)
            {
                LevelName = _profiler.NextLevelToLoad;
                string path = LevelName.ToString();

                _raceLevel = InitializeLevel(path);
            }
            return _raceLevel;
        }

        private IRaceLevel InitializeLevel(string path)
        {
            IRaceLevel level = ResourcesLoader.LoadAndInstantiate<IRaceLevel>(path, new GameObject("Level").transform);

            var configurations = level.Configurations;

            foreach (var trackConfiguration in configurations)
                trackConfiguration.SetActive(false);

            TrackConfiguration c = configurations[Random.Range(0, configurations.Count)];

            foreach (var active in c.Actives)
                active.SetActive(true);

            foreach (var inactive in c.Inactives)
                inactive.SetActive(false);

            c.SetActive(true);
            level.SetCurrentConfiguration(c);

            Debug.Log($"Race level configuration loaded => [{c.name}]");

            return level;

            //TODO: Refactoring is needed after Level version finnaly approved
            //if (level.GetType().Equals(typeof(RaceLevel)))
            //{
            //    RaceLevel raceLevel = (RaceLevel)level;
            //    var configurations = raceLevel.Configurations;

            //    foreach (var trackConfiguration in configurations)
            //        trackConfiguration.SetActive(false);

            //    TrackConfiguration c = configurations[Random.Range(0, configurations.Count)];

            //    foreach (var active in c.Actives)
            //        active.SetActive(true);

            //    foreach (var inactive in c.Inactives)
            //        inactive.SetActive(false);

            //    c.SetActive(true);
            //    raceLevel.SetCurrentConfiguration(c);

            //    Debug.Log($"Race level configuration loaded => [{c.name}]");

            //    return raceLevel;
            //}

            //return level;
        }
    }
}