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

        [SerializeField]
        private PanelExchangeGems[] _panelsArray = new PanelExchangeGems[_panelsAmount];

        public ShopOfferType Type => _type;
        public GameObject GameObject => gameObject;
        public PanelExchangeGems[] PanelsArray => _panelsArray;
        public int PanelsAmount => _panelsArray.Length;

        public void Accept(IOfferPanelInstaller installer)
        {
            installer.Install(this);
        }
    }
}

