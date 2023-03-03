using RaceManager.Cars;
using System.Collections.Generic;

namespace RaceManager.Progress
{
    public interface IProgressConditionInfo
    {
        public bool CanUpgradeCurrentCarFactors();
        public bool HasRankUpgradableCars();
        public bool HasUlockableCars();
        public bool HasIapSpecialOffer();
    }
}
