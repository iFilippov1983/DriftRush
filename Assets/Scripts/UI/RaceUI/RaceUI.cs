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
        [SerializeField] private PositionIndicatorView _positionIndicatorView;
        [SerializeField] private SpeedIndicatorView _speedIndicatorView;
        [SerializeField] private RaceProgressBarView _raceProgressBarView;

        private float _currentSpeed;
        private float _trackProgress;
        private int _currentPosition;

        private void OnGUI()
        {
            int speed = Mathf.RoundToInt(_currentSpeed);
            _speedIndicatorView.SpeedValueText.text = speed.ToString();

            _positionIndicatorView.PositionText.text = _currentPosition.ToString();

            _raceProgressBarView.ProgressImage.fillAmount = _trackProgress;
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

