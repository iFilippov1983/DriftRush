using RaceManager.Race;
using RaceManager.Root;
using System.Collections;
using UnityEngine;

namespace RaceManager.UI
{
    public class CountdownTimer : MonoBehaviour
    {
        [SerializeField] private float _countdownDuration;
        [SerializeField] private CountdownTimerView _countdownTimerView;
        private float _currentTime;

        private void Awake()
        {
            EventsHub<RaceEvent>.Subscribe(RaceEvent.COUNTDOWN, StartTimer);
            _countdownTimerView.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            EventsHub<RaceEvent>.Unsunscribe(RaceEvent.COUNTDOWN, StartTimer);
        }

        private void StartTimer() => StartCoroutine(Countdown());

        private IEnumerator Countdown()
        {
            _countdownTimerView.gameObject.SetActive(true);
            _countdownTimerView.CountdownText.gameObject.SetActive(true);
            _countdownTimerView.StartText.gameObject.SetActive(false);

            _currentTime = _countdownDuration;
            while (_currentTime > 0)
            {
                Show(Mathf.RoundToInt(_currentTime));
                yield return new WaitForSeconds(1f);
                _currentTime--;
            }
            _countdownTimerView.CountdownText.gameObject.SetActive(false);
            _countdownTimerView.StartText.gameObject.SetActive(true);
            EventsHub<RaceEvent>.BroadcastNotification(RaceEvent.START);

            yield return new WaitForSeconds(1.5f);

            _countdownTimerView.gameObject.SetActive(false);
        }

        private void Show(int seconds)
        { 
            _countdownTimerView.CountdownText.text = seconds.ToString();
            if(_countdownTimerView.Animation != null)
                _countdownTimerView.Animation.Play();
            if(_countdownTimerView.AudioSource != null)
                _countdownTimerView.AudioSource.Play();
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}