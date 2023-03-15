using RaceManager.Cars;
using RaceManager.Progress;
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
        [SerializeField] private Image _altImage;
        [SerializeField] private TMP_Text _altAmountText;

        [ReadOnly]
        public CarName CarName;
        [ReadOnly]
        public Rarity CarRarity;
        [ReadOnly]
        public int CardsAmount;
        [ReadOnly]
        public UnitReplacementInfo? ReplacementInfo = null;

        public Image CardCarImage => _cardCarImage;
        public Image FrameImage => _frameImage;
        public TMP_Text CardsAmountText => _cardsAmount;
        public Image AlternativeImage => _altImage;
        public TMP_Text AlternativeAmountText => _altAmountText;    
    }
}

