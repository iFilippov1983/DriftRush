using RaceManager.Shop;
using System;
using UnityEngine;

namespace RaceManager.UI
{
    [Serializable]
    public abstract class OfferPanel : MonoBehaviour, IOfferPanel
    {
        //[ShowInInspector, ReadOnly]
        //private const ShopOfferType _type = ShopOfferType.None;

        //[SerializeField] private GameObject[] _panelsGoArray;

        //private List<IOfferPanel> _offerPanels;

        //public IOfferPanel GetOfferPanel(ShopOfferType type) => _offerPanels.Find(p => p.Type == type);
        //public ShopOfferType Type => _type;
        public abstract ShopOfferType Type { get; }
        public abstract GameObject GameObject { get; }
        public abstract void Accept(IOfferPanelInstaller installer);

        //public void Accept(IOfferPanelInstaller installer)
        //{
        //    if (_offerPanels == null)
        //        MakePanelsList();

        //    foreach (var offerPanel in _offerPanels)
        //    {
        //        GameObject panelGo = _panelsGoArray[_offerPanels.IndexOf(offerPanel)];
        //        bool fits = offerPanel.Type == installer.ShopOfferType;

        //        if (fits)
        //        {
        //            offerPanel.Accept(installer);
        //        }

        //        Debug.Log($"Panel: {gameObject.name} => Offer: {panelGo.name} => Active: {fits}");
        //        panelGo.SetActive(fits);
        //    }
        //}

        //private void MakePanelsList()
        //{
        //    _offerPanels = new List<IOfferPanel>();

        //    for (int i = 0; i < _panelsGoArray.Length; i++)
        //    {
        //        IOfferPanel panel = _panelsGoArray[i].GetComponent<IOfferPanel>();
        //        _offerPanels.Add(panel);
        //    }
        //}
    }
}

