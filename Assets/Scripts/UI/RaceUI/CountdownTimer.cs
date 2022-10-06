using RaceManager.Race;
using RaceManager.Root;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace RaceManager.UI
{
    public class CountdownTimer : MonoBehaviour
    {
        [SerializeField] private float _countdownDuration;
        [SerializeField] private CountdownTimerView _countdownTimerView;
        private float _currentTime;

        private void Awake()
        {
            _countdownTimerView.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            RaceEventsHub.Subscribe(RaceEventType.COUNTDOWN, StartTimer);
        }

        private void OnDisable()
        {
            RaceEventsHub.Unsunscribe(RaceEventType.COUNTDOWN, StartTimer);
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
            RaceEventsHub.BroadcastNotification(RaceEventType.START);

            yield return new WaitForSeconds(1f);

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