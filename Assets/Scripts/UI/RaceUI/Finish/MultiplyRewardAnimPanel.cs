using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class MultiplyRewardAnimPanel : AnimatablePanelView
    {
        [SerializeField] private TMP_Text _rewardTotalText;
        [SerializeField] private Button _watchAdsButton;

        public TMP_Text RewardTotalText => _rewardTotalText;
        public Button WatchAdsButton => _watchAdsButton;
    }
}
