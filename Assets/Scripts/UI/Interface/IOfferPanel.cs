using RaceManager.Shop;
using System;
using UnityEngine;

namespace RaceManager.UI
{
    public interface IOfferPanel
    {
        public ShopOfferType Type { get; }
        public GameObject GameObject { get; }
        public void Accept(IOfferPanelInstaller installer);
    }
}

