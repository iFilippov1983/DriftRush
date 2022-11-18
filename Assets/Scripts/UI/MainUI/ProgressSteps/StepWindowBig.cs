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
        [Space]
        [SerializeField] private RewardSimpleUIView _rewardSimple;
        [SerializeField] private TMP_Text _incomeBonusText;

        public TMP_Text GoalCupsAmount => _goalCupsAmountText;
        public TMP_Text LevelName => _levelNameText;
        public TMP_Text IncomeBonusText => _incomeBonusText;

        public Image ClaimedImage => _claimedImage;
        public Image LevelRepresentationImage => _levelRepresentationImage;

        public RewardSimpleUIView RewardSimple => _rewardSimple;
    }
}

