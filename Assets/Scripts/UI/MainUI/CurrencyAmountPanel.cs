using TMPro;
using UnityEngine;

namespace RaceManager.UI
{
    public class CurrencyAmountPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _moneyAmount;
        [SerializeField] private TMP_Text _gemsAmount;

        public TMP_Text MoneyAmount => _moneyAmount;
        public TMP_Text GemsAmount => _gemsAmount;
    }
}

