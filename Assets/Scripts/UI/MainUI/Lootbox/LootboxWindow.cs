using RaceManager.Cars;
using RaceManager.Progress;
using RaceManager.Root;
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
        [Space]
        [SerializeField] private RectTransform _moveOutRect;

        private GameObject _carCardPrefab;
        private SpritesContainerCarCollection _spritesCarsCollection;
        private SpritesContainerRewards _spritesReward;

        private List<CarCardView> _cardsList = new List<CarCardView>();
        private Stack<CarCardView> _cardsStack = new Stack<CarCardView>();

        private UIAnimator Animator => Singleton<UIAnimator>.Instance;

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
        private void Construct(SpritesContainerCarCollection spritesContainerCars, SpritesContainerRewards spritesReward)
        {
            _spritesCarsCollection = spritesContainerCars;
            _spritesReward = spritesReward;
            
            _okButton.onClick.AddListener(Cleanup);
        }

        public async void RepresentLootbox(int moneyAmount, List<CarCardReward> list)
        { 
            foreach (var card in list)
            {
                AddCarCard(card.CarName, card.Rarity, card.CardsAmount);
            }

            MakeMoneyCard(moneyAmount);

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
            cardView.CardsAmount.SetActive(true);
            cardView.CardsAmount.color = color;

            cardView.FrameImage.SetActive(true);
            cardView.FrameImage.color = color;

            cardView.CarName = carName;
            cardView.CarRarity = carRarity;
            cardView.MoneyAmount = 0;

            cardView.CardCarImage.SetActive(true);
            cardView.CardCarImage.sprite = _spritesCarsCollection.GetCarSprite(carName);

            cardView.CardsAmount.SetActive(true);
            cardView.CardsAmount.text = amount.ToString();
            
            cardView.MoneyImage.SetActive(false);

            cardView.MoneyAmountText.SetActive(false);

            _cardsList.Add(cardView);
        }

        private void MakeMoneyCard(int moneyAmount, CarCardView card = null)
        {
            CarCardView cardView;
            if (_cardsStack.Count != 0)
            {
                cardView = _cardsStack.Pop();
                cardView.SetActive(true);
            }
            else if (card != null)
            { 
                cardView = card;
            }
            else
            {
                GameObject cardGo = Instantiate(CarCardPrefab, _receivedCardsContent.transform, false);
                cardView = cardGo.GetComponent<CarCardView>();
            }

            cardView.CardsAmount.SetActive(false);

            cardView.FrameImage.SetActive(false);

            cardView.CardCarImage.SetActive(false);

            cardView.CardsAmount.SetActive(false);

            cardView.MoneyAmount = moneyAmount;

            cardView.MoneyImage.SetActive(true);
            cardView.MoneyImage.sprite = _spritesReward.GetColoredRewardSprite(RewardType.Money);

            cardView.MoneyAmountText.SetActive(true);
            cardView.MoneyAmountText.text = moneyAmount.ToString();

            if(card == null)
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

                SetRepresentationCard(cardView);
                //string name = cardView.CarName.ToString().SplitByUppercaseWith(" ");
                //_representationCard.CarName.text = name.ToUpper();

                ////Color color = _spritesCarsCollection.GetCarRarityColor(cardView.CarRarity);
                //_representationCard.FrameImage.color = cardView.FrameImage.color;
                //_representationCard.CardAmount.text = cardView.CardsAmount.text;
                //_representationCard.CarImage.sprite = cardView.CardCarImage.sprite;
                //_representationCard.Animator.SetBool(AnimParameter.Taped, false);

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

        private void SetRepresentationCard(CarCardView cardView)
        {
            if (cardView.MoneyAmount > 0)
            {
                _representationCard.CarName.SetActive(false);

                _representationCard.FrameImage.SetActive(false);

                _representationCard.CardAmount.SetActive(false);

                _representationCard.CarImage.SetActive(false);

                _representationCard.MoneyAmount.SetActive(true);
                _representationCard.MoneyAmount.text = cardView.MoneyAmount.ToString();

                Animator.SpawnGroupOnAndMoveTo(RewardType.Money, transform, _representationCard.transform, _moveOutRect.transform);

                _representationCard.Animator.SetBool(AnimParameter.Taped, false);
            }
            else
            {
                string name = cardView.CarName.ToString().SplitByUppercaseWith(" ");
                _representationCard.CarName.SetActive(true);
                _representationCard.CarName.text = name.ToUpper();

                //Color color = _spritesCarsCollection.GetCarRarityColor(cardView.CarRarity);
                _representationCard.FrameImage.SetActive(true);
                _representationCard.FrameImage.color = cardView.FrameImage.color;

                _representationCard.CardAmount.SetActive(true);
                _representationCard.CardAmount.text = cardView.CardsAmount.text;

                _representationCard.CarImage.SetActive(true);
                _representationCard.CarImage.sprite = cardView.CardCarImage.sprite;

                _representationCard.MoneyAmount.SetActive(false);

                _representationCard.Animator.SetBool(AnimParameter.Taped, false);
            }
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

