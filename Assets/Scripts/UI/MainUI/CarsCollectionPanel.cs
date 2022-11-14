using RaceManager.Cars;
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
        private SpritesContainer _spritesContainer;
        private List<CollectionCard> _collectionCards = new List<CollectionCard>();

        public Button CloseButton => _closePanelButton;

        public GameObject CollectionCardPrefab
        {
            get 
            {
                if (_collectionCardPrefab == null)
                    _collectionCardPrefab = ResourcesLoader.LoadPrefab(ResourcePath.CollectionCardPrefab);

                return _collectionCardPrefab;
            }
        }

        public Action<string> OnUseCarButtonPressed;

        [Inject]
        private void Construct(SpritesContainer spritesContainer)
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
            card.CarNameText.text = carName.ToString();
            card.LockedImage.SetActive(!isAvailable);

            card.UseCarButton.interactable = isAvailable;
            card.UseCarButton.onClick.AddListener(() => OnUseCarButtonPressed?.Invoke(card.CarNameText.text));

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
                Debug.LogError($"Collection card whith name {carName} was not found");
            }
        }

        private void OnDestroy()
        {
            foreach (var card in _collectionCards)
                card.UseCarButton.onClick.RemoveAllListeners();
        }
    }
}

