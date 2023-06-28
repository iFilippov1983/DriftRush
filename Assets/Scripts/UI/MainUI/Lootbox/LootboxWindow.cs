using RaceManager.Progress;
using RaceManager.Root;
using RaceManager.Tools;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx.Triggers;
using UniRx;

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

        private CardRepresenter _representer;
        private GameObject _carCardPrefab;
        private SpritesContainerCarCollection _spritesCarsCollection;
        private SpritesContainerRewards _spritesReward;

        private CancellationTokenSource _tokenSource;

        private Queue<CarCardView> _cardsQueue = new Queue<CarCardView>();
        private Stack<CarCardView> _cardsStack = new Stack<CarCardView>();

        private UIAnimator Animator => Singleton<UIAnimator>.Instance;

        private Subject<bool> _mousePressedSubject = new Subject<bool>();

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

            _tokenSource = new CancellationTokenSource();
            _representer = new CardRepresenter(_representationCard, Animator);

            this.UpdateAsObservable()
                .Subscribe(_ => 
                { 
                    if(Input.GetMouseButtonDown(0))
                        _mousePressedSubject.OnNext(true);
                })
                ?.AddTo(this);

            _okButton.onClick.AddListener(Cleanup);
        }

        public async void RepresentLootbox(int lootboxMoney, List<CarCardReward> list)
        { 
            ResetCarCards();

            foreach (var reward in list)
            {
                CarCardView card = AddCarCard(reward);

                if (reward.ReplacementInfo != null)
                    MakeAltCard(reward.ReplacementInfo, card);
            }

            MakeAltCard(new UnitReplacementInfo() 
            { 
                Type = GameUnitType.Money,
                Amount = lootboxMoney 
            });

            //Debug.Log($"List: {_cardsList.Count}; Stack: {_cardsStack.Count}");

            while (await RepresentReceivedCards() == false)
            {
                _tokenSource.Token.ThrowIfCancellationRequested();
                await Task.Yield();
            }
        }

        private void ResetCarCards()
        {
            if (_cardsStack.Count != 0)
            {
                foreach (var card in _cardsStack)
                { 
                    card.CardsAmount = 0;
                    card.ReplacementInfo = null;
                    card.transform.SetParent(transform);
                    card.SetActive(false);
                }
            }
        }

        private CarCardView AddCarCard(CarCardReward reward)
        {
            CarCardView cardView;
            if (_cardsStack.Count != 0)
            {
                cardView = _cardsStack.Pop();
                cardView.transform.SetParent(_receivedCardsContent.transform);
                cardView.SetActive(true);
            }
            else 
            {
                GameObject cardGo = Instantiate(CarCardPrefab, _receivedCardsContent.transform, false);
                cardView = cardGo.GetComponent<CarCardView>();
            }

            Color color = _spritesCarsCollection.GetCarRarityColor(reward.Rarity);
            cardView.CardsAmountText.SetActive(true);
            cardView.CardsAmountText.color = color;

            cardView.FrameImage.SetActive(true);
            cardView.FrameImage.color = color;

            cardView.CarName = reward.CarName;
            cardView.CarRarity = reward.Rarity;

            cardView.CardCarImage.SetActive(true);
            cardView.CardCarImage.sprite = _spritesCarsCollection.GetCarSprite(reward.CarName);

            cardView.CardsAmountText.SetActive(true);
            cardView.CardsAmountText.text = reward.CardsAmount.ToString();
            
            cardView.CardsAmount = reward.CardsAmount;

            cardView.ReplacementInfo = reward.ReplacementInfo;

            cardView.AlternativeImage.SetActive(false);
            cardView.AlternativeAmountText.SetActive(false);

            _cardsQueue.Enqueue(cardView);

            return cardView;
        }

        private void MakeAltCard(UnitReplacementInfo? info, CarCardView card = null)
        {
            CarCardView cardView;
            if (card != null)
            {
                cardView = card;
            }
            else if (_cardsStack.Count != 0)
            {
                cardView = _cardsStack.Pop();
                cardView.transform.SetParent(_receivedCardsContent.transform);
                cardView.SetActive(true);
            }
            else
            {
                GameObject cardGo = Instantiate(CarCardPrefab, _receivedCardsContent.transform, false);
                cardView = cardGo.GetComponent<CarCardView>();
            }

            cardView.CardsAmountText.SetActive(false);

            cardView.FrameImage.SetActive(false);

            cardView.CardCarImage.SetActive(false);

            cardView.CardsAmountText.SetActive(false);

            cardView.ReplacementInfo = info;

            cardView.AlternativeImage.SetActive(true);
            cardView.AlternativeImage.sprite = _spritesReward.GetColoredRewardSprite(info.Value.Type);

            cardView.AlternativeAmountText.SetActive(true);
            cardView.AlternativeAmountText.text = info.Value.Amount.ToString();

            if(card == null)
                _cardsQueue.Enqueue(cardView);
        }

        private async Task<bool> RepresentReceivedCards()
        {
            _okButton.SetActive(false);
            _receivedCardsRect.SetActive(false);

            foreach (CarCardView cardView in _cardsQueue)
            {
                while (await _representer.Represent(cardView, _mousePressedSubject, _tokenSource) == false)
                {
                    _tokenSource.Token.ThrowIfCancellationRequested();
                    await Task.Yield();
                }
            }

            _representationCard.CardRect.SetActive(false);
            _receivedCardsRect.SetActive(true);
            _okButton.SetActive(true);

            return true;
        }

        private void Cleanup()
        {
            while(_cardsQueue.Count > 0)
            {
                CarCardView cardView = _cardsQueue.Dequeue();
                cardView.CardsAmount = 0;
                cardView.ReplacementInfo = null;
                _cardsStack.Push(cardView);
            }
 
            _cardsQueue.Clear();
        }

        private void OnDestroy()
        {
            Cleanup();
            _tokenSource.Cancel();
        }
    }
}

