using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class CupsProgressPanel : AnimatableSubject
    {
        [Space]
        [Header("Main Fields")]
        [SerializeField] private Image _fillImage;
        [SerializeField] private Image _hasUnreceivedRewardsImage;
        [SerializeField] private TMP_Text _cupsAmountOwned;
        [SerializeField] private TMP_Text _nextGlobalGoalAmount;

        public Image FillImage => _fillImage;
        public Image HasUnreceivedRewardsImage => _hasUnreceivedRewardsImage;
        public TMP_Text CupsAmountOwned => _cupsAmountOwned;
        public TMP_Text NextGlobalGoalAmount => _nextGlobalGoalAmount;
    }
}

