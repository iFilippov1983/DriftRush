using RaceManager.Root;
using System.Collections;
using UnityEngine;

namespace RaceManager.UI
{
    public class CountdownTimer : MonoBehaviour
    {
        [SerializeField] private float _countdownDuration;
        [SerializeField] private CountdownTimer _countdownTimer;
        private float _currentTime;

        private void OnEnable()
        {
            RaceEventsHub.Instance.Subscribe(RaceEventType.COUNTDOWN, StartTimer);
        }

        private void OnDisable()
        {
            RaceEventsHub.Instance.Unsunscribe(RaceEventType.COUNTDOWN, StartTimer);
        }

        private void StartTimer() => StartCoroutine(Countdown());

        private IEnumerator Countdown()
        {
            _currentTime = _countdownDuration;
            while (_currentTime > 0)
            {
                yield return new WaitForSeconds(1f);
                _currentTime--;
            }

            RaceEventsHub.Instance.Notify(RaceEventType.START);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}