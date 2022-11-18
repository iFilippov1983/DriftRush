using RaceManager.Cars;
using RaceManager.Race;
using RaceManager.Root;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Progress
{
    [Serializable]
    public class ProgressReward : IReward
    {
        [SerializeField] private List<IReward> _rewards = new List<IReward>();

        public RewardType Type => RewardType.ProgressReward;

        public void Reward(PlayerProfile playerProfile)
        {
            foreach (var reward in _rewards)
                reward.Reward(playerProfile);
        }
    }

    [Serializable]
    public class RaceReward : IReward
    {
        [SerializeField] private int _money;
        [SerializeField] private int _cups;

        public int Money => _money;
        public int Cups => _cups;

        public RewardType Type => RewardType.RaceReward;

        public void Reward(PlayerProfile playerProfile)
        {
            playerProfile.Currency.Money = Money;
            playerProfile.Currency.Cups = Cups;
        }
    }

    [Serializable]
    public class Money : IReward
    {
        [SerializeField] private int _money;

        public RewardType Type => RewardType.Money;

        public void Reward(PlayerProfile playerProfile)
        {
            playerProfile.Currency.Money = _money;
        }
    }

    [SerializeField]
    public class Cups : IReward
    {
        [SerializeField] private int _cups;

        public RewardType Type => RewardType.Cups;

        public void Reward(PlayerProfile playerProfile)
        {
            playerProfile.Currency.Cups = _cups;
        }
    }

    [Serializable]
    public class Gems : IReward
    {
        [SerializeField] private int _gems;

        public RewardType Type => RewardType.Gems;

        public void Reward(PlayerProfile playerProfile)
        {
            playerProfile.Currency.Gems = _gems;
        }
    }

    [Serializable]
    public class LootboxReward : IReward
    {
        [SerializeField] private LootboxModel _lootbox;

        public RewardType Type => RewardType.Lootbox;

        public void Reward(PlayerProfile playerProfile)
        {
            playerProfile.AddLootbox(_lootbox);
        }
    }

    [Serializable]
    public class CarCard : IReward
    {
        [SerializeField] private CarName _carName;
        [SerializeField] private int _cardsAmount;

        public CarName CarName => _carName;
        public int CardsAmount => _cardsAmount;

        public CarCard(CarName name, int amount)
        {
            _carName = name;
            _cardsAmount = amount;
        }

        public RewardType Type => RewardType.CarCard;

        public void Reward(PlayerProfile playerProfile)
        {
            playerProfile.Currency.CarCards[_carName] += _cardsAmount;
        }
    }

    [Serializable]
    public class RaceMap : IReward
    {
        [SerializeField] private LevelName _levelName;

        public RewardType Type => RewardType.RaceMap;

        public void Reward(PlayerProfile playerProfile)
        {
            playerProfile.AddLevel(_levelName);
        }
    }

    [Serializable]
    public class IncomeBonus : IReward
    {
        [Tooltip("Percentage of income to add to income factor")]
        [SerializeField] private float _incomeBonusInPercents;

        public RewardType Type => RewardType.IncomeBonus;

        public void Reward(PlayerProfile playerProfile)
        {
            playerProfile.Currency.IncomeFactor += _incomeBonusInPercents / 100f;
        }
    }
}
