using RaceManager.Cars;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace RaceManager.UI
{
    public class RaceUI : MonoBehaviour, IObserver<DriverProfile>
    {
        [SerializeField] private RaceUIView _inRaceUI;
        [SerializeField] private FinishUIView _finishUI;

        private float _currentSpeed;
        private float _trackProgress;
        private int _currentPosition;

        private bool _isRaceFinished;

        public void Init(DriverProfile profile, UnityAction actionForRespawnButton)
        {
            profile.CarState.Subscribe(playerCarState => ChangeViewDependingOn(playerCarState));

            _finishUI.gameObject.SetActive(false);
            _finishUI.OkButtonFinish.onClick.AddListener(ShowExtraRewardPanel);
            _finishUI.OkButtonExtraReward.onClick.AddListener(FinalizeRace);

            _inRaceUI.gameObject.SetActive(true);
            _inRaceUI.RespawnCarButton.AddListener(actionForRespawnButton);
        }

        private void Update()
        {
            if (!_isRaceFinished)
            {
                ShowSpeed();
                HandlePositionIndication();
                HandleProgressBar();
            }
        }

        private void ShowSpeed()
        {
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

        private void SetFinishValues()
        {
            _finishUI.MoneyAmount.text = "999";
            _finishUI.GemsAmount.text = "999";

            _finishUI.PositionText.text = GetPositionText();

            _finishUI.RewardMoneyAmountText.text = "99";
            _finishUI.RewardCupsAmountText.text = "99";

            //animate?
            _finishUI.FillUpImage.fillAmount = 0.8f;
            _finishUI.PersentageProgressText.text = "80" + "%";
        }

        private void ShowRaceUI()
        {
            _isRaceFinished = false;
            _inRaceUI.gameObject.SetActive(true);
        }

        private void ShowFinishUI()
        {
            _isRaceFinished = true;
            _inRaceUI.gameObject.SetActive(false);

            SetFinishValues();
            _finishUI.gameObject.SetActive(true);
        }

        private void FinalizeRace()
        {
            Loader.Load(Loader.Scene.MenuScene);
        }

        private string GetPositionText()
        {
            if (_currentPosition == 1)
                return string.Concat(_currentPosition, "st");
            else if (_currentPosition == 2 || _currentPosition == 3)
                return string.Concat(_currentPosition, "d");
            else
                return string.Concat(_currentPosition, "th");
        }

        private void ShowExtraRewardPanel()
        {
            _finishUI.FinishPanel.gameObject.SetActive(false);
            _finishUI.ExtraRewardPanel.gameObject.SetActive(true);
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
