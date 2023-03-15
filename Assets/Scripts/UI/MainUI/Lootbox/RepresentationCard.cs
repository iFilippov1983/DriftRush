using TMPro;
using UnityEngine.UI;
using UnityEngine;
using RaceManager.Progress;
using Sirenix.OdinInspector;

namespace RaceManager.UI
{
    public class RepresentationCard : AnimatableSubject
    {
        [Header("Main Fields")]
        [SerializeField] private TMP_Text _carName;
        [SerializeField] private TMP_Text _cardsAmountText;
        [SerializeField] private TMP_Text _altAmountText;
        [SerializeField] private TMP_Text _maxCardsText;
        [Space]
        [SerializeField] private Image _carImage;
        [SerializeField] private Image _frameImage;
        [Space]
        [SerializeField] private RectTransform _cardRect;
        [SerializeField] private RectTransform _replaceableRect;
        [Space]
        [SerializeField] private RectTransform _disappearTargetRectLeft;
        [SerializeField] private RectTransform _disappearTargetRectUp;

        [ReadOnly]
        public bool IsVisible = true;
        [ReadOnly]
        public bool IsAppearing = true;
        [ReadOnly]
        public bool IsReplaceable = false;
        [ReadOnly]
        public bool IsReplacing = false;
        [ReadOnly]
        public UnitReplacementInfo? ReplacementInfo = null;

        public TMP_Text CarNameText => _carName;
        public TMP_Text CardAmountText => _cardsAmountText;
        public TMP_Text AlternativeAmountText => _altAmountText;
        public TMP_Text MaxCardsText => _maxCardsText;
        public Image CarImage => _carImage;
        public Image FrameImage => _frameImage;
        public RectTransform CardRect => _cardRect;
        public RectTransform ReplaceableRect => _replaceableRect;
        public RectTransform DisappearTargetRectLeft => _disappearTargetRectLeft;
        public RectTransform DisappearTargetRectUp => _disappearTargetRectUp;
    }
}

