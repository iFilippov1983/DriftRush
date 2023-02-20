using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class PanelBuyGems : MonoBehaviour
    {
        [SerializeField] private IAPButton _buyButton;
        [SerializeField] private Image _gemsImage;
        [SerializeField] private TMP_Text _gemsAmountText;
        [SerializeField] private TMP_Text _costText;

        public IAPButton BuyButton => _buyButton;
        public Image GemsImage => _gemsImage;
        public TMP_Text GemsAmountText => _gemsAmountText;
        public TMP_Text CostText => _costText;
    }
}

