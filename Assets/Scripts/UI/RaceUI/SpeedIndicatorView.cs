using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class SpeedIndicatorView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _speedValueText;
        [SerializeField] private Image _accelerationIntenseImage;

        public TMP_Text SpeedValueText => _speedValueText;
        public Image AccelerationIntenseImage => _accelerationIntenseImage;

        private void Awake()
        {
            _accelerationIntenseImage.fillAmount = 0f;
        }
    }
}

