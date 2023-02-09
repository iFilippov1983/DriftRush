using RaceManager.UI;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace RaceManager.Shop
{
    [Serializable]
    public class OfferPanelInstaller : IOfferPanelInstaller
    {
        [Title("Panel Installer Settings")]
        [SerializeField] private ShopOfferType _type;

        [ShowIf("NoAdsInstaller")]
        [SerializeField] private NoAdsData _noAdsData;

        [ShowIf("BuyLootboxInstaller")]
        [SerializeField] private BuyLootboxData[] _buyLootboxData;

        [ShowIf("ExchangeGemsInstaller")]
        [SerializeField] private ExchangeGemsData[] _exchangeGemsData;

        [ShowIf("BuyGemsInstaller")]
        [SerializeField] private BuyGemsData[] _buyGemsData;

        public ShopOfferType ShopOfferType
        {
            get => _type;
            set => _type = value;
        }

        #region ShowIf Inspector Properties

        private bool NoAdsInstaller => _type == ShopOfferType.NoAds;
        private bool BuyLootboxInstaller => _type == ShopOfferType.BuyLootbox;
        private bool ExchangeGemsInstaller => _type == ShopOfferType.ExchangeGems;
        private bool BuyGemsInstaller => _type == ShopOfferType.BuyGems;

        #endregion

        public void Install(OfferPanelNoAds panelNoAds)
        {
            panelNoAds.CostText.text = "$" + _noAdsData.offerCost.ToString();

            panelNoAds.BonusAmountText_1.text = _noAdsData.bonusAmount_1.ToString();
            panelNoAds.BonusImage_1.sprite = _noAdsData.bonusSprite_1;

            panelNoAds.BonusAmountText_2.text = _noAdsData.bonusAmount_2.ToString();
            panelNoAds.BonusImage_2.sprite = _noAdsData.bonusSprite_2;
        }

        public void Install(OfferPanelBuyLootbox panelBuyLootbox)
        {
            if (panelBuyLootbox.PanelsAmount < _buyLootboxData.Length)
                Debug.LogWarning("The amount of BuyLootboxPanels is less then BuyLootboxData you assigned to ShopScheme!");

            if (panelBuyLootbox.PanelsAmount > _buyLootboxData.Length)
            {
                Debug.LogError("The amount of BuyLootboxPanels is more then BuyLootboxData you assigned to ShopScheme! Installation breaked!");
                return;
            }

            panelBuyLootbox.SetView(_buyLootboxData);
        }

        public void Install(OfferPanelExchangeGems panelExchangeGems)
        {
            if (panelExchangeGems.PanelsAmount < _exchangeGemsData.Length)
                Debug.LogWarning("The amount of ExchangeGemsPanels is less then ExchangeGemsData you assigned to ShopScheme!");

            if (panelExchangeGems.PanelsAmount > _exchangeGemsData.Length)
            {
                Debug.LogError("The amount of ExchangeGemsPanels is more then ExchangeGemsData you assigned to ShopScheme! Installation breaked!");
                return;
            }

            panelExchangeGems.SetView(_exchangeGemsData);
        }

        public void Install(OfferPanelBuyGems panelBuyGems)
        {
            if (panelBuyGems.PanelsAmount < _buyGemsData.Length)
                Debug.LogWarning("The amount of BuyGemsPanels is less then BuyGemsData you assigned to ShopScheme!");

            if (panelBuyGems.PanelsAmount > _buyGemsData.Length)
            {
                Debug.LogError("The amount of BuyGemsPanels is more then BuyGemsData you assigned to ShopScheme! Installation breaked!");
                return;
            }

            panelBuyGems.SetView(_buyGemsData);
        }
    }
}
