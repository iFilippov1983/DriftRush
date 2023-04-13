using RaceManager.Progress;
using RaceManager.Root;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace RaceManager.UI
{
    public class CardsWindow : MonoBehaviour
    {
        [SerializeField] private Button _okButton;
        [Space]
        [SerializeField] private RepresentationCard _representationCard;
        [Space]
        [SerializeField] private RectTransform _moveOutRect;

        private CardRepresenter _representer;
        private SpritesContainerCarCollection _spritesCarsCollection;

        private Subject<bool> _mouseButtonSubject;

        private UIAnimator Animator => Singleton<UIAnimator>.Instance;

        public Button OkButton => _okButton;

        [Inject]
        private void Construct(SpritesContainerCarCollection spritesContainerCars)
        {
            _spritesCarsCollection = spritesContainerCars;

            _representer = new CardRepresenter(_representationCard, Animator);
            _mouseButtonSubject = new Subject<bool>();
        }

        public async Task<bool> RepresentCards(CarCardReward cardsReward, CancellationTokenSource tokenSource)
        {
            _mouseButtonSubject.BindToButtonOnClick(_okButton, _ => _mouseButtonSubject?.OnNext(true));
            CarCardsInfo info = MakeCardsInfo(cardsReward);

            while (await _representer.Represent(info, _mouseButtonSubject, tokenSource) == false)
            {
                tokenSource.Token.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            return true;
        }

        private CarCardsInfo MakeCardsInfo(CarCardReward reward)
        {
            CarCardsInfo cardsInfo = new CarCardsInfo();

            cardsInfo.CarName = reward.CarName;
            cardsInfo.CarRarity = reward.Rarity;
            cardsInfo.CardsAmount = reward.CardsAmount;
            cardsInfo.ReplacementInfo = reward.ReplacementInfo;
            cardsInfo.CardColor = _spritesCarsCollection.GetCarRarityColor(reward.Rarity);
            cardsInfo.CarSprite = _spritesCarsCollection.GetCarSprite(reward.CarName);

            return cardsInfo;
        }
    }
}

