using RaceManager.Cars;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class LootboxPopup : MonoBehaviour
    {
        [SerializeField] private Button _closePopupButton;
        [SerializeField] private Button _closePopupWindowButton;
        [SerializeField] private Button _timerOpenButton;
        [SerializeField] private Button _instantOpenButton;
        [SerializeField] private Button _speedupButton;
        [Space]
        [SerializeField] private TMP_Text _moneyAmountText;
        [SerializeField] private TMP_Text _cardsAmountText;
        [SerializeField] private TMP_Text _instantOpenCostText;
        [SerializeField] private TMP_Text _timeToOpenText;
        [Space]
        [SerializeField] private TMP_Text _lootboxRarityText;
        [SerializeField] private Image _lootboxImage;

        public Button ClosePopupButton => _closePopupButton;
        public Button ClosePopupWindowButton => _closePopupWindowButton;
        public Button TimerOpenButton => _timerOpenButton;
        public Button InstantOpenButton => _instantOpenButton;
        public Button SpeedupButton => _speedupButton;
        public TMP_Text MoneyAmountText => _moneyAmountText;
        public TMP_Text CardAmountText => _cardsAmountText;
        public TMP_Text InstantOpenCostText => _instantOpenCostText;
        public TMP_Text TimeToOpenText => _timeToOpenText;
        public TMP_Text LootboxRarityText => _lootboxRarityText;
        public Image LootboxImage => _lootboxImage;

        public void InitiallizeView(PopupInfo info)
        {
            LootboxRarityText.text = $"{info.lootboxRarity.ToString().ToUpper()} LOOTBOX";
            LootboxImage.sprite = info.lootboxSprite;
            MoneyAmountText.text = $"{info.moneyMin}-{info.moneyMax}";
            CardAmountText.text = $"{info.cardsMin}-{info.cardsMax}";
            InstantOpenCostText.text = info.instantOpenCost.ToString();
            TimeToOpenText.text = $"{info.timeToOpen.ToString()}h.";
        }

        public struct PopupInfo
        {
            public Rarity lootboxRarity;
            public Sprite lootboxSprite;
            public int moneyMin;
            public int moneyMax;
            public int cardsMin;
            public int cardsMax;
            public int instantOpenCost;
            public int timeToOpen;
        }
    }
}

