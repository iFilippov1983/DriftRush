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

        [ReadOnly]
        public CarName CarName;
        [ReadOnly]
        public Rarity CarRarity;

        public Image CardCarImage => _cardCarImage;
        public Image FrameImage => _frameImage;
        public TMP_Text CardsAmount => _cardsAmount;
    }
}

