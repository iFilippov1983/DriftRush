using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class RewardCardsUIView : MonoBehaviour
    {
        [SerializeField] private Image _carImage;
        [SerializeField] private TMP_Text _carName;
        [Space]
        [SerializeField] private Image _cardsRarityImage;
        [SerializeField] private TMP_Text _cardsAmountText;

        public Image CarImage => _carImage;
        public TMP_Text CarName => _carName;

        public Image CardsRarityImage => _cardsRarityImage;
        public TMP_Text CardAmountText => _cardsAmountText;
    }
}

