﻿using RaceManager.Progress;
using RaceManager.Shop;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class ShopPanel : MonoBehaviour
    {
        [SerializeField] private Button _getFreeLootboxButton;
        [SerializeField] private RectTransform _contentRect;
        [SerializeField] private ShopConfirmationPanel _confirmationPanel;
        [SerializeField] private List<GameObject> _panelsGoList;

        private List<IOfferPanel> _panels;

        public Button GetFreeLootboxButton => _getFreeLootboxButton;
        public ShopConfirmationPanel ConfirmationPanel => _confirmationPanel;
        
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
                $"Error: OfferPanels amount doesn't match with installers amount! |or| {e.Message}".Error();
            }

            _confirmationPanel.BackButton.onClick.AddListener(DeactivateConfirmationPanel);
            _confirmationPanel.CloseWindowButton.onClick.AddListener(DeactivateConfirmationPanel);
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

        public void ActivateConfirmationPanel
            (
            RewardType type, 
            Sprite popupSprite, 
            string rewardText, 
            string rewardCostText
            )
        {
            foreach (var popup in _confirmationPanel.PopupsList)
            {
                if (popup.Type == type)
                {
                    popup.popupRect.SetActive(true);
                    popup.popupImage.sprite = popupSprite;
                    popup.rewardText.text = rewardText.ToUpper();
                    popup.rewardCost.text = rewardCostText.ToUpper();
                }
                else
                { 
                    popup.popupRect.SetActive(false);
                }
            }

            _confirmationPanel.SetActive(true);
        }

        public void DeactivateConfirmationPanel()
        {
            foreach (var popup in _confirmationPanel.PopupsList)
            {
               popup.popupRect.SetActive(false);
            }

            _confirmationPanel.SetActive(false);
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

