﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class MultiplyRewardAnimPanel : AnimatablePanelView
    {
        [SerializeField] private TMP_Text _multiplyerValueText;
        [SerializeField] private TMP_Text _rewardTotalText;
        [SerializeField] private Text _intermediateScoresText;
        [SerializeField] private Button _watchAdsButton;

        public TMP_Text MultiplyerValueText => _multiplyerValueText;
        public TMP_Text RewardTotalText => _rewardTotalText;
        public Text IntermediateText => _intermediateScoresText;
        public Button WatchAdsButton => _watchAdsButton;
    }
}
