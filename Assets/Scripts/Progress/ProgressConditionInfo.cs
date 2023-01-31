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
            _carUpgradesHandler.CanUpgradeCurrentCarFactors() & _tutorialSteps.IsTutorialComplete;

        public bool TryGetRankUpgradableCars(out List<CarName> cars)
        {
            cars = new List<CarName>();
            bool hasUpgradeable = false;

            foreach (var profile in _playerCarDepot.CarProfiles)
            {
                var scheme = profile.RankingScheme;

                bool canUpgrade =
                    !scheme.CurrentRank.IsGranted
                    &
                    _playerProfile.Money > scheme.CurrentRank.AccessCost 
                    &
                    _playerProfile.CarCardsAmount(profile.CarName) > scheme.CurrentRank.PointsForAccess;

                if (canUpgrade)
                {
                    cars.Add(profile.CarName);
                    hasUpgradeable = true;
                } 
            }

            return hasUpgradeable;
        }

        public bool TryGetUlockableCars(out List<CarName> cars)
        {
            cars = new List<CarName>();
            bool hasUlockable = false;

            foreach (var profile in _playerCarDepot.CarProfiles)
            {
                var scheme = profile.RankingScheme;
                var curRank = scheme.CurrentRank;

                bool canUnlock = curRank.Rank == Rank.Rank_1 & curRank.IsReached;
            }

            return hasUlockable;
        }

        public bool TryGetIapSpecialOffer(out IReward reward)
        {
            throw new System.NotImplementedException();
        }
    }
}

