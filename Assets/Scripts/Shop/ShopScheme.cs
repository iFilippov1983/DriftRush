using RaceManager.Root;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Shop
{
    [Serializable]
    [CreateAssetMenu(menuName = "Shop/ShopScheme", fileName = "ShopScheme", order = 1)]
    public class ShopScheme : SerializedScriptableObject, ISaveable
    {
        public bool GotSpecialOffer;

        [SerializeField]
        private List<OfferPanelInstaller> _installers = new List<OfferPanelInstaller>()
        { 
            new OfferPanelInstaller() { ShopOfferType = ShopOfferType.NoAds },
            new OfferPanelInstaller() { ShopOfferType = ShopOfferType.BuyLootbox },
            new OfferPanelInstaller() { ShopOfferType = ShopOfferType.ExchangeGems },
            new OfferPanelInstaller() { ShopOfferType = ShopOfferType.BuyGems }
        };

        public class SaveData { public bool gotSpecialOffer; }
        public Type DataType() => typeof(SaveData);
        public void Load(object data) => GotSpecialOffer = ((SaveData)data).gotSpecialOffer;
        public object Save() => new SaveData() { gotSpecialOffer = GotSpecialOffer };
    }
}
