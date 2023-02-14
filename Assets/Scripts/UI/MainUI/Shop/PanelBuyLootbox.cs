using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class PanelBuyLootbox : MonoBehaviour
    {
        [SerializeField] private Rarity _looboxRarity;
        [SerializeField] private Button _buyButton;
        [SerializeField] private Image _lootboxImage;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private TMP_Text _rarityText;

        public Rarity LooboxRarity { get => _looboxRarity; set => _looboxRarity = value; }
        public Button BuyButton => _buyButton;
        public Image LootboxImage => _lootboxImage;
        public TMP_Text CostText => _costText;
        public TMP_Text RarityText => _rarityText;
    }
}

