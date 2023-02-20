using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class CupsAmountSlider : MonoBehaviour
    {
        [SerializeField] private Image _frontSliderImage;
        [SerializeField] private TMP_Text _cupsAmountText;
        [SerializeField] private RectTransform _sliderFlag;

        public Image SliderImage => _frontSliderImage;
        public RectTransform SliderFlag => _sliderFlag;
        public TMP_Text CupsAmountText => _cupsAmountText;
    }
}

