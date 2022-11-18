using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class RewardSimpleUIView : MonoBehaviour
    {
        [SerializeField] private Image _rewardImage;
        [SerializeField] private TMP_Text _rewardAmountText;

        public Image RewardImage => _rewardImage;
        public TMP_Text RewardAmountText => _rewardAmountText;
    }
}

