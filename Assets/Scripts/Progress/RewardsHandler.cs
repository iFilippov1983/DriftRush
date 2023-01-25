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

        public Action<List<CarCardReward>> OnLootboxOpen;
        public Action<Lootbox> OnRaceRewardLootboxAdded;
        public Action OnProgressReward;

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

        public void RewardForRace(PositionInRace positionInRace, out RaceHandler.RaceRewardInfo info)
        {
            RaceReward reward = _raceRewardsScheme.GetRewardFor(positionInRace);
            reward.Reward(_profiler);

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
                

            info = new RaceHandler.RaceRewardInfo()
            {
                RewardMoneyAmount = reward.Money,
                RewardCupsAmount = reward.Cups,
                MoneyTotal = _playerProfile.Money,
                GemsTotal = _playerProfile.Gems
            };
            _saveManager.Save();
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
