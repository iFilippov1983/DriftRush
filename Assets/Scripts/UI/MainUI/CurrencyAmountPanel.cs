using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class CurrencyAmountPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _moneyAmount;
        [SerializeField] private Image _moneyImage;
        [Space]
        [SerializeField] private TMP_Text _gemsAmount;
        [SerializeField] private Image _gemsImage;

        public TMP_Text MoneyAmount => _moneyAmount;
        public Image MoneyImage => _moneyImage;
        public TMP_Text GemsAmount => _gemsAmount;
        public Image GemsImage => _gemsImage;
    }
}

