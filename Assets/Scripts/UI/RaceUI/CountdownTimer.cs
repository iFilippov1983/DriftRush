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
            _currentTime = _countdownDuration;
            while (_currentTime > 0)
            {
                _countdownTimerView.Show(Mathf.RoundToInt(_currentTime));
                yield return new WaitForSeconds(1f);
                _currentTime--;
            }

            RaceEventsHub.Notify(RaceEventType.START);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}