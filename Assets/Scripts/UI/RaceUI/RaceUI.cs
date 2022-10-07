using RaceManager.Cars;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace RaceManager.UI
{
    public class RaceUI : MonoBehaviour, IObserver<DriverProfile>
    {
        private const float KPHFactor = 3.6f;
        private const float MPHFactor = 2.23693629f;

        [SerializeField] private SpeedType _speedType = SpeedType.KPH;
        [SerializeField] private PositionIndicatorView _positionIndicatorView;
        [SerializeField] private SpeedIndicatorView _speedIndicatorView;
        [SerializeField] private RaceProgressBarView _raceProgressBarView;

        private float _currentSpeed;
        private float _trackProgress;
        private int _currentPosition;

        private void Update()
        {
            ShowSpeed();

            bool isActive = _currentPosition > 0 ? true : false;
            _positionIndicatorView.PositionText.gameObject.SetActive(isActive);
            _positionIndicatorView.PositionText.text = _currentPosition.ToString();

            _raceProgressBarView.ProgressImage.fillAmount = _trackProgress;
        }

        private void ShowSpeed()
        {
            float factor = _speedType == SpeedType.KPH ? KPHFactor : MPHFactor;
            _currentSpeed *= factor;
            int speed = Mathf.RoundToInt(_currentSpeed);

            _speedIndicatorView.SpeedValueText.text = speed.ToString();
        }


        public void OnNext(DriverProfile profile)
        {
            _currentSpeed = profile.CarCurrentSpeed;
            _trackProgress = profile.TrackProgress;
            _currentPosition = profile.PositionInRace;
        }

        public void OnCompleted() => throw new NotImplementedException();
        public void OnError(Exception error) => throw error;
    }
}

