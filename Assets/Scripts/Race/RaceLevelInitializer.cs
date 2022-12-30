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

        public IRaceLevel GetRaceLevel()
        {
            if (_raceLevel == null)
            {
                List<LevelName> levels = _profiler.AvailableLevels;
                LevelName = levels[Random.Range(0, levels.Count)];
                string path = LevelName.ToString();

                _raceLevel = InitializeLevel(path);
            }
            return _raceLevel;
        }

        public RaceLevelInitializer(IProfiler profiler)
        {
            _profiler = profiler;
        }

        private IRaceLevel InitializeLevel(string path)
        {
            IRaceLevel level = ResourcesLoader.LoadAndInstantiate<IRaceLevel>(path, new GameObject("Level").transform);

            //TODO: Refactoring is needed after Level version finnaly approved
            if (level.GetType().Equals(typeof(RaceLevel)))
            {
                RaceLevel raceLevel = (RaceLevel)level;
                var configurations = raceLevel.Configurations;

                foreach (var trackConfiguration in configurations)
                    trackConfiguration.SetActive(false);

                TrackConfiguration c = configurations[Random.Range(0, configurations.Count)];

                foreach (var active in c.Actives)
                    active.SetActive(true);

                foreach (var inactive in c.Inactives)
                    inactive.SetActive(false);

                c.SetActive(true);
                raceLevel.SetCurrentConfiguration(c);

                Debug.Log($"Race level configuration loaded => [{c.name}]");

                return raceLevel;
            }

            return level;
        }
    }
}