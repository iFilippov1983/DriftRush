using RaceManager.Cars;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class CarCardView : MonoBehaviour
    {
        [SerializeField] private Image _cardCarImage;
        [SerializeField] private Image _frameImage;
        [SerializeField] private TMP_Text _cardsAmount;
        [Space]
        [SerializeField] private Image _moneyImage;
        [SerializeField] private TMP_Text _moneyAmountText;

        [ReadOnly]
        public CarName CarName;
        [ReadOnly]
        public Rarity CarRarity;
        [ReadOnly]
        public int MoneyAmount = 0;

        public Image CardCarImage => _cardCarImage;
        public Image FrameImage => _frameImage;
        public TMP_Text CardsAmount => _cardsAmount;
        public Image MoneyImage => _moneyImage;
        public TMP_Text MoneyAmountText => _moneyAmountText;    
    }
}

