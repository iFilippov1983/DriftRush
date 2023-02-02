using RaceManager.Cars;
using RaceManager.Root;
using System.Collections.Generic;
using Zenject;

namespace RaceManager.Progress
{
    public class ProgressConditionInfo : IProgressConditionInfo
    {
        private PlayerProfile _playerProfile;
        private TutorialSteps _tutorialSteps;
        private CarsDepot _playerCarDepot;
        private CarUpgradesHandler _carUpgradesHandler;

        [Inject]
        private void Construct(PlayerProfile playerProfile, TutorialSteps tutorialSteps, CarsDepot playerCarDepot, CarUpgradesHandler carUpgradesHandler)
        {
            _playerProfile = playerProfile;
            _tutorialSteps = tutorialSteps;
            _playerCarDepot = playerCarDepot;
            _carUpgradesHandler = carUpgradesHandler;
        }

        public bool CanUpgradeCurrentCarFactors() =>
            _tutorialSteps.IsTutorialComplete
            &&
            _carUpgradesHandler.CanUpgradeCurrentCarFactors();

        public bool HasRankUpgradableCars(out List<CarName> cars)
        {
            cars = new List<CarName>();
            bool hasUpgradeable = false;

            foreach (var profile in _playerCarDepot.CarProfiles)
            {
                var scheme = profile.RankingScheme;

                bool canUpgrade =
                    _tutorialSteps.IsTutorialComplete
                    &&
                    !scheme.CurrentRank.IsGranted
                    &&
                    _playerProfile.Money > scheme.CurrentRank.AccessCost 
                    &&
                    _playerProfile.CarCardsAmount(profile.CarName) > scheme.CurrentRank.PointsForAccess;

                if (canUpgrade)
                {
                    cars.Add(profile.CarName);
                    hasUpgradeable = true;
                } 
            }

            return hasUpgradeable;
        }

        public bool HasUlockableCars(out List<CarName> cars)
        {
            cars = new List<CarName>();
            bool hasUlockable = false;

            foreach (var profile in _playerCarDepot.CarProfiles)
            {
                var scheme = profile.RankingScheme;
                var curRank = scheme.CurrentRank;

                bool canUnlock = 
                    _tutorialSteps.IsTutorialComplete
                    &&
                    curRank.Rank == Rank.Rank_1 
                    && 
                    curRank.IsReached;

                if (canUnlock)
                {
                    cars.Add(profile.CarName);
                    hasUlockable = true;
                }
            }

            return hasUlockable;
        }

        public bool HasIapSpecialOffer(out IReward reward)
        {
            //TODO: Implement function
            reward = null;
            return false;
        }
    }
}

