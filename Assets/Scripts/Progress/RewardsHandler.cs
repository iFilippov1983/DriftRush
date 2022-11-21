using RaceManager.Cars;
using RaceManager.Race;
using RaceManager.Root;
using RaceManager.UI;
using System;
using UnityEngine;
using Zenject;

namespace RaceManager.Progress
{
    public class RewardsHandler : MonoBehaviour
    {
        private GameProgressScheme _gameProgressScheme;
        private RaceRewardsScheme _raceRewardsScheme;

        private PlayerProfile _playerProfile;
        private SaveManager _saveManager;

        public Action OnProgressReward;

        [Inject]
        private void Construct(PlayerProfile playerProfile, SaveManager saveManager, GameProgressScheme gameProgressScheme, RaceRewardsScheme raceRewardsScheme)
        {
            _playerProfile = playerProfile;
            _saveManager = saveManager;
            _gameProgressScheme = gameProgressScheme;
            _raceRewardsScheme = raceRewardsScheme;
        }

        public void RewardForRace(DriverProfile driverProfile, out RaceHandler.RaceRewardInfo info)
        {
            RaceReward reward = _raceRewardsScheme.GetRewardFor(driverProfile.PositionInRace);
            reward.Reward(_playerProfile);

            _playerProfile.CountRace();
            info = new RaceHandler.RaceRewardInfo()
            {
                RewardMoneyAmount = reward.Money,
                RewardCupsAmount = reward.Cups,
                MoneyTotal = _playerProfile.Currency.Money,
                GemsTotal = _playerProfile.Currency.Gems
            };

            _saveManager.Save();
            Debug.Log($"GOT REWARD - M:{reward.Money}; C:{reward.Cups} => NOW HAVE - M:{_playerProfile.Currency.Money}; C:{_playerProfile.Currency.Cups} => Race count: {_playerProfile.RacesCounter}");
        }

        public void RewardForProgress(int cupsAmountLevel)
        {
            ProgressStep step = _gameProgressScheme.GetStepWhithGoal(cupsAmountLevel);
            foreach (var reward in step.Rewards)
            {
                reward.Reward(_playerProfile);
                reward.IsReceived = true;
            }

            OnProgressReward?.Invoke();
            _saveManager.Save();
        }
    }
}
