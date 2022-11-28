using RaceManager.Cars;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class CollectionCard : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _progressImage;
        [SerializeField] private Image _lockedImage;
        [Space]
        [SerializeField] private TMP_Text _progressCurrentText;
        [SerializeField] private TMP_Text _progressTotalText;
        [SerializeField] private TMP_Text _carNameText;
        [Space]
        [SerializeField] private Button _useCarButton;

        public Image BackgroundImage => _backgroundImage;
        public Image ProgressImage => _progressImage;
        public Image LockedImage => _lockedImage;

        public TMP_Text ProgressCurrentText => _progressCurrentText;
        public TMP_Text ProgressTotalText => _progressTotalText;
        public TMP_Text CarNameText => _carNameText;

        public Button UseCarButton => _useCarButton;

        public CarName CashedCarName { get; set; }
    }
}

