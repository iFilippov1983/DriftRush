using TMPro;
using UnityEngine.UI;
using UnityEngine;

namespace RaceManager.UI
{
    public class RepresentationCard : MonoBehaviour
    {
        [SerializeField] private TMP_Text _carName;
        [SerializeField] private TMP_Text _cardsAmountText;
        [Space]
        [SerializeField] private Image _carImage;
        [SerializeField] private Image _frameImage;
        [SerializeField] private Animator _animator;

        public bool IsVisible = true;
        public bool IsAppearing = true;

        public TMP_Text CarName => _carName;
        public TMP_Text CardAmount => _cardsAmountText;
        public Image CarImage => _carImage;
        public Image FrameImage => _frameImage;
        public Animator Animator => _animator;

        //Methods for Animator events
        private void HasAppeared() => IsAppearing = false;
        private void GotOut() => IsVisible = false; 
    }
}

