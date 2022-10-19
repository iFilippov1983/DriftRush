using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.Root
{
    public class CupsProgressPanel : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;
        [SerializeField] private TMP_Text _cupsAmountOwned;
        [SerializeField] private TMP_Text _cupsAmountGlobal;

        public Image FillImage => _fillImage;
        public TMP_Text CupsAmountOwned => _cupsAmountOwned;
        public TMP_Text CupsAmountGlobal => _cupsAmountGlobal;
    }
}

