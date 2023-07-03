using RaceManager.Cars;
using RaceManager.Tools;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace RaceManager.UI
{
    public class CarsCollectionPanel : AnimatableSubject
    {
        [Space(20)]
        [Header("Main Fields")]
        [SerializeField] private Button _closePanelButton;
        [SerializeField] private Button _closePanelWindowButton;
        [Space]
        [SerializeField] private TMP_Text _carNameText;
        [SerializeField] private TMP_Text _carStatsProgressText;
        [Space]
        [SerializeField] private GridLayoutGroup _collectionContent;
        [SerializeField] private RectTransform _collectionContentRect;
        [Space]
        [SerializeField] private CarWindow _carWindow;

        private GameObject _collectionCardPrefab;
        private SpritesContainerCarCollection _spritesCarsCollection;

        private List<CollectionCard> _collectionCards = new List<CollectionCard>();

        public Action<CarName> OnUseCar;
        public Action OnCarWindowOpen;
        public Action<Button> OnButtonPressed;

        public CarName UsedCarName { get; set; }
        public CarWindow CarWindow => _carWindow;
        public Button CloseButton => _closePanelButton;
        public Button ClosePanelWindowButton => _closePanelWindowButton;
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
        private void Construct(SpritesContainerCarCollection spritesCars)
        { 
            _spritesCarsCollection = spritesCars;
        }

        [Title("TEST")]
        [Button]
        public void AddCollectionCard
            (
            CarName carName, 
            Rarity carRarity, 
            int factorsProgress,
            int progressCurrent, 
            int progressTotal, 
            bool isAvailable, 
            bool noGoal)
        {
            GameObject cardGo = Instantiate(CollectionCardPrefab, _collectionContent.transform, false);
            CollectionCard card = cardGo.GetComponent<CollectionCard>();

            Color rarityColor = _spritesCarsCollection.GetCarRarityColor(carRarity);
            card.FrameImage.color = rarityColor;
            //card.CardImage.color = rarityColor;
            card.CarImage.sprite = _spritesCarsCollection.GetCarSprite(carName);
            card.LockedImage.sprite = _spritesCarsCollection.GetCarSprite(carName, true);
            
            string name = carName.ToString().SplitByUppercaseWith(" ");
            name = name.Replace('_', ' ');

            card.CarNameText.text = name.ToUpper();
            card.CashedCarName = carName;
            card.LockedImage.SetActive(!isAvailable);

            string text = noGoal
                ? "MAX"
                : $"{progressCurrent}/{progressTotal}";

            card.ProgressText.text = text;
            card.PartsAmountText.text = factorsProgress.ToString();

            card.ProgressImage.fillAmount = noGoal || progressCurrent > progressTotal
                ? 1f
                : (float)progressCurrent / (float)progressTotal;

            card.UseCarButton.interactable = isAvailable;
            card.UseCarButton.onClick.AddListener(() => HandleClick(card));
            card.UseCarButton.onClick.AddListener(() => OnButtonPressedMethod(card.UseCarButton));

            _collectionCards.Add(card);

            Rect rect = _collectionContentRect.rect;
            rect.height = _collectionContent.cellSize.y * (_collectionCards.Count / 2 + 1);
            _collectionContentRect.rect.SetHeight(rect.height);
        }

        public void UpdateCard(CarName carName, int factorsProgress, int cardsProgressCurrent, int progressTotal, bool isAvailable, bool noGoal)
        {
            CollectionCard card = _collectionCards.Find(c => c.CashedCarName == carName);

            card.LockedImage.SetActive(!isAvailable);
            card.UseCarButton.interactable = isAvailable;

            string text = noGoal
                ? "MAX"
                : $"{cardsProgressCurrent}/{progressTotal}";

            card.ProgressText.text = text;
            card.PartsAmountText.text = factorsProgress.ToString();

            card.ProgressImage.fillAmount = noGoal || cardsProgressCurrent > progressTotal
                ? 1f
                : (float)cardsProgressCurrent / (float)progressTotal;

            UpdateCarWindow(card);
        }

        public void UpdateStatsProgress(string carName, int currentValue) //, int maxValue)
        {
            string name = carName.SplitByUppercaseWith(" ");
            name = name.Replace('_', ' ');

            _carNameText.text = name.ToUpper();
            _carStatsProgressText.text = $"{currentValue}"; //  /{maxValue}";
        }

        public void SetCarWindow(int upgradeCost, bool upgraded, bool isAvailable, bool hasFunds)
        {
            _carWindow.UpgradeCostText.text = upgradeCost.ToString();

            _carWindow.UpgradeButton.SetActive(!upgraded);
            _carWindow.UpgradeButton.interactable = isAvailable && hasFunds;
        }

        public async void OpenCarWindow(CarName carName)
        {
            CollectionCard card = _collectionCards.Find(c => c.CashedCarName == carName);

            await Task.Delay((int)(Settings.appearAnimDuration * 500));
            card.UseCarButton.onClick?.Invoke();
            card.UseCarButton.onClick?.Invoke();
        }

        private void HandleClick(CollectionCard card)
        {
            bool openCarWindow = card.CashedCarName == UsedCarName;

            if (openCarWindow)
            {
                OnCarWindowOpen?.Invoke();

                UpdateCarWindow(card);
            }
            else
            {
                OnUseCar?.Invoke(card.CashedCarName);
                UsedCarName = card.CashedCarName;
            }
        }

        private void UpdateCarWindow(CollectionCard card)
        {
            //Sprite spriteCards = _spritesCarCards.GetCardSprite(card.CashedCarName);

            //_carWindow.CardsImage.sprite = spriteCards;
            //_carWindow.CardsImage.color = card.FrameImage.color;
            //_carWindow.CardsProgressText.color = card.FrameImage.color;
            _carWindow.CardsProgressText.text = card.ProgressText.text;
            _carWindow.ProgressBarImage.fillAmount = card.ProgressImage.fillAmount;
            
        }

        private void OnButtonPressedMethod(Button button) => OnButtonPressed?.Invoke(button);

        private void OnDisable()
        {
            _carWindow.SetActive(false);
        }
    }
}

