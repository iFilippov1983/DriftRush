using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class RaceProgressBarView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Image _progressImage;

        private void OnEnable()
        {
            _progressImage.fillAmount = 0;
        }

        public Image ProgressImage => _progressImage;
        public TMP_Text LevelText => _levelText;
    }
}
