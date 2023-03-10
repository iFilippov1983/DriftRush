using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager
{
    public class LoadingProgressBar : MonoBehaviour
    {
        private const string Loading = "LOADING";

        [SerializeField] private TMP_Text _loadingText;
        [SerializeField] private Image _fillImage;
        [SerializeField] private float _animDuration;

        private int _cycles = 3;
        private int _counter;
        private float _timer;

        private void Awake()
        {
            _fillImage.fillAmount = 0f;
            _timer = _animDuration / _cycles;
            _counter = 0;
        }

        private void FixedUpdate()
        {
            AnimateText();

            float fill = Mathf.Lerp(_fillImage.fillAmount, Loader.GetLoadingProgress(), 0.05f);
            _fillImage.fillAmount = fill;
        }

        private void AnimateText()
        {
            _timer -= Time.fixedDeltaTime;

            if (_timer < 0)
            {
                if (_counter > 0)
                {
                    _timer = _animDuration / _cycles;
                    _counter--;

                    _loadingText.text += ".";
                }
                else
                {
                    _timer = _animDuration / _cycles;
                    _counter = _cycles;

                    _loadingText.text = Loading;
                }
            }
        }
    }
}
