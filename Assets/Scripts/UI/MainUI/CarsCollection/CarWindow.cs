using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class CarWindow : AnimatableSubject
    {
        [Header("Main Fields")]
        [SerializeField] private Image _cardsImage;
        [SerializeField] private Image _progressBarImage;
        [Space]
        [SerializeField] private TMP_Text _cardsProgressText;
        [SerializeField] private TMP_Text _upgradeCostText;
        [Space]
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Button _backButton;

        public Image CardsImage => _cardsImage;
        public Image ProgressBarImage => _progressBarImage;

        public TMP_Text CardsProgressText => _cardsProgressText;
        public TMP_Text UpgradeCostText => _upgradeCostText;

        public Button UpgradeButton => _upgradeButton;
        public Button BackButton => _backButton;
    }
}

