using RaceManager.Cars;
using RaceManager.Race;
using RaceManager.Root;
using System;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

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

        public void Init(DriverProfile driverProfile, PlayerProfile playerProfile, UnityAction actionForRespawnButton, UnityAction actionForGetToCheckpointButton)
        {
            driverProfile.CarState.Subscribe(playerCarState => ChangeViewDependingOn(playerCarState));

            _finishUI.gameObject.SetActive(false);
            _finishUI.OkButtonFinish.onClick.AddListener(ShowExtraRewardPanel);
            _finishUI.OkButtonExtraReward.onClick.AddListener(FinalizeRace);

            _inRaceUI.gameObject.SetActive(true);
            _inRaceUI.RaceProgressBar.LevelText.text = "LEVEL " + (int)playerProfile.nextLevelPrefabToLoad;

            _inRaceUI.RespawnCarButton.AddListener(actionForRespawnButton);
            _inRaceUI.GetToCheckpointButton.AddListener(actionForGetToCheckpointButton);
        }

        private void Update()
        {
            if (_isRaceFinished)
                return;

            ShowSpeed();
            HandlePositionIndication();
            HandleProgressBar();

            if (Input.GetMouseButtonDown(0))
            {
                StopAllCoroutines();
                StartCoroutine(HandleAccelerationRepresentation(true));
            }

            if (Input.GetMouseButtonUp(0))
            {
                StopAllCoroutines();
                StartCoroutine(HandleAccelerationRepresentation(false));
            }
        }

        private IEnumerator HandleAccelerationRepresentation(bool isAccelerating)
        {
            int lerpToValue = isAccelerating ? 1 : 0;
            while (!Mathf.Approximately(_inRaceUI.SpeedIndicator.AccelerationIntenseImage.fillAmount, lerpToValue))
            {
                _inRaceUI.SpeedIndicator.AccelerationIntenseImage.fillAmount = 
                    Mathf.Lerp
                    (
                        _inRaceUI.SpeedIndicator.AccelerationIntenseImage.fillAmount, 
                        lerpToValue, 
                        Time.deltaTime
                    );

                yield return null;
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
            EventsHub<RaceEvent>.BroadcastNotification(RaceEvent.QUIT);
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
            _currentPosition = (int)profile.PositionInRace;
        }

        public void OnCompleted() => throw new NotImplementedException();
        public void OnError(Exception error) => throw error;
    }
}
