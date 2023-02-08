using RaceManager.Shop;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace RaceManager.UI
{
    public class OfferPanelBuyGems : MonoBehaviour, IOfferPanel
    {
        [ShowInInspector, ReadOnly]
        private const ShopOfferType _type = ShopOfferType.BuyGems;
        [ShowInInspector, ReadOnly]
        private const int _panelsAmount = 6;

        private PanelBuyGems[] _panelsArray;

        public PanelBuyGems BuyGems_1;
        public PanelBuyGems BuyGems_2;
        public PanelBuyGems BuyGems_3;
        public PanelBuyGems BuyGems_4;
        public PanelBuyGems BuyGems_5;
        public PanelBuyGems BuyGems_6;

        public Action<string> OnPurchaseButtonClicked;

        public ShopOfferType Type => _type;
        public GameObject GameObject => gameObject;
        public int PanelsAmount => _panelsAmount;
        
        public void Accept(IOfferPanelInstaller installer)
        {
            MakePanelsArray();
            installer.Install(this);
        }

        private void MakePanelsArray()
        {
            _panelsArray = new PanelBuyGems[_panelsAmount] 
            { 
                BuyGems_1, 
                BuyGems_2, 
                BuyGems_3, 
                BuyGems_4, 
                BuyGems_5, 
                BuyGems_6 
            };
        }

        public void SetView(params BuyGemsData[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                PanelBuyGems panel = _panelsArray[i];
                BuyGemsData d = data[i];

                panel.GemsImage.sprite = d.gemsAmountSprite;
                panel.GemsAmountText.text = d.gemsAmount.ToString();
                panel.CostText.text = "$" + d.offerCost.ToString();
            }
        }
    }
}

