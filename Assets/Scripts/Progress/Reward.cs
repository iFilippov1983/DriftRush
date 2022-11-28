using RaceManager.Cars;
using RaceManager.Race;
using System;
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

        public void Reward(Profiler profiler)
        {
            profiler.AddMoney(Money);
            profiler.AddCups(Cups);
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

        public void Reward(Profiler profiler)
        {
            profiler.AddMoney(MoneyAmount, true);
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

        public void Reward(Profiler profiler)
        {
            profiler.AddCups(_cups);
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

        public void Reward(Profiler profiler)
        {
            profiler.AddGems(_gems);
            IsReceived = true;
        }
    }

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
        public RewardType Type => RewardType.Lootbox;
        public Rarity Rarity => _rarity;

        public void Reward(Profiler profiler)
        {
            Lootbox lootbox = new Lootbox(_rarity, _timeToOpen);
            profiler.AddOrOpenLootbox(lootbox);
            IsReceived = true;
        }
    }

    [Serializable]
    public class CarCardReward : IReward
    {
        [SerializeField] private CarName _carName;
        [SerializeField] private int _cardsAmount;

        public CarCardReward()
        {
            _carName = CarName.ToyotaSupra;
            _cardsAmount = 0;
        }

        public CarCardReward(CarName name, int amount)
        {
            _carName = name;
            _cardsAmount = amount;
            IsReceived = true;
        }

        public bool IsReceived { get; set; }
        public RewardType Type => RewardType.CarCard;
        public CarName CarName => _carName;
        public int CardsAmount => _cardsAmount;

        public void Reward(Profiler profiler)
        {
            profiler.AddCarCards(_carName, _cardsAmount);
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

        public void Reward(Profiler profiler)
        {
            profiler.AddLevel(_levelName);
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

        public void Reward(Profiler profiler)
        {
            profiler.ModifyIcomeFactor(_incomeBonusInPercents);
            IsReceived = true;
        }
    }
}
