using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class WorldSpaceUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _partsAmountText;
        [SerializeField] private TMP_Text _upgradeCostText;
        [SerializeField] private Button _upgradeButton;

        public TMP_Text PartsAmount => _partsAmountText;
        public TMP_Text UpgradeCost => _upgradeCostText;
        public Button UpgradeButton => _upgradeButton;
    }
}

