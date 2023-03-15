using UnityEngine;

namespace RaceManager.Progress
{
    [SerializeField]
    public class Cups : IReward
    {
        [SerializeField] private int _cups;

        public bool IsReceived { get; set; }
        public UnitReplacementInfo? ReplacementInfo { get; set; } = null;
        public GameUnitType Type => GameUnitType.Cups;
        public int CupsAmount => _cups;
        
        public void Reward(Profiler profiler)
        {
            profiler.AddCups(_cups);
            IsReceived = true;
        }
    }
}
