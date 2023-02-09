using RaceManager.Shop;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.UI
{
    public class ShopPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform _contentRect;
        [SerializeField] private List<GameObject> _panelsGoList;

        private List<IOfferPanel> _panels;

        public void InstallAllPanels(List<OfferPanelInstaller> installers)
        {
            if (_panels == null)
                MakePanelsList();

            try
            {
                foreach (var panel in _panels)
                {
                    var installer = installers.Find(i => i.ShopOfferType == panel.Type);
                    panel.Accept(installer);
                }
            }
            catch (Exception e)
            {
                $"Error: OfferPanels amount doesn't match with installers amount! || {e.Message}".Error();
            }
        }

        public bool TryRemovePanel(ShopOfferType offerType)
        {
            var panel = _panels.Find(p => p.Type == offerType);
            if (panel != null)
            {
                _panels.Remove(panel);
                Destroy(panel.GameObject);
                return true;
            }

            $"Panel with Type [{offerType}] was not found!".Log(Logger.ColorRed);
            return false;
        }

        private void MakePanelsList()
        {
            _panels = new List<IOfferPanel>();

            foreach (var go in _panelsGoList)
            {
                if (go.TryGetComponent(out IOfferPanel panel))
                {
                    _panels.Add(panel);
                }
            }
        }
    }
}

