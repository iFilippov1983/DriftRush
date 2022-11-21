using RaceManager.Cars;
using RaceManager.Race;
using RaceManager.Root;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Progress
{
    [Serializable]
    public class RaceReward : IReward
    {
        [SerializeField] private int _money;
        [SerializeField] private int _cups;

        public bool IsReceived { get; set; }
        public int Money => _money;
        public int Cups => _cups;
        public RewardType Type => RewardType.RaceReward;

        public void Reward(PlayerProfile playerProfile)
        {
            playerProfile.Currency.Money += Money;
            playerProfile.Currency.Cups += Cups;
            IsReceived = true;
        }
    }

    [Serializable]
    public class Money : IReward
    {
        [SerializeField] private int _money;

        public bool IsReceived { get; set; }
        public RewardType Type => RewardType.Money;
        public int MoneyAmount => _money;

        public void Reward(PlayerProfile playerProfile)
        {
            playerProfile.Currency.Money += _money;
            IsReceived = true;
        }
    }

    [SerializeField]
    public class Cups : IReward
    {
        [SerializeField] private int _cups;

        public bool IsReceived { get; set; }
        public RewardType Type => RewardType.Cups;
        public int CupsAmount => _cups;

        public void Reward(PlayerProfile playerProfile)
        {
            playerProfile.Currency.Cups += _cups;
            IsReceived = true;
        }
    }

    [Serializable]
    public class Gems : IReward
    {
        [SerializeField] private int _gems;

        public bool IsReceived { get; set; }
        public RewardType Type => RewardType.Gems;
        public int GemsAmount => _gems;

        public void Reward(PlayerProfile playerProfile)
        {
            playerProfile.Currency.Gems += _gems;
            IsReceived = true;
        }
    }

    [Serializable]
    public class LootboxReward : IReward
    {
        [SerializeField] private Rarity _rarity;

        public bool IsReceived { get; set; }
        public RewardType Type => RewardType.Lootbox;
        public Rarity Rarity => _rarity;

        public void Reward(PlayerProfile playerProfile)
        {
            playerProfile.AddLootbox(_rarity);
            IsReceived = true;
        }
    }

    [Serializable]
    public class CarCard : IReward
    {
        [SerializeField] private CarName _carName;
        [SerializeField] private int _cardsAmount;

        public CarCard()
        {
            _carName = CarName.ToyotaSupra;
            _cardsAmount = 0;
        }

        public CarCard(CarName name, int amount)
        {
            _carName = name;
            _cardsAmount = amount;
            IsReceived = true;
        }

        public bool IsReceived { get; set; }
        public RewardType Type => RewardType.CarCard;
        public CarName CarName => _carName;
        public int CardsAmount => _cardsAmount;

        public void Reward(PlayerProfile playerProfile)
        {
            playerProfile.Currency.CarCards[_carName] += _cardsAmount;
            IsReceived = true;
        }
    }

    [Serializable]
    public class RaceMap : IReward
    {
        [SerializeField] private LevelName _levelName;

        public bool IsReceived { get; set; }
        public RewardType Type => RewardType.RaceMap;
        public LevelName LevelName => _levelName;

        public void Reward(PlayerProfile playerProfile)
        {
            playerProfile.AddLevel(_levelName);
            IsReceived = true;
        }
    }

    [Serializable]
    public class IncomeBonus : IReward
    {
        [Tooltip("Percentage of income to add to income factor")]
        [SerializeField] private float _incomeBonusInPercents;

        public bool IsReceived { get; set; }
        public RewardType Type => RewardType.IncomeBonus;
        public float BonusValue => _incomeBonusInPercents;

        public void Reward(PlayerProfile playerProfile)
        {
            playerProfile.Currency.IncomeFactor += _incomeBonusInPercents / 100f;
            IsReceived = true;
        }
    }
}
