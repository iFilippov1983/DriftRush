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
        private const int _defaultPanelsAmount = 6;

        public PanelBuyGems[] _panelsArray = new PanelBuyGems[_defaultPanelsAmount];

        public ShopOfferType Type => _type;
        public GameObject GameObject => gameObject;
        public PanelBuyGems[] PanelsArray => _panelsArray;
        public int PanelsAmount => _panelsArray.Length;
        
        public void Accept(IOfferPanelInstaller installer)
        {
            installer.Install(this);
        }
    }
}

