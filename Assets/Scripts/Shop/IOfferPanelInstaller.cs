using RaceManager.UI;

namespace RaceManager.Shop
{
    public interface IOfferPanelInstaller
    {
        public ShopOfferType ShopOfferType { get; set; }
        public void Install(OfferPanelNoAds panelNoAds);
        public void Install(OfferPanelBuyLootbox panelBuyLootbox);
        public void Install(OfferPanelExchangeGems panelExchangeGems);
        public void Install(OfferPanelBuyGems panelBuyGems);
    }
}
