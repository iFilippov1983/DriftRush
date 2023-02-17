using RaceManager.Cars;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class CollectionCard : MonoBehaviour
    {
        [SerializeField] private Image _frameImage;
        [SerializeField] private Image _carImage;
        [SerializeField] private Image _progressImage;
        [SerializeField] private Image _lockedImage;
        [SerializeField] private Image _cardsImage;
        [Space]
        [SerializeField] private TMP_Text _progressText;
        [SerializeField] private TMP_Text _carNameText;
        [SerializeField] private TMP_Text _partsAmountText;
        [Space]
        [SerializeField] private Button _useCarButton;

        public Image FrameImage => _frameImage;
        public Image CarImage => _carImage;
        public Image ProgressImage => _progressImage;
        public Image LockedImage => _lockedImage;
        public Image CardImage => _cardsImage;

        public TMP_Text ProgressText => _progressText;
        public TMP_Text CarNameText => _carNameText;
        public TMP_Text PartsAmountText => _partsAmountText;

        public Button UseCarButton => _useCarButton;

        public CarName CashedCarName { get; set; }
    }
}

