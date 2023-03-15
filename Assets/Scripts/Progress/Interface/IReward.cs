namespace RaceManager.Progress
{
    public interface IReward
    {
        bool IsReceived { get; set; }
        GameUnitType Type { get; }
        UnitReplacementInfo? ReplacementInfo { get; }
        void Reward(Profiler profiler);
    }
}
