using RaceManager.Shop;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class OfferPanelNoAds : MonoBehaviour, IOfferPanel
    {
        [Serializable]
        public class BonusContentView
        {
            public Image Image;
            public TMP_Text AmountText;
        }

        [ShowInInspector, ReadOnly]
        private const int DefaultBonusesAmount = 2;
        [ShowInInspector, ReadOnly]
        private const ShopOfferType _type = ShopOfferType.NoAds;

        [SerializeField] private IAPButton _buyButton;
        [SerializeField] private TMP_Text _costText;

        [SerializeField] private BonusContentView[] _bonusViews = new BonusContentView[DefaultBonusesAmount];

        public IAPButton BuyButton => _buyButton;
        public TMP_Text CostText => _costText;
        public BonusContentView[] BonusViews => _bonusViews;
        public ShopOfferType Type => _type;
        public GameObject GameObject => gameObject;

        public void Accept(IOfferPanelInstaller installer)
        {
            installer.Install(this);
        }
    }
}

