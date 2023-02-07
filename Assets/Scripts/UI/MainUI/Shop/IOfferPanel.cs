using RaceManager.Shop;
using System;

namespace RaceManager.UI
{
    public interface IOfferPanel
    {
        public ShopOfferType Type { get; }
        public void Accept(IOfferPanelInstaller installer);
    }
}

