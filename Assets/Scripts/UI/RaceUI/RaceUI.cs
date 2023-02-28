using RaceManager.Cars;
using RaceManager.Race;
using RaceManager.Root;
using RaceManager.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;
using DG.Tweening;
using UniRx;
using TMPro;
using UniRx.Triggers;

namespace RaceManager.UI
{
    public class RaceUI : MonoBehaviour, IObserver<DriverProfile>
    {
        [SerializeField] private RaceUIView _inRaceUI;
        [SerializeField] private FinishUIView _finishUI;
        [SerializeField] private float _extraScoresAnimDuration = 0.7f;

        private GameObject _extraScoresIndicatorPrefab;
        private FinishUIHandler _finishUIHandler;
        private SpritesContainerRewards _spritesRewards;

        private float _currentSpeed;
        private float _trackProgress;
        private int _currentPosition;
        private int _currentScores;
        private int _currentExtraScores;
        private int _targetExtraScores;

        private bool _isRaceFinished;

        private Stack<IEnumerator> _jobsStack = new Stack<IEnumerator>();
        private Stack<ExtraScoresIndicatorView> _extraScoresStack = new Stack<ExtraScoresIndicatorView>();

        public Action<string> OnButtonPressed;
        public Action OnAdsInit;

        private ScoresIndicatorView ScoresIndicator => _inRaceUI.ScoresIndicator;
        private GameObject ExtraScoresPrefab
        {
            get 
            {
                if (_extraScoresIndicatorPrefab is null)
                    _extraScoresIndicatorPrefab = ResourcesLoader.LoadPrefab(ResourcePath.ExtraScoresIndicatorViewPrefab);

                return _extraScoresIndicatorPrefab;
            }
        }

        public bool RaceFinished => _isRaceFinished;

        [Inject]
        private void Construct(SpritesContainerRewards spritesContainer)
        { 
            _spritesRewards = spritesContainer;
            _finishUIHandler = new FinishUIHandler(_finishUI.TitleRect, _finishUI.FinishTitleText, _finishUI.PositionText);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            _extraScoresStack.Clear();
            _jobsStack.Clear();
        }

        #region Public Functions

        public void Initialize(RaceLevelInitializer levelInitializer, UnityAction actionForRespawnButton, UnityAction actionForGetToCheckpointButton)
        {
            _inRaceUI.gameObject.SetActive(true);
            _inRaceUI.RaceProgressBar.LevelText.text = ("LEVEL " + levelInitializer.LevelName).ToUpper();

            _inRaceUI.RespawnCarButton.AddListener(actionForRespawnButton);
            _inRaceUI.GetToCheckpointButton.AddListener(actionForGetToCheckpointButton);

            ScoresIndicator.ScoresRect.SetActive(false);
            ScoresIndicator.ExtraScoresRect.SetActive(false);

            foreach (var panel in _finishUI.AnimatablePanels)
            {
                panel.Accept(_finishUIHandler);
            }

            _finishUIHandler.OnButtonPressed
                .Subscribe(t => 
                {
                    OnButtonPressed?.Invoke(t.bName);

                    if(t.isFinal)
                        StartCoroutine(FinalizeRace());
                })
                .AddTo(this);

            _finishUIHandler.OnWatchAds
                .Subscribe(t => OnAdsInit?.Invoke())
                .AddTo(this);

            _finishUI.gameObject.SetActive(false);
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

        public void SetFinishValues(RaceRewardInfo info) 
        {
            _finishUI.FinishTitleText.text = TextConstant.Finished.ToUpper();
            _finishUI.PositionText.text = GetPositionText().ToUpper();

            _finishUIHandler.ShowMoneyRewardPanel(info);
        }

        public void SetLootboxToGrant(Rarity rarity)
        {
            Sprite lootboxSprite = _spritesRewards.GetLootboxSprite(rarity);
            _finishUIHandler.SetLootboxRewardPanel(true, lootboxSprite);
        }

        public void ShowScores(bool show, int scoresValue = 0)
        {
            ScoresIndicator.ScoresRect.SetActive(show);
            _currentScores = scoresValue + _currentExtraScores;

            string text = show ? _currentScores.ToString() : string.Empty;
            ScoresIndicator.ScoresText.text = text;

            if (!show)
            {
                _currentExtraScores = 0;
                _targetExtraScores = 0;
                foreach (var job in _jobsStack)
                    StopCoroutine(job);

                _jobsStack.Clear();
            }
        }

        public void ShowPause(bool show, int pauseTime = 0)
        {
            ScoresIndicator.PauseTimerText.SetActive(show);

            string text = show ? pauseTime.ToString() : string.Empty;
            ScoresIndicator.PauseTimerText.text = text;
        }

        public void ShowExtraScores(RaceScoresType scoresType, int scoresValue)
        {
            ExtraScoresIndicatorView indicator;
            if (_extraScoresStack.Count == 0)
            {
                GameObject indicatorGo = Instantiate(ExtraScoresPrefab, ScoresIndicator.ExtraScoresRect.transform, false);
                indicator = indicatorGo.GetComponent<ExtraScoresIndicatorView>();
            }
            else
            { 
                indicator = _extraScoresStack.Pop();
                indicator.SetActive(true);
                indicator.transform.position = ScoresIndicator.ExtraScoresRect.transform.position;
            }

            string titleText = scoresType.ToString().ToUpper();
            indicator.ExtraScoresTitle.text = titleText;
            indicator.ExtraScoresText.text = scoresValue.ToString();

            ScoresIndicator.ExtraScoresRect.SetActive(true);
            AnimateExtraScores(indicator, scoresValue);
        }

        public void ShowSpeed()
        {
            int speed = Mathf.RoundToInt(_currentSpeed);

            _inRaceUI.SpeedIndicator.SpeedValueText.text = speed.ToString();
        }

        public void HandlePositionIndication()
        {
            bool isActive = _currentPosition > 0 ? true : false;
            _inRaceUI.PositionIndicator.PositionText.gameObject.SetActive(isActive);
            _inRaceUI.PositionIndicator.PositionText.text = _currentPosition.ToString();
        }

        public void HandleProgressBar() 
        {
            _inRaceUI.RaceProgressBar.ProgressImage.fillAmount = _trackProgress;
        }

        public void HandleAcceleration(bool accelerating) 
        {
            StopAllCoroutines();
            StartCoroutine(HandleAccelerationRepresentation(accelerating));
        }

        public void OnAdsRewardAction()
        {
            _finishUIHandler.OnWatchAdsSuccess();
        }

        #endregion

        #region Private Functions

        private void AnimateExtraScores(ExtraScoresIndicatorView indicator, int scoresValue)
        {
            if (ScoresIndicator.ScoresRect.gameObject.activeSelf)
            {
                IEnumerator job = ExtrapolateExtraScores(scoresValue);
                _jobsStack.Push(job);
                StartCoroutine(job);
            }

            Vector3 initialScale = indicator.Rect.localScale;
            Color initialTextColor = indicator.ExtraScoresText.color;

            float duration = _extraScoresAnimDuration;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(indicator.Rect.DOMove(ScoresIndicator.ScoresRect.transform.position, duration));
            sequence.Insert(duration / 2, indicator.ExtraScoresText.DOFade(0f, duration / 2));
            sequence.Insert(duration / 2, indicator.ExtraScoresTitle.DOFade(0f, duration / 2));
            sequence.OnComplete(OnComplete);

            void OnComplete()
            {
                indicator.Rect.localScale = initialScale;
                indicator.ExtraScoresText.color = initialTextColor;
                indicator.ExtraScoresTitle.color = initialTextColor;
                indicator.SetActive(false);
                _extraScoresStack.Push(indicator);
            }
        }

        private IEnumerator ExtrapolateExtraScores(int extraScoresValue)
        {
            _targetExtraScores += extraScoresValue;

            while (_currentExtraScores < _targetExtraScores) 
            {
                _currentExtraScores += _jobsStack.Count + 1;
                yield return null;
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

        private void ScrambleText(int targetValue, Text intermediateText, TMP_Text tmpText, float duration)
        {
            string text = string.Empty;

            intermediateText.DOText(targetValue.ToString(), duration, true, ScrambleMode.Numerals);
            intermediateText.UpdateAsObservable()
                .Where(_ => text != intermediateText.text)
                .Subscribe(_ =>
                {
                    text = intermediateText.text;
                    tmpText.text = text;
                });
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
            _finishUI.gameObject.SetActive(true);
        }

        private IEnumerator FinalizeRace()
        {
            while (_finishUIHandler.HasJob)
                yield return null;

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

        #endregion

        #region Observer Functions

        public void OnNext(DriverProfile profile)
        {
            _currentSpeed = profile.CarCurrentSpeed;
            _trackProgress = profile.TrackProgress;
            _currentPosition = (int)profile.PositionInRace;
        }

        public void OnCompleted() => throw new NotImplementedException();
        public void OnError(Exception error) => throw error;

        #endregion
    }
}
