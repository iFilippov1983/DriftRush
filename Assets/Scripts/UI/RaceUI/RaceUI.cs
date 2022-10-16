using RaceManager.Cars;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace RaceManager.UI
{
    public class RaceUI : MonoBehaviour, IObserver<DriverProfile>
    {
        private const float KPHFactor = 3.6f;
        private const float MPHFactor = 2.23693629f;

        [SerializeField] private SpeedType _speedType = SpeedType.KPH;
        [SerializeField] private RaceUIView _inRaceUI;
        [SerializeField] private FinishUIView _finishUI;
        //private DriverProfile _profile;

        private float _currentSpeed;
        private float _trackProgress;
        private int _currentPosition;

        private bool _isRaceFinished;

        public void Init(DriverProfile profile, UnityAction actionForRespawnButton)
        {
            //_profile = profile;
            profile.CarState.Subscribe(playerCarState => ChangeViewDependingOn(playerCarState));
            _inRaceUI.RespawnCarButton.AddListener(actionForRespawnButton);
        }

        //private void SetValues(DriverProfile driverProfile)
        //{
        //    _currentSpeed = driverProfile.CarCurrentSpeed;
        //    _trackProgress = driverProfile.TrackProgress;
        //    _currentPosition = driverProfile.PositionInRace;
        //}

        private void Update()
        {
            if (_isRaceFinished)
            {

            }
            else
            {
                ShowSpeed();
                HandlePositionIndication();
                HandleProgressBar();
            }
        }

        private void ShowSpeed()
        {
            //float factor = _speedType == SpeedType.KPH ? KPHFactor : MPHFactor;
            //_currentSpeed *= factor;
            int speed = Mathf.RoundToInt(_currentSpeed);

            _inRaceUI.SpeedIndicator.SpeedValueText.text = speed.ToString();
        }

        private void HandlePositionIndication()
        {
            bool isActive = _currentPosition > 0 ? true : false;
            _inRaceUI.PositionIndicator.PositionText.gameObject.SetActive(isActive);
            _inRaceUI.PositionIndicator.PositionText.text = _currentPosition.ToString();
        }

        private void HandleProgressBar()
        {
            _inRaceUI.RaceProgressBar.ProgressImage.fillAmount = _trackProgress;
        }

        private void ChangeViewDependingOn(CarState playerCarState)
        {
            switch (playerCarState)
            {
                case CarState.InShed:
                    break;
                case CarState.OnTrack:
                    ShowRaceUI();
                    break;
                case CarState.Stuck:
                    break;
                case CarState.Finished:
                    ShowFinishUI();
                    break;
                case CarState.GotHit:
                    break;
            }
        }

        private void ShowRaceUI()
        {
            _isRaceFinished = false;
        }

        private void ShowFinishUI()
        {
            _isRaceFinished = true;
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
