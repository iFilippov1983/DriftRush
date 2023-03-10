using TMPro;
using UnityEngine.UI;
using UnityEngine;

namespace RaceManager.UI
{
    public class RepresentationCard : AnimatableSubject
    {
        [Header("Main Fields")]
        [SerializeField] private TMP_Text _carName;
        [SerializeField] private TMP_Text _cardsAmountText;
        [SerializeField] private TMP_Text _moneyAmountText;
        [Space]
        [SerializeField] private Image _carImage;
        [SerializeField] private Image _frameImage;
        [SerializeField] private Animator _animator;

        public bool IsVisible = true;
        public bool IsAppearing = true;

        public TMP_Text CarName => _carName;
        public TMP_Text CardAmount => _cardsAmountText;
        public TMP_Text MoneyAmount => _moneyAmountText;
        public Image CarImage => _carImage;
        public Image FrameImage => _frameImage;
        public Animator Animator => _animator;

        //Methods for Animator events
        private void HasAppeared() => IsAppearing = false;
        private void GotOut() => IsVisible = false; 
    }
}

