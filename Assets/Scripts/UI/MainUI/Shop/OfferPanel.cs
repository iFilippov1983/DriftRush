using RaceManager.Shop;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.UI
{
    public class OfferPanel : MonoBehaviour, IOfferPanel
    {
        [ShowInInspector, ReadOnly]
        private const ShopOfferType _type = ShopOfferType.None;

        [SerializeField] private GameObject[] _panels;

        private List<IOfferPanel> _offerPanels;

        public IOfferPanel GetOfferPanel(ShopOfferType type) => _offerPanels.Find(p => p.Type == type);
        public ShopOfferType Type => _type;

        public void Accept(IOfferPanelInstaller installer)
        {
            if (_offerPanels == null)
                MakePanelsList();

            foreach (var offerPanel in _offerPanels)
            {
                GameObject panelGo = _panels[_offerPanels.IndexOf(offerPanel)];
                bool fits = offerPanel.Type == installer.ShopOfferType;

                if (fits)
                {
                    offerPanel.Accept(installer);
                }

                panelGo.SetActive(fits);
            }
        }

        private void MakePanelsList()
        {
            _offerPanels = new List<IOfferPanel>();

            for (int i = 0; i < _panels.Length; i++)
            {
                IOfferPanel panel = _panels[i].GetComponent<IOfferPanel>();
                _offerPanels.Add(panel);
            }
        }
    }
}

