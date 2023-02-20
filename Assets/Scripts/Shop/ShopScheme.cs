using RaceManager.Root;
using RaceManager.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Shop
{
    [Serializable]
    [CreateAssetMenu(menuName = "Shop/ShopScheme", fileName = "ShopScheme", order = 1)]
    public class ShopScheme : SerializedScriptableObject
    {
        [SerializeField] private SpritesContainerRewards _rewardSpritesContainer;

        [SerializeField]
        private List<OfferPanelInstaller> _installers = new List<OfferPanelInstaller>()
        { 
            new OfferPanelInstaller() { ShopOfferType = ShopOfferType.NoAds },
            new OfferPanelInstaller() { ShopOfferType = ShopOfferType.BuyLootbox },
            new OfferPanelInstaller() { ShopOfferType = ShopOfferType.ExchangeGems },
            new OfferPanelInstaller() { ShopOfferType = ShopOfferType.BuyGems }
        };

        public List<OfferPanelInstaller> Installers => _installers;

        public bool TryGetInstallerTypeOf(ShopOfferType type, out OfferPanelInstaller installer)
        {
            installer = _installers.Find(i => i.ShopOfferType == type);
            return installer != null;
        }
    }
}
