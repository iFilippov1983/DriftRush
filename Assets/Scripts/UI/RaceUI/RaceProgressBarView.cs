using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class RaceProgressBarView : MonoBehaviour
    {
        [SerializeField] private Image _progressImage;

        private void OnEnable()
        {
            _progressImage.fillAmount = 0;
        }

        public Image ProgressImage => _progressImage;
    }
}
