using RaceManager.Cars;
using System;
using UnityEngine;

namespace RaceManager.Progress
{
    [Serializable]
    public class CarCardReward : IReward
    {
        [SerializeField] private CarName _carName;
        [SerializeField] private Rarity _carRarity;
        [SerializeField] private int _cardsAmount;

        public CarCardReward(CarName name, Rarity carRarity, int amount)
        {
            _carName = name;
            _carRarity = carRarity;
            _cardsAmount = amount;
        }

        //public bool IsValid { get; set; }
        public bool IsReceived { get; set; }
        public UnitReplacementInfo? ReplacementInfo { get; set; } = null;
        public GameUnitType Type => GameUnitType.CarCard;
        public CarName CarName => _carName;
        public Rarity Rarity => _carRarity;
        public int CardsAmount => _cardsAmount;
        

        public void Reward(Profiler profiler)
        {
            profiler.AddCarCards(_carName, _cardsAmount);
            IsReceived = true;
        }
    }
}
