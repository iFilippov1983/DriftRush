using RaceManager.Cars;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class CarCardView : MonoBehaviour
    {
        [SerializeField] private Image _cardImage;
        [SerializeField] private TMP_Text _cardsAmount;

        [ReadOnly]
        public CarName CarName;

        public Image CardImage => _cardImage;
        public TMP_Text CardsAmount => _cardsAmount;
    }
}

