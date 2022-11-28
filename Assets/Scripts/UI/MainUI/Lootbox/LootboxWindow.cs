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
        private SpritesContainerCarCollection _spritesCars;

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
        public RepresentationCard RepresentationCard => _representationCard;
        public RectTransform ReceivedCardsRect => _receivedCardsRect;

        [Inject]
        private void Construct(SpritesContainerCarCollection spritesContainerCars)
        {
            _spritesCars = spritesContainerCars;

            _okButton.onClick.AddListener(Cleanup);
        }

        public async void RepresentLootbox(List<CarCardReward> list)
        { 
            foreach (var card in list)
            {
                AddCarCard(card.CarName, card.CardsAmount);
            }

            //Debug.Log($"List: {_cardsList.Count}; Stack: {_cardsStack.Count}");

            var task = RepresentReceivedCards();
            await Task.WhenAll(task);
        }

        private void AddCarCard(CarName carName, int amount)
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
            cardView.CarName = carName;
            cardView.CardImage.sprite = _spritesCars.GetCarSprite(carName);
            cardView.CardsAmount.text = amount.ToString();

            _cardsList.Add(cardView);
        }

        private async Task RepresentReceivedCards()
        {
            _okButton.SetActive(false);
            _receivedCardsRect.SetActive(false);
            Vector3 cardsPos = _representationCard.transform.localPosition;

            foreach (CarCardView cardView in _cardsList)
            {
                _representationCard.SetActive(true);
                _representationCard.IsVisible = true;
                _representationCard.IsAppearing = true;

                _representationCard.CarName.text = cardView.CarName.ToString();
                _representationCard.CardAmount.text = cardView.CardsAmount.text;
                _representationCard.CarCardImage.sprite = cardView.CardImage.sprite;
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
        }

        private void Cleanup()
        {
            foreach (var card in _cardsList)
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

