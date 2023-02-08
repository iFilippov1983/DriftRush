using RaceManager.Shop;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class OfferPanelNoAds : MonoBehaviour, IOfferPanel
    {
        [ShowInInspector, ReadOnly]
        private const ShopOfferType _type = ShopOfferType.NoAds;

        [SerializeField] private IAPButton _buyButton;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private Image _bonusImage_1;
        [SerializeField] private Image _bonusImage_2;
        [SerializeField] private TMP_Text _bonusAmountText_1;
        [SerializeField] private TMP_Text _bonusAmountText_2;

        public IAPButton BuyButton => _buyButton;
        public TMP_Text CostText => _costText;
        public Image BonusImage_1 => _bonusImage_1;
        public Image BonusImage_2 => _bonusImage_2;
        public TMP_Text BonusAmountText_1 => _bonusAmountText_1;
        public TMP_Text BonusAmountText_2 => _bonusAmountText_2;

        public ShopOfferType Type => _type;
        public GameObject GameObject => gameObject;

        public void Accept(IOfferPanelInstaller installer)
        {
            installer.Install(this);
        }
    }
}

