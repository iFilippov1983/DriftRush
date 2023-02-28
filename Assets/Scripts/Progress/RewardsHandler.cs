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

        private PlayerProfile _playerProfile;
        private Profiler _profiler;
        private SaveManager _saveManager;
        private GameEvents _gameEvents;

        private RaceReward _raceReward;

        public Action<List<CarCardReward>> OnLootboxOpen;
        public Action<Lootbox> OnRaceRewardLootboxAdded;
        public Action OnProgressReward;

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
            GameEvents gameEvents
            )
        {
            _playerProfile = playerProfile;
            _profiler = profiler;
            _saveManager = saveManager;
            _gameProgressScheme = gameProgressScheme;
            _raceRewardsScheme = raceRewardsScheme;
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

            if (_playerProfile.CanGetLootbox)
            {
                _profiler.CountVictoryCycle();

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

            if (positionInRace == PositionInRace.First)
            {
                _gameEvents.RaceWin.OnNext();
                Debugger.Log($"Victories count: {_playerProfile.VictoriesTotalCounter}");
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
                reward.Reward(_profiler);
            }

            OnProgressReward?.Invoke();
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

        private void HandleLootboxOpen(Lootbox lootbox)
        {
            List<CarCardReward> list = lootbox.CardsList;
            foreach (var reward in list)
            {
                _profiler.AddCarCards(reward.CarName, reward.CardsAmount);
            }

            OnLootboxOpen?.Invoke(list);
            _saveManager.Save();
        }

        public void Dispose()
        {
            _profiler.OnLootboxOpen -= HandleLootboxOpen;
        }
    }
}
