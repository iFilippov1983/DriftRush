using RaceManager.Progress;
using RaceManager.Root;
using RaceManager.Shop;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class ShopPanel : AnimatableSubject
    {
        [Space]
        [Header("Main Fields")]
        [SerializeField] private Button _getFreeLootboxButton;
        [SerializeField] private RectTransform _contentRect;
        [SerializeField] private ShopConfirmationPanel _confirmationPanel;
        [SerializeField] private List<GameObject> _panelsGoList;

        private List<IOfferPanel> _panels;

        public Button GetFreeLootboxButton => _getFreeLootboxButton;
        public ShopConfirmationPanel ConfirmationPanel => _confirmationPanel;

        private UIAnimator Animator => Singleton<UIAnimator>.Instance;
        
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
                Debug.LogError($"OfferPanels amount doesn't match with installers amount! |or| {e.Message}");
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

            Debug.LogWarning($"Panel with Type [{offerType}] was not found!");
            return false;
        }

        public bool TryGetPanelTransform(ShopOfferType offerType, out Transform panelTransform)
        {
            var panel = _panels.Find(p => p.Type == offerType);
            if (panel != null)
            {
                panelTransform = panel.GameObject.transform;
                return true;
            }

            Debug.LogWarning($"Panel with Type [{offerType}] was not found!");
            panelTransform = null;
            return false;
        }

        public void ActivateConfirmationPanel
            (
            GameUnitType type, 
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

            Animator.AppearSubject(_confirmationPanel).AddTo(this);
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

