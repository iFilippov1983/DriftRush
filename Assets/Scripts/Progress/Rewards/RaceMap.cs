using RaceManager.Race;
using System;
using UnityEngine;

namespace RaceManager.Progress
{
    [Serializable]
    public class RaceMap : IReward
    {
        [SerializeField] private LevelName _levelName;

        public bool IsReceived { get; set; }
        public GameUnitType Type => GameUnitType.RaceMap;
        public LevelName LevelName => _levelName;
        public UnitReplacementInfo? ReplacementInfo { get; set; } = null;

        public void Reward(Profiler profiler)
        {
            profiler.SetNextLevelToLoad(_levelName);
            IsReceived = true;
        }
    }
}
