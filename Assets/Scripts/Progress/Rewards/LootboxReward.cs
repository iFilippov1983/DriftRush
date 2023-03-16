using System;
using UnityEngine;

namespace RaceManager.Progress
{
    [Serializable]
    public class LootboxReward : IReward
    {
        [SerializeField] private Rarity _rarity;
        [SerializeField] private float _timeToOpen;

        public LootboxReward(Rarity rarity, float timeToOpen)
        {
            _rarity = rarity;
            _timeToOpen = timeToOpen;
        }

        public bool IsReceived { get; set; }
        public GameUnitType Type => GameUnitType.Lootbox;
        public Rarity Rarity => _rarity;
        public UnitReplacementInfo? ReplacementInfo { get; set; } = null;

        public void Reward(Profiler profiler)
        {
            Lootbox lootbox = new Lootbox(_rarity, _timeToOpen);
            profiler.AddOrOpenLootbox(lootbox);
            IsReceived = true;
        }
    }
}
