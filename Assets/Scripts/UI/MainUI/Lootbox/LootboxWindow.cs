using RaceManager.Cars;
using RaceManager.Progress;
using RaceManager.Tools;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace RaceManager.UI
{
    public class LootboxWindow : MonoBehaviour
    {
        [SerializeField] private Button _okButton;
        [Space]
        [SerializeField] private RepresentationCard _representationCard;
        [Space]
        [SerializeField] private RectTransform _receivedCardsRect;
        [SerializeField] private GridLayoutGroup _receivedCardsContent;

        private GameObject _carCardPrefab;
        private SpritesContainerCarCollection _spritesCarsCollection;

        private List<CarCardView> _cardsList = new List<CarCardView>();
        private Stack<CarCardView> _cardsStack = new Stack<CarCardView>();

        private GameObject CarCardPrefab
        {
            get 
            {
                if (_carCardPrefab == null)
                    _carCardPrefab = ResourcesLoader.LoadPrefab(ResourcePath.CarCardViewPrefab);
                return _carCardPrefab;
            }
        }

        public Button OkButton => _okButton;

        [Inject]
        private void Construct(SpritesContainerCarCollection spritesContainerCars)
        {
            _spritesCarsCollection = spritesContainerCars;
            
            _okButton.onClick.AddListener(Cleanup);
        }

        public async void RepresentLootbox(int moneyAmount, List<CarCardReward> list)
        { 
            foreach (var card in list)
            {
                AddCarCard(card.CarName, card.Rarity, card.CardsAmount);
            }

            //Debug.Log($"List: {_cardsList.Count}; Stack: {_cardsStack.Count}");
            
            while (await RepresentReceivedCards() == false)
                await Task.Yield();
        }

        private void AddCarCard(CarName carName, Rarity carRarity, int amount)
        {
            CarCardView cardView;
            if (_cardsStack.Count != 0)
            {
                cardView = _cardsStack.Pop();
                cardView.SetActive(true);
            }
            else 
            {
                GameObject cardGo = Instantiate(CarCardPrefab, _receivedCardsContent.transform, false);
                cardView = cardGo.GetComponent<CarCardView>();
            }

            Color color = _spritesCarsCollection.GetCarRarityColor(carRarity);
            cardView.CardsAmount.color = color;
            cardView.FrameImage.color = color;

            cardView.CarName = carName;
            cardView.CarRarity = carRarity;
            cardView.CardCarImage.sprite = _spritesCarsCollection.GetCarSprite(carName);
            cardView.CardsAmount.text = amount.ToString();
            

            _cardsList.Add(cardView);
        }

        private async Task<bool> RepresentReceivedCards()
        {
            _okButton.SetActive(false);
            _receivedCardsRect.SetActive(false);

            foreach (CarCardView cardView in _cardsList)
            {
                _representationCard.SetActive(true);
                _representationCard.IsVisible = true;
                _representationCard.IsAppearing = true;

                string name = cardView.CarName.ToString().SplitByUppercaseWith(" ");
                _representationCard.CarName.text = name.ToUpper();

                //Color color = _spritesCarsCollection.GetCarRarityColor(cardView.CarRarity);
                _representationCard.FrameImage.color = cardView.FrameImage.color;
                _representationCard.CardAmount.text = cardView.CardsAmount.text;
                _representationCard.CarImage.sprite = cardView.CardCarImage.sprite;
                _representationCard.Animator.SetBool(AnimParameter.Taped, false);

                while (_representationCard.IsVisible)
                {
                    await Task.Yield();
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (_representationCard.IsAppearing)
                            _representationCard.IsVisible = false;

                        _representationCard.Animator.SetBool(AnimParameter.Taped, true);
                    }
                }

                _representationCard.SetActive(false);
            }

            _receivedCardsRect.SetActive(true);
            _okButton.SetActive(true);

            return true;
        }

        private void Cleanup()
        {
            foreach (CarCardView card in _cardsList)
                _cardsStack.Push(card);

            _cardsList.Clear();

            //Debug.Log($"List: {_cardsList.Count}; Stack: {_cardsStack.Count}");
        }

        private void OnDestroy()
        {
            _okButton.onClick.RemoveAllListeners();
        }
    }
}

