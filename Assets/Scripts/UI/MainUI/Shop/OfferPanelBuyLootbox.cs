﻿using RaceManager.Shop;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace RaceManager.UI
{
    public class OfferPanelBuyLootbox : MonoBehaviour, IOfferPanel
    {
        [ShowInInspector, ReadOnly]
        private const ShopOfferType _type = ShopOfferType.BuyLootbox;
        private const int _panelsAmount = 3;

        private PanelBuyLootbox[] _panelsArray;

        public PanelBuyLootbox BuyLootbox_1;
        public PanelBuyLootbox BuyLootbox_2;
        public PanelBuyLootbox BuyLootbox_3;

        /// <summary>
        /// Rarity: Lootbox rarity; int: Cost in gems;
        /// </summary>
        public UnityAction<Rarity, int> OnBuyLootboxButtonPressed;

        public ShopOfferType Type => _type;
        public GameObject GameObject => gameObject;
        public int PanelsAmount => _panelsAmount;

        public void Accept(IOfferPanelInstaller installer)
        {
            MakePanelsArray();
            installer.Install(this);
        }

        private void MakePanelsArray()
        {
            _panelsArray = new PanelBuyLootbox[_panelsAmount]
            {
                BuyLootbox_1,
                BuyLootbox_2,
                BuyLootbox_3
            };
        }

        public void SetView(params BuyLootboxData[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                PanelBuyLootbox panel = _panelsArray[i];
                BuyLootboxData d = data[i];

                panel.LooboxRarity = d.lootboxRarity;
                panel.LootboxImage.sprite = d.lootboxSprite;
                panel.CostText.text = d.costInGems.ToString();

                panel.BuyButton.onClick.AddListener(() => OnBuyLootboxButtonPressed?.Invoke(d.lootboxRarity, d.costInGems));
            }
        }
    }
}

