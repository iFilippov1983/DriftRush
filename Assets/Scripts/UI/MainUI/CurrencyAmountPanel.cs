using TMPro;
using UnityEngine;

namespace RaceManager.Root
{
    public class CurrencyAmountPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _moneyAmount;
        [SerializeField] private TMP_Text _gemsAmount;

        public TMP_Text MoneyAmount => _moneyAmount;
        public TMP_Text GesmsAmount => _gemsAmount;
    }
}

