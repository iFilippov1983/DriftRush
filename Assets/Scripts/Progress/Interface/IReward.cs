using RaceManager.Root;

namespace RaceManager.Progress
{
    public interface IReward
    {
        RewardType Type { get; }
        void Reward(PlayerProfile playerProfile);
    }
}
