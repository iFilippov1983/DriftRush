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
        [Space]
        [SerializeField] private CarWindow _carWindow;

        private GameObject _collectionCardPrefab;
        private SpritesContainerCarCollection _spritesCars;
        private SpritesContainerCarCards _spritesCarCards;

        private List<CollectionCard> _collectionCards = new List<CollectionCard>();

        public Action<CarName> OnUseCar;
        public Action OnCarWindowOpen;

        public CarName UsedCarName { get; set; }
        public CarWindow CarWindow => _carWindow;
        public Button CloseButton => _closePanelButton;
        public Button CarWindowUpgradeButton => _carWindow.UpgradeButton;
        public Button CarWindowBackButton => _carWindow.BackButton;

        private GameObject CollectionCardPrefab
        {
            get 
            {
                if (_collectionCardPrefab == null)
                    _collectionCardPrefab = ResourcesLoader.LoadPrefab(ResourcePath.CollectionCardPrefab);

                return _collectionCardPrefab;
            }
        }

        [Inject]
        private void Construct(SpritesContainerCarCollection spritesCars, SpritesContainerCarCards spritesCarCards)
        { 
            _spritesCars = spritesCars;
            _spritesCarCards = spritesCarCards;
        }

        [Title("TEST")]
        [Button]
        public void AddCollectionCard(CarName carName, int progressCurrent, int progressTotal, bool isAvailable)
        {
            GameObject cardGo = Instantiate(CollectionCardPrefab, _collectionContent.transform, false);
            CollectionCard card = cardGo.GetComponent<CollectionCard>();

            card.BackgroundImage.sprite = _spritesCars.GetCarSprite(carName);
            card.ProgressCurrentText.text = progressCurrent.ToString();
            card.ProgressTotalText.text = progressTotal.ToString();
            card.CashedCarName = carName;
            card.CarNameText.text = carName.ToString();
            card.LockedImage.SetActive(!isAvailable);

            card.ProgressImage.fillAmount = progressTotal == 0
                ? 1f
                : (float)progressCurrent / (float)progressTotal;

            card.UseCarButton.interactable = isAvailable;
            card.UseCarButton.onClick.AddListener(() => HandleClick(card));

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
                card.ProgressCurrentText.text = progressCurrent.ToString();
                card.ProgressTotalText.text = progressTotal.ToString();
                card.LockedImage.SetActive(!isAvailable);
                card.UseCarButton.interactable = isAvailable;

                card.ProgressImage.fillAmount = progressTotal == 0
                ? 1f
                : (float)progressCurrent / (float)progressTotal;
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

        public void SetCarWindow(int upgradeCost, bool upgraded, bool canUpgrade)
        {
            _carWindow.UpgradeCostText.text = upgradeCost.ToString();

            _carWindow.UpgradeButton.SetActive(!upgraded);
            _carWindow.UpgradeButton.interactable = canUpgrade;
        }

        private void HandleClick(CollectionCard card)
        {
            bool openCarWindow = card.CashedCarName == UsedCarName;

            if (openCarWindow)
            {
                OnCarWindowOpen.Invoke();

                Sprite spriteCards = _spritesCarCards.GetCardSprite(card.CashedCarName);

                _carWindow.CardsImage.sprite = spriteCards;
                _carWindow.ProgressBarImage.fillAmount = card.ProgressImage.fillAmount;
                _carWindow.CardsProgressText.text = $"{card.ProgressCurrentText.text}/{card.ProgressTotalText.text}";
            }
            else
            {
                OnUseCar?.Invoke(card.CashedCarName);
                UsedCarName = card.CashedCarName;
            }
        }
    }
}

