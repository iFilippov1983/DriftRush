using RaceManager.Shop;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace RaceManager.UI
{
    public class OfferPanelExchangeGems : MonoBehaviour, IOfferPanel
    {
        [ShowInInspector, ReadOnly]
        private const ShopOfferType _type = ShopOfferType.ExchangeGems;
        [ShowInInspector, ReadOnly]
        private const int _panelsAmount = 3;

        private PanelExchangeGems[] _panelsArray;

        public PanelExchangeGems PanelExchangeGems_1;
        public PanelExchangeGems PanelExchangeGems_2;
        public PanelExchangeGems PanelExchangeGems_3;

        /// <summary>
        /// int 1: Money amount; int 2: Exchange cost in gems;
        /// </summary>
        public Action<int, int> OnExchangeGemsButtonPressed;

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
            _panelsArray = new PanelExchangeGems[_panelsAmount]
            { 
                PanelExchangeGems_1,
                PanelExchangeGems_2,
                PanelExchangeGems_3
            };
        }

        public void SetView(params ExchangeGemsData[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                PanelExchangeGems panel = _panelsArray[i];
                ExchangeGemsData d = data[i];

                panel.MoneyAmountImage.sprite = d.moneyAmountSprite;
                panel.MoneyAmountText.text = d.moneyAmount.ToString();
                panel.ExchangeCostText.text = d.exchangeCostInGems.ToString();

                panel.BuyButton.onClick.AddListener(() => OnExchangeGemsButtonPressed?.Invoke(d.moneyAmount, d.exchangeCostInGems));
            }
        }
    }
}

