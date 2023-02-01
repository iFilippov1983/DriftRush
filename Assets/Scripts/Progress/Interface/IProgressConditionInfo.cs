using RaceManager.Cars;
using System.Collections.Generic;

namespace RaceManager.Progress
{
    public interface IProgressConditionInfo
    {
        public bool CanUpgradeCurrentCarFactors();
        public bool TryGetRankUpgradableCars(out List<CarName> cars);
        public bool TryGetUlockableCars(out List<CarName> cars);
        public bool TryGetIapSpecialOffer(out IReward reward);
    }
}
