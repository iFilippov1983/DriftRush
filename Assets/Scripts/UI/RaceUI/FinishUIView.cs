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
        [SerializeField] private TopPanelObjectView _topPanel;
        [SerializeField] private TMP_Text _moneyAmount;
        [SerializeField] private TMP_Text _gemsAmount;

        [Title("Finish panel")]
        [SerializeField] private FinishPanelObjectView _finishPanel;
        [SerializeField] private GotLootboxPopup _gotLootboxPopup;
        [SerializeField] private TMP_Text _positionText;
        [SerializeField] private TMP_Text _rewardMoneyAmountText;
        [SerializeField] private TMP_Text _rewardCupsAmountText;
        [SerializeField] private Button _okButtonFinish;

        [Title("Extra reward panel")]
        [SerializeField] private ExtraRewardPanelObjectView _extraRewardPanel;
        [Tooltip("Image representing extra reward item")]
        [SerializeField] private Image _fillUpImage;
        [SerializeField] private TMP_Text _persentageProgressText;
        [SerializeField] private Button _okButtonExtraReward;

        public TopPanelObjectView TopPanel => _topPanel;
        public TMP_Text MoneyAmount => _moneyAmount;
        public TMP_Text GemsAmount => _gemsAmount;

        public FinishPanelObjectView FinishPanel => _finishPanel;
        public GotLootboxPopup GotLootboxPopup => _gotLootboxPopup;
        public TMP_Text PositionText => _positionText;
        public TMP_Text RewardMoneyAmountText => _rewardMoneyAmountText;
        public TMP_Text RewardCupsAmountText => _rewardCupsAmountText;
        public Button OkButtonFinish => _okButtonFinish;

        public ExtraRewardPanelObjectView ExtraRewardPanel => _extraRewardPanel;
        public Image FillUpImage => _fillUpImage;
        public TMP_Text PersentageProgressText => _persentageProgressText;
        public Button OkButtonExtraReward => _okButtonExtraReward;

        private void OnEnable()
        {
            _backPanel.SetActive(true);
        }

        private void OnDisable()
        {
            _backPanel.SetActive(false);
        }
    }

}
