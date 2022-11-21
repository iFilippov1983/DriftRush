using Google.Protobuf.WellKnownTypes;
using RaceManager.Root;

namespace RaceManager.Progress
{
    public interface IReward
    {
        bool IsReceived { get; set; }
        RewardType Type { get; }
        void Reward(PlayerProfile playerProfile);
    }
}
