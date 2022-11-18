using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class CupsAmountSlider : MonoBehaviour
    {
        [SerializeField] private Image _frontSliderImage;
        [SerializeField] private Image _sliderLevelImage;
        [SerializeField] private TMP_Text _cupsAmountText;

        public Image SliderImage => _frontSliderImage;
        public Image SliderLevelImage => _sliderLevelImage;
        public TMP_Text CupsAmountText => _cupsAmountText;
    }
}

