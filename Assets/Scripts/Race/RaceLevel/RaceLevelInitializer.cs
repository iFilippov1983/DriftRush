using RaceManager.Root;
using RaceManager.Tools;
using UnityEngine;

namespace RaceManager.Race
{
    public class RaceLevelInitializer
    { 
        //private IRaceLevel _raceLevel;
        private IProfiler _profiler;
        private IRaceLevelBuilder _builder;

        public LevelName LevelName { get; private set; }

        public RaceLevelInitializer(IProfiler profiler, IRaceLevelBuilder builder)
        {
            _profiler = profiler;
            _builder = builder;
        }

        public void MakeInitialLevel()
        {
            _builder.SetPrefab(_profiler.NextLevelToLoad.ToString());
            _builder.SetTrackConfigurations(Difficulty.Easy);
            _builder.ActivateAccessoryObjects(true);
            _builder.SetOpponents();
        }

        public void MakeCommonRaceRunLevel()
        { 
            _builder.SetPrefab( _profiler.NextLevelToLoad.ToString());
            _builder.SetTrackConfigurations();
            _builder.ActivateAccessoryObjects(false);
            _builder.SetOpponents(-1);
        }

        //public IRaceLevel GetRaceLevel()
        //{
        //    if (_raceLevel == null)
        //    {
        //        LevelName = _profiler.NextLevelToLoad;
        //        string path = LevelName.ToString();

        //        _raceLevel = InitializeLevel(path);
        //    }
        //    return _raceLevel;
        //}

        //private IRaceLevel InitializeLevel(string path)
        //{
        //    IRaceLevel level = ResourcesLoader.LoadAndInstantiate<IRaceLevel>(path, new GameObject("Level").transform);

        //    var configurations = level.Configurations;

        //    foreach (var trackConfiguration in configurations)
        //        trackConfiguration.SetActive(false);

        //    TrackConfiguration c = configurations[Random.Range(0, configurations.Count)];

        //    foreach (var active in c.Actives)
        //        active.SetActive(true);

        //    foreach (var inactive in c.Inactives)
        //        inactive.SetActive(false);

        //    c.SetActive(true);
        //    level.SetCurrentConfiguration(c);

        //    Debug.Log($"Race level configuration loaded => [{c.name}]");

        //    return level;
        //}
    }
}