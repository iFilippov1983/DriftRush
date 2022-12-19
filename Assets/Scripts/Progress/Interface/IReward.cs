namespace RaceManager.Progress
{
    public interface IReward
    {
        bool IsReceived { get; set; }
        RewardType Type { get; }
        void Reward(Profiler profiler);
    }
}
