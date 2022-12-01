using RaceManager.Cars;
using RaceManager.Race;
using RaceManager.Root;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace RaceManager.Progress
{
    public class RewardsHandler : MonoBehaviour
    {
        private GameProgressScheme _gameProgressScheme;
        private RaceRewardsScheme _raceRewardsScheme;

        private PlayerProfile _playerProfile;
        private Profiler _profiler;
        private SaveManager _saveManager;

        public Action<List<CarCardReward>> OnLootboxOpen;
        public Action<Lootbox> OnRaceRewardLootboxAdded;
        public Action OnProgressReward;

        [Inject]
        private void Construct(PlayerProfile playerProfile, Profiler profiler, SaveManager saveManager, GameProgressScheme gameProgressScheme, RaceRewardsScheme raceRewardsScheme)
        {
            _playerProfile = playerProfile;
            _profiler = profiler;
            _saveManager = saveManager;
            _gameProgressScheme = gameProgressScheme;
            _raceRewardsScheme = raceRewardsScheme;

            _profiler.OnLootboxOpen += HandleLootboxOpen;
        }

        public void RewardForRace(PositionInRace positionInRace, out RaceHandler.RaceRewardInfo info)
        {
            RaceReward reward = _raceRewardsScheme.GetRewardFor(positionInRace);
            reward.Reward(_profiler);

            if (_playerProfile.CanGetLootbox)
            {
                _profiler.CountVictory();

                Rarity rarity = Rarity.Common;
                bool isLucky = 
                    _playerProfile.WillGetLootboxForVictiories == false 
                    && 
                    _raceRewardsScheme.TryLuckWithNotCommonLootbox(out rarity);

                if (isLucky)
                {
                    Lootbox lootbox = new Lootbox(rarity);
                    _profiler.AddOrOpenLootbox(lootbox);

                    OnRaceRewardLootboxAdded?.Invoke(lootbox);
                }
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
            List<CarCardReward> list = lootbox.LootboxModel.GetCardsList();
            foreach (var reward in list)
            {
                _profiler.AddCarCards(reward.CarName, reward.CardsAmount);
            }

            OnLootboxOpen?.Invoke(list);
            _saveManager.Save();
        }

        private void OnDestroy()
        {
            _profiler.OnLootboxOpen -= HandleLootboxOpen;
        }
    }
}
