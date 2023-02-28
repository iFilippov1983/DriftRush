using RaceManager.Cars;
using RaceManager.Race;
using RaceManager.Root;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace RaceManager.UI
{
    public class RaceUI : MonoBehaviour, IObserver<DriverProfile>
    {
        [SerializeField] private RaceUIView _inRaceUI;
        [SerializeField] private FinishUIView _finishUI;

        private SpritesContainerRewards _spritesRewards;

        private float _currentSpeed;
        private float _trackProgress;
        private int _currentPosition;

        private bool _isRaceFinished;
        private bool _showLootbox;

        public Action<string> OnButtonPressed;

        [Inject]
        private void Construct(SpritesContainerRewards spritesContainer)
        { 
            _spritesRewards = spritesContainer;
        }

        public void Initialize(RaceLevelInitializer levelInitializer, UnityAction actionForRespawnButton, UnityAction actionForGetToCheckpointButton)
        {
            _inRaceUI.gameObject.SetActive(true);
            _inRaceUI.RaceProgressBar.LevelText.text = "LEVEL " + levelInitializer.LevelName;

            _inRaceUI.RespawnCarButton.AddListener(actionForRespawnButton);
            _inRaceUI.GetToCheckpointButton.AddListener(actionForGetToCheckpointButton);

            _finishUI.gameObject.SetActive(false);
            _finishUI.OkButtonFinish.onClick.AddListener(FinalizeRace);
            _finishUI.OkButtonFinish.onClick.AddListener(() => OnButtonPressedMethod(_finishUI.OkButtonFinish));

            //_finishUI.OkButtonFinish.onClick.AddListener(ShowExtraRewardPanel);
            //_finishUI.OkButtonExtraReward.onClick.AddListener(FinalizeRace);
            //_finishUI.OkButtonExtraReward.onClick.AddListener(() => OnButtonPressedMethod(_finishUI.OkButtonExtraReward));
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

        public void ChangeViewDependingOn(CarState playerCarState)
        {
            switch (playerCarState)
            {
                case CarState.None:
                case CarState.CanStart:
                    break;
                case CarState.OnTrack:
                    ShowRaceUI();
                    break;
                case CarState.Finished:
                    ShowFinishUI();
                    break;
                
            }
        }

        public void SetFinishValues(int moneyReward, int cupsReward, int moneyTotal, int gemsTotal)
        {
            _finishUI.MoneyAmount.text = moneyTotal.ToString();
            _finishUI.GemsAmount.text = gemsTotal.ToString();

            _finishUI.PositionText.text = GetPositionText();

            _finishUI.RewardMoneyAmountText.text = moneyReward.ToString();
            _finishUI.RewardCupsAmountText.text = cupsReward.ToString();

            if (_showLootbox)
                _finishUI.GotLootboxPopup.SetActive(true);

            //animate?
            //_finishUI.FillUpImage.fillAmount = 0.8f;
            //_finishUI.PersentageProgressText.text = "80" + "%";
        }

        public void SetLootboxPopupValues(Rarity rarity)
        {
            _showLootbox = true;
            _finishUI.GotLootboxPopup.RarityText.text = rarity.ToString().ToUpper();
            _finishUI.GotLootboxPopup.LootboxImage.sprite = _spritesRewards.GetLootboxSprite(rarity);
        }

        private void ShowRaceUI()
        {
            _isRaceFinished = false;
            _showLootbox = false;
            _inRaceUI.gameObject.SetActive(true);
        }

        private void ShowFinishUI()
        {
            _isRaceFinished = true;
            _inRaceUI.gameObject.SetActive(false);
            _finishUI.gameObject.SetActive(true);
        }

        private void FinalizeRace()
        {
            EventsHub<RaceEvent>.BroadcastNotification(RaceEvent.QUIT);
        }

        private string GetPositionText()
        {
            if (_currentPosition == 0)
                return "DNF";
            else if (_currentPosition == 1)
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

        private void OnButtonPressedMethod(Button button) => OnButtonPressed?.Invoke(button.gameObject.name);

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
