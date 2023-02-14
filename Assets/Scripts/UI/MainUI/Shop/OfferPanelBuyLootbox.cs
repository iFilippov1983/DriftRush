using RaceManager.Shop;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace RaceManager.UI
{
    public class OfferPanelBuyLootbox : MonoBehaviour, IOfferPanel
    {
        [ShowInInspector, ReadOnly]
        private const ShopOfferType _type = ShopOfferType.BuyLootbox;
        private const int _panelsAmount = 3;

        [SerializeField]
        private PanelBuyLootbox[] _panelsArray = new PanelBuyLootbox[_panelsAmount];

        public ShopOfferType Type => _type;
        public GameObject GameObject => gameObject;
        public PanelBuyLootbox[] PanelsArray => _panelsArray;
        public int PanelsAmount => _panelsArray.Length;

        public void Accept(IOfferPanelInstaller installer)
        {
            installer.Install(this);
        }
    }
}

