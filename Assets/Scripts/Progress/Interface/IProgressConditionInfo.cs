namespace RaceManager.Progress
{
    public interface IProgressConditionInfo
    {
        public bool RemindersAllowed(int frequency);
        public bool CanUpgradeCurrentCarFactors();
        public bool HasRankUpgradableCars();
        public bool HasUlockableCars();
        public bool HasIapSpecialOffer();
    }
}
