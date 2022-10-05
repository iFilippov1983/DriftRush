using UnityEngine;
using TMPro;

namespace RaceManager.UI
{
    public class CountdownTimerView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _countdownText;
        private Animation _animation;
        private AudioSource _audioSource;

        public TMP_Text CountdownText => _countdownText;
        public Animation Animation => _animation;
        public AudioSource AudioSource => _audioSource;


        private void Awake()
        {
            _animation = GetComponent<Animation>();
            _audioSource = GetComponent<AudioSource>();
        }
    }
}
