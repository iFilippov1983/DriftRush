using RaceManager.Cars;
using RaceManager.Race;
using RaceManager.Root;
using System;
using System.Collections.Generic;
using Zenject;

namespace RaceManager.Progress
{
    public class RewardsHandler : IDisposable
    {
        private GameProgressScheme _gameProgressScheme;
        private RaceRewardsScheme _raceRewardsScheme;
        private CarsDepot _carsDepot;

        private PlayerProfile _playerProfile;
        private Profiler _profiler;
        private SaveManager _saveManager;
        private GameEvents _gameEvents;

        private RaceReward _raceReward;

        public Action<int, List<CarCardReward>> OnLootboxOpen;
        public Action<CarCardReward> OnCarCardsReward;
        public Action<Lootbox> OnRaceRewardLootboxAdded;

        private int _moneyRewardDrift;
        private int _moneyRewardBump;
        private int _moneyRewardCrush;

        [Inject]
        private void Construct
            (
            PlayerProfile playerProfile, 
            Profiler profiler, 
            SaveManager saveManager, 
            GameProgressScheme gameProgressScheme, 
            RaceRewardsScheme raceRewardsScheme,
            CarsDepot carsDepot,
            GameEvents gameEvents
            )
        {
            _playerProfile = playerProfile;
            _profiler = profiler;
            _saveManager = saveManager;
            _gameProgressScheme = gameProgressScheme;
            _raceRewardsScheme = raceRewardsScheme;
            _carsDepot = carsDepot;
            _gameEvents = gameEvents;

            _profiler.OnLootboxOpen += HandleLootboxOpen;
        }

        public void RewardForRaceInit(PositionInRace positionInRace, out RaceRewardInfo info)
        {
            _raceReward = _raceRewardsScheme.GetRewardFor(positionInRace);

            info = new RaceRewardInfo()
            {
                MoneyRewardFinishPos = _raceReward.Money,
                MoneyRewardDrift = _moneyRewardDrift,
                MoneyRewardBump = _moneyRewardBump,
                MoneyRewardCrush = _moneyRewardCrush,
                MoneyMultiplyer = _raceRewardsScheme.MoneyMultiplyer,

                CupsRewardAmount = _raceReward.Cups,
                CupsTotalAmount = _playerProfile.Cups
            };

            int extraMoney = _moneyRewardDrift + _moneyRewardBump + _moneyRewardCrush;
            _raceReward.AddMoney(extraMoney);
            _raceReward.Reward(_profiler);

            if (positionInRace == PositionInRace.First)
            {
                _profiler.CountVictoryCycle();

                _gameEvents.RaceWin.OnNext();
            }

            if (_playerProfile.CanGetLootbox)
            {
                Rarity rarity = Rarity.Common;
                bool isLucky = 
                    _playerProfile.WillGetLootboxForVictiories == false 
                    && 
                    _raceRewardsScheme.TryLuckWithNotCommonLootbox(out rarity)
                    &&
                    _profiler.LootboxForRaceEnabled;

                if (isLucky)
                {
                    Lootbox lootbox = new Lootbox(rarity);
                    _profiler.AddOrOpenLootbox(lootbox);

                    OnRaceRewardLootboxAdded?.Invoke(lootbox);
                }
            }

            _saveManager.Save();
        }

        public void RewardForRaceMoneyMultiplyed()
        { 
            if(_raceReward is null)
                _raceReward = _raceRewardsScheme.GetRewardFor(PositionInRace.DNF);

            _raceReward.Unreward(_profiler);
            _raceReward.MultiplyMoney(_raceRewardsScheme.MoneyMultiplyer);
            _raceReward.Reward(_profiler);
        }

        public void RewardForProgress(int cupsAmountLevel)
        {
            ProgressStep step = _gameProgressScheme.GetStepWhithGoal(cupsAmountLevel);
            foreach (var reward in step.Rewards)
            {
                if (reward.Type == GameUnitType.CarCard)
                {
                    CarCardReward cardsReward = (CarCardReward)reward;
                    if (ValidCardsReward(cardsReward))
                    {
                        cardsReward.ReplacementInfo = null;
                        cardsReward.Reward(_profiler);
                    }
                    else
                    {
                        IReward r = ExchangeCards(cardsReward);
                        r?.Reward(_profiler);

                        cardsReward.ReplacementInfo = GetReplacementInfo(r);
                    }

                    OnCarCardsReward?.Invoke(cardsReward);
                    continue;
                }

                reward.Reward(_profiler);
            }

            _saveManager.Save();
        }

        public void SetMoneyReward(RaceScoresType type, int value)
        {
            switch (type)
            {
                case RaceScoresType.Drift:
                    _moneyRewardDrift = value;
                    break;
                case RaceScoresType.Bump:
                    _moneyRewardBump = value;
                    break;
                case RaceScoresType.Crush:
                    _moneyRewardCrush = value;
                    break;
                case RaceScoresType.Finish:
                default:
                    break;
            }
        }

        public void GrantInitialMoneyToPlayer()
        {
            bool canGrant = _gameProgressScheme.GrantMoneyOnStart && _profiler.MoneyHave == 0;

            if (canGrant)
            {
                _profiler.AddMoney(_gameProgressScheme.StartMoneyAmount);
                _saveManager.Save();
            }
        }

        private void HandleLootboxOpen(Lootbox lootbox)
        {
            List<CarCardReward> list = lootbox.CardsList;
            foreach (var reward in list)
            {
                if (ValidCardsReward(reward))
                {
                    reward.ReplacementInfo = null;
                    reward.Reward(_profiler);
                }
                else
                {
                    IReward r = ExchangeCards(reward);
                    r?.Reward(_profiler);

                    reward.ReplacementInfo = GetReplacementInfo(r);
                }
            }

            int lootboxMoney = UnityEngine.Random.Range(lootbox.MoneyAmountMin, lootbox.MoneyAmountMax + 1);
            int remain = lootboxMoney % 10;
            lootboxMoney -= remain;
            _profiler.AddMoney(lootboxMoney);

            OnLootboxOpen?.Invoke(lootboxMoney, list);
            _saveManager.Save();
        }

        private bool ValidCardsReward(CarCardReward reward)
        {
            var profile = _carsDepot.GetProfile(reward.CarName);
            int goalPoints = profile.RankingScheme.RankPointsTotalForCar;
            int curPoints = _playerProfile.CarCardsAmount(reward.CarName);
            bool allRanksAreReached = profile.RankingScheme.AllRanksReached;

            //Debug.Log($"Car name: {reward.CarName}; Goal: {goalPoints}; Current: {curPoints}; Valid: {goalPoints > curPoints}");

            return goalPoints > curPoints && !allRanksAreReached;
        }

        private IReward ExchangeCards(CarCardReward reward)
        {
            var eData = _gameProgressScheme.GetExchangeRateFor(reward.Type);

            IReward r = eData.AltRewardType switch
            {
                GameUnitType.Money => new Money(eData.OneUnitRate * reward.CardsAmount),
                _ => null,
            };

            return r;
        }

        private UnitReplacementInfo? GetReplacementInfo(IReward reward)
        {
            UnitReplacementInfo? info = reward.Type switch
            {
                GameUnitType.Money => new UnitReplacementInfo() { Type = GameUnitType.Money, Amount = ((Money)reward).MoneyAmount },
                GameUnitType.Gems => new UnitReplacementInfo() { Type = GameUnitType.Gems, Amount = ((Gems)reward).GemsAmount },
                _ => null,
            };

            return info;
        }

        public void Dispose()
        {
            _profiler.OnLootboxOpen -= HandleLootboxOpen;
        }
    }
}
