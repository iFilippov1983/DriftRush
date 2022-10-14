using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class FinishUIView : MonoBehaviour
    {
        [SerializeField] private GameObject _backPanel;

        [Title("Currency amount panel")]
        [SerializeField] private TMP_Text _moneyAmount;
        [SerializeField] private TMP_Text _gemsAmount;

        [Title("Finish panel")]
        [SerializeField] private TMP_Text _positionText;
        [SerializeField] private TMP_Text _rewardMoneyAmountText;
        [SerializeField] private TMP_Text _rewardCupsAmountText;
        [SerializeField] private Button _okButtonFinish;

        [Title("Extra reward panel")]
        [Tooltip("Image representing extra reward item")]
        [SerializeField] private Image _fillUpImage;
        [SerializeField] private Image _backImage;
        [SerializeField] private TMP_Text _persentageProgressText;
        [SerializeField] private Button _okButtonExtraReward;

        public TMP_Text MoneyAmount => _moneyAmount;
        public TMP_Text GesAmount => _gemsAmount;

        public GameObject BackPanel => _backPanel;
        public TMP_Text PositionText => _positionText;
        public TMP_Text RewardMoneyAmountText => _rewardMoneyAmountText;
        public TMP_Text RewardCupsAmountText => _rewardCupsAmountText;
        public Button OkButtonFinish => _okButtonFinish;

        public Image FillUpImage => _fillUpImage;
        public Image BackImage => _backImage;
        public TMP_Text PersentageProgressText => _persentageProgressText;
        public Button OkButtonExtraReward => _okButtonExtraReward;

    }

}
