using RaceManager.Cars;
using RaceManager.Progress;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class CarCardView : MonoBehaviour, ICarCardsInfo
    {
        [SerializeField] private Image _cardCarImage;
        [SerializeField] private Image _frameImage;
        [SerializeField] private TMP_Text _cardsAmount;
        [Space]
        [SerializeField] private Image _altImage;
        [SerializeField] private TMP_Text _altAmountText;

        [ShowInInspector, ReadOnly]
        public CarName CarName { get; set; }
        [ShowInInspector, ReadOnly]
        public Rarity CarRarity { get; set; }
        [ShowInInspector, ReadOnly]
        public int CardsAmount { get; set; }
        [ShowInInspector, ReadOnly]
        public UnitReplacementInfo? ReplacementInfo { get; set; } = null;

        public Image CardCarImage => _cardCarImage;
        public Image FrameImage => _frameImage;
        public TMP_Text CardsAmountText => _cardsAmount;
        public Image AlternativeImage => _altImage;
        public TMP_Text AlternativeAmountText => _altAmountText;

        public Color CardColor => FrameImage.color;
        public Sprite CarSprite => CardCarImage.sprite;
    }
}

