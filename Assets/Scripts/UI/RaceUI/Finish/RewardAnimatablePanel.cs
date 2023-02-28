using RaceManager.Race;
using System;
using TMPro;
using UnityEngine;

namespace RaceManager.UI
{
    [Serializable]
    public class RewardAnimatablePanel : AnimatablePanelView
    {
        [SerializeField] private RaceScoresType _scoreType;
        [SerializeField] private TMP_Text _rewardAmountText;

        public RaceScoresType ScoreType => _scoreType;
        public TMP_Text RewardAmountText => _rewardAmountText;
    }
}
