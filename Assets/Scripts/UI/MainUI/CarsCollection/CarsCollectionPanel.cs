﻿using RaceManager.Cars;
using RaceManager.Tools;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace RaceManager.UI
{
    public class CarsCollectionPanel : MonoBehaviour
    {
        [SerializeField] private Button _closePanelButton;
        [Space]
        [SerializeField] private TMP_Text _carNameText;
        [SerializeField] private TMP_Text _carStatsProgressText;
        [Space]
        [SerializeField] private GridLayoutGroup _collectionContent;
        [SerializeField] private RectTransform _collectionContentRect;

        private GameObject _collectionCardPrefab;
        private SpritesContainerCarCollection _spritesContainer;

        private List<CollectionCard> _collectionCards = new List<CollectionCard>();

        private GameObject CollectionCardPrefab
        {
            get 
            {
                if (_collectionCardPrefab == null)
                    _collectionCardPrefab = ResourcesLoader.LoadPrefab(ResourcePath.CollectionCardPrefab);

                return _collectionCardPrefab;
            }
        }

        public Button CloseButton => _closePanelButton;
        public Action<CarName> OnUseCarButtonPressed;

        [Inject]
        private void Construct(SpritesContainerCarCollection spritesContainer)
        { 
            _spritesContainer = spritesContainer;
        }

        [Button]
        public void AddCollectionCard(CarName carName, int progressCurrent, int progressTotal, bool isAvailable)
        {
            GameObject cardGo = Instantiate(CollectionCardPrefab, _collectionContent.transform, false);
            CollectionCard card = cardGo.GetComponent<CollectionCard>();

            card.BackgroundImage.sprite = _spritesContainer.GetCarSprite(carName);
            card.ProgressImage.fillAmount = (float)progressCurrent / (float)progressTotal;
            card.ProgressCurrentText.text = progressCurrent.ToString();
            card.ProgressTotalText.text = progressTotal.ToString();
            card.CashedCarName = carName;
            card.CarNameText.text = carName.ToString();
            card.LockedImage.SetActive(!isAvailable);

            card.UseCarButton.interactable = isAvailable;
            card.UseCarButton.onClick.AddListener(() => OnUseCarButtonPressed?.Invoke(card.CashedCarName));

            _collectionCards.Add(card);

            Rect rect = _collectionContentRect.rect;
            rect.height = _collectionContent.cellSize.y * (_collectionCards.Count / 2 + 1);
            _collectionContentRect.rect.SetHeight(rect.height);
        }

        public void UpdateCard(CarName carName, int progressCurrent, int progressTotal, bool isAvailable)
        {
            CollectionCard card = _collectionCards.Find(c => c.CarNameText.text == carName.ToString());
            if (card != null)
            {
                card.ProgressImage.fillAmount = (float)progressCurrent / (float)progressTotal;
                card.ProgressCurrentText.text = progressCurrent.ToString();
                card.ProgressTotalText.text = progressTotal.ToString();
                card.LockedImage.SetActive(!isAvailable);
                card.UseCarButton.interactable = isAvailable;
            }
            else
            {
                Debug.LogError($"Collection card whith Car name '{carName}' was not found");
            }
        }

        public void UpdateStatsProgress(string carName, int currentValue, int maxValue)
        {
            _carNameText.text = carName;
            _carStatsProgressText.text = $"{currentValue}/{maxValue}";
        }

        private void OnDestroy()
        {
            foreach (var card in _collectionCards)
                card.UseCarButton.onClick.RemoveAllListeners();
        }
    }
}
