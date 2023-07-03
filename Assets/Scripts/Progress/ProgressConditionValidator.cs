using RaceManager.Cars;
using RaceManager.Root;
//using Zenject;

namespace RaceManager.Progress
{
    public class ProgressConditionValidator : IProgressConditionInfo
    {
        private PlayerProfile _playerProfile;
        private TutorialSteps _tutorialSteps;
        private CarsDepot _playerCarDepot;
        private CarUpgradesHandler _carUpgradesHandler;

        private bool LastSceneWasRace => Loader.LastSceneName.Equals(Loader.Scene.RaceScene.ToString());

        public ProgressConditionValidator
            (
            PlayerProfile playerProfile,
            TutorialSteps tutorialSteps,
            CarsDepot playerCarDepot,
            CarUpgradesHandler carUpgradesHandler
            )
        {
            _playerProfile = playerProfile;
            _tutorialSteps = tutorialSteps;
            _playerCarDepot = playerCarDepot;
            _carUpgradesHandler = carUpgradesHandler;
        }

        //[Inject]
        //private void Construct
        //    (
        //    PlayerProfile playerProfile, 
        //    TutorialSteps tutorialSteps, 
        //    CarsDepot playerCarDepot, 
        //    CarUpgradesHandler carUpgradesHandler
        //    )
        //{
        //    _playerProfile = playerProfile;
        //    _tutorialSteps = tutorialSteps;
        //    _playerCarDepot = playerCarDepot;
        //    _carUpgradesHandler = carUpgradesHandler;
        //}

        public bool RemindersAllowed(int frequency) =>
            _tutorialSteps.IsTutorialComplete
            &&
            LastSceneWasRace
            &&
            _playerProfile.RacesTotalCounter % frequency == 0;


        public bool CanUpgradeCurrentCarFactors() =>
            _tutorialSteps.IsTutorialComplete
            &&
            LastSceneWasRace
            &&
            _carUpgradesHandler.CanUpgradeCurrentCarFactors()
            &&
            !_carUpgradesHandler.CurrentCarHasMaxFactorsUpgrade();

        public bool HasRankUpgradableCars()
        {
            bool hasUpgradeable = false;

            foreach (var profile in _playerCarDepot.ProfilesList)
            {
                var scheme = profile.RankingScheme;

                bool canUpgrade =
                    _tutorialSteps.IsTutorialComplete
                    &&
                    LastSceneWasRace
                    &&
                    !scheme.GetCurrentRank().IsGranted
                    &&
                    _playerProfile.Money > scheme.GetCurrentRank().AccessCost 
                    &&
                    _playerProfile.CarCardsAmount(profile.CarName) >= scheme.GetCurrentRank().PointsForAccess;

                if (canUpgrade)
                {
                    hasUpgradeable = true;
                } 
            }

            return hasUpgradeable;
        }

        public bool HasUlockableCars()
        {
            bool hasUlockable = false;

            foreach (var profile in _playerCarDepot.ProfilesList)
            {
                var scheme = profile.RankingScheme;
                var curRank = scheme.GetCurrentRank();

                bool canUnlock = 
                    _tutorialSteps.IsTutorialComplete
                    &&
                    LastSceneWasRace
                    &&
                    curRank.Rank == Rank.Rank_1 
                    && 
                    curRank.IsReached;

                if (canUnlock)
                {
                    hasUlockable = true;
                }
            }

            return hasUlockable;
        }

        public bool HasIapSpecialOffer()
        {
            //TODO: Implement function
            //reward = null;
            return false;
        }
    }
}

