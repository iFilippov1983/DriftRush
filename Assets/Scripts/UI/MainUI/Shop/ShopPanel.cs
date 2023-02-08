using RaceManager.Shop;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.UI
{
    public class ShopPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform _contentRect;
        [SerializeField] private List<OfferPanel> _offerPanels;

        private List<IOfferPanel> _panels = new List<IOfferPanel>();

        public void InstallAllPanels(List<OfferPanelInstaller> installers)
        {
            try
            {
                foreach (var panel in _offerPanels)
                {
                    var i = installers[_offerPanels.IndexOf(panel)];
                    panel.Accept(i);
                }
            }
            catch (Exception e)
            {
                $"Error: OfferPanels amount desn't match with installers amount! Or: {e.Message}".Error();
            }
        }

        public bool TryRemovePanel(ShopOfferType offerType)
        {
            var panel = _offerPanels.Find(p => p.Type == offerType);
            if (panel != null)
            {
                _panels.Remove(panel);
                Destroy(panel.GameObject);
                return true;
            }

            $"Panel with Type [{offerType}] was not found!".Log(Logger.ColorRed);
            return false;
        }
    }
}

