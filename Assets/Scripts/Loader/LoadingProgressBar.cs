using UnityEngine;
using UnityEngine.UI;

namespace RaceManager
{
    public class LoadingProgressBar : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;

        private void Awake()
        {
            _fillImage.fillAmount = 0f;
        }

        void FixedUpdate()
        {
            float fill = Mathf.Lerp(_fillImage.fillAmount, Loader.GetLoadingProgress(), 0.2f);
            _fillImage.fillAmount = fill;
        }
    }
}
