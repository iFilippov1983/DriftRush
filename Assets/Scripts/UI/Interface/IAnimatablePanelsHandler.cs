namespace RaceManager.UI
{
    public interface IAnimatablePanelsHandler
    {
        public void Handle(MoneyRewardPanel moneyRewardPanel);
        public void Handle(CupsRewardPanel cupsRewardPanel);
        public void Handle(LootboxRewardPanel lootboxRewardPanel);
    }
}

