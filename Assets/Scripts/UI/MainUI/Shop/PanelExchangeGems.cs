using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class PanelExchangeGems : MonoBehaviour
    {
        [SerializeField] private Button _buyButton;
        [SerializeField] private Image _moneyAmountImage;
        [SerializeField] private TMP_Text _moneyAmountText;
        [SerializeField] private TMP_Text _exchangeCostText;

        public Button BuyButton => _buyButton;
        public Image MoneyAmountImage => _moneyAmountImage;
        public TMP_Text MoneyAmountText => _moneyAmountText;
        public TMP_Text ExchangeCostText => _exchangeCostText;
    }
}

