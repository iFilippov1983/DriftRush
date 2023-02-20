using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class StepWindow : MonoBehaviour
    {
        [SerializeField] private TMP_Text _goalCupsAmountText;
        [SerializeField] private Image _claimedImage;
        [SerializeField] private Image _titleImage;
        [Space]
        [SerializeField] private RewardSimpleUIView _rewardSimple;
        [SerializeField] private RewardCardsUIView _rewardCards;
        [SerializeField] private RewardLootboxUIView _rewardLootbox;

        public TMP_Text GoalCupsAmount => _goalCupsAmountText;
        public Image ClaimedImage => _claimedImage;
        public Image TitleImage => _titleImage;
        public RewardSimpleUIView RewardSimple => _rewardSimple;
        public RewardCardsUIView RewardCards => _rewardCards;
        public RewardLootboxUIView RewardLootbox => _rewardLootbox;
    }
}

