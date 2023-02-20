using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class StepWindowBig : MonoBehaviour
    {
        [SerializeField] private TMP_Text _goalCupsAmountText;
        [SerializeField] private TMP_Text _levelNameText;
        [SerializeField] private Image _claimedImage;
        [SerializeField] private Image _levelRepresentationImage;
        [SerializeField] private Image _titleImage;
        [Space]
        [SerializeField] private RewardSimpleUIView _rewardSimple;
        [SerializeField] private TMP_Text _incomeBonusText;
        [SerializeField] private RectTransform _incomeBonusRectTransform;

        public TMP_Text GoalCupsAmount => _goalCupsAmountText;
        public TMP_Text LevelName => _levelNameText;
        public TMP_Text IncomeBonusText => _incomeBonusText;

        public Image ClaimedImage => _claimedImage;
        public Image LevelRepresentationImage => _levelRepresentationImage;
        public Image TitleImage => _titleImage;

        public RewardSimpleUIView RewardSimple => _rewardSimple;
        public RectTransform IncomeBonusWindow => _incomeBonusRectTransform;
    }
}

