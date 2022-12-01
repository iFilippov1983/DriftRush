using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class UpgradeWindowTuner : MonoBehaviour
    {
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private TMP_Text _partsAmountText;
        [SerializeField] private Button _upgradeButton;

        public TMP_Text CostText => _costText;
        public TMP_Text PartsAmountText => _partsAmountText;
        public Button UpgradeButton => _upgradeButton;
    }
}