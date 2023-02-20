using RaceManager.UI;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

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

        /// <summary>
        /// string = Button name
        /// </summary>
        public Action<string> OnButtonClicked;

        public bool SpecialOfferGranted { get; set; }
        public ShopCore ShopCore { get; set; }
        public SpritesContainerRewards SpritesContainer { get; set; }
        public ShopOfferType ShopOfferType
        {
            get => _type;
            set => _type = value;
        }
        public NoAdsData NoAdsData => _noAdsData;

        #region ShowIf Inspector Properties

        private bool NoAdsInstaller => _type == ShopOfferType.NoAds;
        private bool BuyLootboxInstaller => _type == ShopOfferType.BuyLootbox;
        private bool ExchangeGemsInstaller => _type == ShopOfferType.ExchangeGems;
        private bool BuyGemsInstaller => _type == ShopOfferType.BuyGems;

        #endregion

        public void Install(OfferPanelNoAds panelNoAds)
        {
            if (SpecialOfferGranted)
            {
                Object.Destroy(panelNoAds.GameObject);
                return;
            }

            panelNoAds.CostText.text = "$" + _noAdsData.offerCost.ToString();

            for (int i = 0; i < panelNoAds.BonusViews.Length; i++)
            {
                var panel = panelNoAds.BonusViews[i];
                var content = _noAdsData.BonusContents[i];

                panel.AmountText.text = content.Amount.ToString();
                panel.Image.sprite = SpritesContainer.GetShopSprite(content.Type, content.Amount);
            }
        }

        public void Install(OfferPanelBuyLootbox panelBuyLootbox)
        {
            if (panelBuyLootbox.PanelsAmount < _buyLootboxData.Length)
            {
                Debug.LogWarning("The amount of BuyLootboxPanels is less then BuyLootboxData you assigned to ShopScheme!");
            }
            else if (panelBuyLootbox.PanelsAmount > _buyLootboxData.Length)
            {
                Debug.LogError("The amount of BuyLootboxPanels is more then BuyLootboxData you assigned to ShopScheme! Installation breaked!");
                return;
            }

            for (int i = 0; i < panelBuyLootbox.PanelsArray.Length; i++)
            {
                var panel = panelBuyLootbox.PanelsArray[i];
                var data = _buyLootboxData[i];

                panel.CostText.text = data.costInGems.ToString();
                panel.RarityText.text = data.lootboxRarity.ToString().ToUpper();
                panel.LooboxRarity = data.lootboxRarity;
                panel.LootboxImage.sprite = SpritesContainer.GetLootboxSprite(data.lootboxRarity);

                panel.BuyButton.onClick.AddListener(() => ShopCore.OnTryBuyLootbox?.Invoke(data.costInGems, data.lootboxRarity));
                panel.BuyButton.onClick.AddListener(() => OnButtonClicked?.Invoke(panel.BuyButton.name));
            }
        }

        public void Install(OfferPanelExchangeGems panelExchangeGems)
        {
            if (panelExchangeGems.PanelsAmount < _exchangeGemsData.Length)
            {
                Debug.LogWarning("The amount of ExchangeGemsPanels is less then ExchangeGemsData you assigned to ShopScheme!");
            }
            else if (panelExchangeGems.PanelsAmount > _exchangeGemsData.Length)
            {
                Debug.LogError("The amount of ExchangeGemsPanels is more then ExchangeGemsData you assigned to ShopScheme! Installation breaked!");
                return;
            }

            for (int i = 0; i < panelExchangeGems.PanelsArray.Length; i++)
            {
                var panel = panelExchangeGems.PanelsArray[i];
                var data = _exchangeGemsData[i];

                panel.ExchangeCostText.text = data.exchangeCostInGems.ToString();
                panel.MoneyAmountText.text = data.moneyAmount.ToString();
                panel.MoneyAmountImage.sprite = SpritesContainer.GetShopSprite(data.Type, data.moneyAmount);

                panel.ExchangeButton.onClick.AddListener(() => ShopCore.OnTryGemsExchange?.Invoke(data.exchangeCostInGems, data.moneyAmount));
                panel.ExchangeButton.onClick.AddListener(() => OnButtonClicked?.Invoke(panel.ExchangeButton.name));
            }
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

            for (int i = 0; i < panelBuyGems.PanelsArray.Length; i++)
            {
                var panel = panelBuyGems.PanelsArray[i];
                var data = _buyGemsData[i];

                panel.CostText.text = "$" + data.offerCost.ToString();
                panel.GemsAmountText.text = data.gemsAmount.ToString();
                panel.GemsImage.sprite = SpritesContainer.GetShopSprite(data.Type, data.gemsAmount);
            }
        }
    }
}
