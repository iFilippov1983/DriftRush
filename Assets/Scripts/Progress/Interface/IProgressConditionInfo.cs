using RaceManager.Cars;
using System.Collections.Generic;

namespace RaceManager.Progress
{
    public interface IProgressConditionInfo
    {
        public bool CanUpgradeCurrentCarFactors();
        public bool HasRankUpgradableCars(out List<CarName> cars);
        public bool HasUlockableCars(out List<CarName> cars);
        public bool HasIapSpecialOffer(out IReward reward);
    }
}
