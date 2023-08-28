using RaceManager.Cars;
using RaceManager.Race;
using RaceManager.Root;
using RaceManager.Tools;
using RaceManager.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using TMPro;
using Sirenix.OdinInspector;

namespace RaceManager.UI
{
    public class RaceUI : MonoBehaviour
    {
        private readonly Vector3 _shakeStrengthLight = new Vector3(1.5f, 1f, 0f);
        private readonly Vector3 _shakeStrengthHard = new Vector3(3f, 2f, 0f);

        private readonly Vector3 _punchValueLight = new Vector3(0f, 1.03f, 0f);
        private readonly Vector3 _punchValueHard = new Vector3(1.05f, 1.05f, 1.05f);

        private readonly float _fillAmountThreshold = 0.05f;

        [SerializeField] private RaceUIView _inRaceUI;
        [SerializeField] private FinishUIView _finishUI;
        [SerializeField] private float _scoresAnimDuration = 0.7f;
        [SerializeField] private float _accelerationAnimDuration = 2f;
        [SerializeField] private bool _showDriftPauseTimer;
        [Space]
        [SerializeField] private bool _showPerformance;
        [ShowIf("_showPerformance")]
        [SerializeField] private PerformacePanelView _performancePanelView;
        [ShowIf("_showPerformance")]
        [SerializeField] private int _maxFpsToDisplay = 99;

        private GameObject _extraScoresIndicatorPrefab;
        private GameObject _driftScoreIndicatorPrefab;
        private FinishUIHandler _finishUIHandler;
        private SpritesContainerRewards _spritesRewards;
        private PerformanceDisplayer _fpsDisplayer;

        private Tweener _totalScoresShakeTween;
        private Tweener _driftScoresShakeTween;
        private Tweener _multiplyerTextTween;

        private RaceRewardInfo _rewardInfo;
        private Vector3 _scoresInitialPos;

        private float _currentSpeed;
        private float _trackProgress;

        private int _driversCount;
        private int _currentPosition;

        private bool _isRaceFinished;

        private Stack<DriftScoresIndicatorView> _driftScoresStack = new Stack<DriftScoresIndicatorView>();
        private Stack<ExtraScoresIndicatorView> _collisionScoresStack = new Stack<ExtraScoresIndicatorView>();

        public Action<string> OnButtonPressed;
        public Action OnAdsInit;

        #region Minor variables

        private bool m_Accelerating;

        private Vector3 m_DriftScoresInitalPos;

        private Color m_InitialTotalColor;
        private Color m_InitialTitleColor;
        private Color m_InitialTextColor;

        private Tween m_AccelerationTween;

        #endregion

        private DriftScoresIndicatorView CurrentDriftIndicator { get; set; }
        private UIAnimator Animator => Singleton<UIAnimator>.Instance;
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
        private GameObject DriftScoresPrefab
        {
            get
            {
                if (_driftScoreIndicatorPrefab is null)
                    _driftScoreIndicatorPrefab = ResourcesLoader.LoadPrefab(ResourcePath.DriftScoresIndicatorViewPrefab);

                return _driftScoreIndicatorPrefab;
            }
        }
        private MaxSdkAdvertisement Advertisement => Singleton<MaxSdkAdvertisement>.Instance;

        public bool RaceFinished => _isRaceFinished;
        public bool ShowSimpleFinish { get; set; }

        [Inject]
        private void Construct(SpritesContainerRewards spritesContainer)
        { 
            _spritesRewards = spritesContainer;
            _finishUIHandler = new FinishUIHandler(_finishUI.TitleRect, _finishUI.FinishTitleText, _finishUI.PositionText);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            _collisionScoresStack.Clear();
            _driftScoresStack.Clear();
        }

        #region Public Functions

        public void Initialize(int initialPositionToShow, UnityAction actionForRespawnButton = null, UnityAction actionForGetToCheckpointButton = null)
        {
            InitializeFpsPanel();

            _inRaceUI.gameObject.SetActive(true);

            _inRaceUI.RespawnCarButton.AddListener(actionForRespawnButton);
            _inRaceUI.GetToCheckpointButton.AddListener(actionForGetToCheckpointButton);

            _scoresInitialPos = ScoresIndicator.ScoresRect.position;
            ScoresIndicator.ScoresRect.SetActive(false);
            ScoresIndicator.ExtraScoresRect.SetActive(false);

            foreach (var panel in _finishUI.AnimatablePanels)
            {
                panel.Accept(_finishUIHandler);
            }

            _currentPosition = initialPositionToShow;
            _driversCount = initialPositionToShow;
            _inRaceUI.PositionIndicator.PositionText.gameObject.SetActive(true);
            _inRaceUI.PositionIndicator.PositionText.text = _currentPosition.ToString();
            _inRaceUI.PositionIndicator.DriverTotalText.text = _driversCount.ToString();

            _finishUIHandler.OnButtonPressed
                .Subscribe(t => 
                {
                    OnButtonPressed?.Invoke(t.bName);

                    if (t.isFinal)
                    {
                        FinalizeRace();
                    }
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
                case CarState.Stopped:
                    ShowRewards();
                    break;
            }
        }

        public void SetFinishValues(RaceRewardInfo info) 
        {
            _finishUI.FinishTitleText.text = TextConstant.Finished.ToUpper();
            _finishUI.PositionText.text = GetPositionText().ToUpper();

            _rewardInfo = info;
        }

        public void SetLootboxToGrant(Rarity rarity)
        {
            Sprite lootboxSprite = _spritesRewards.GetLootboxSprite(rarity);
            _finishUIHandler.SetLootboxRewardPanel(true, lootboxSprite);
        }

        public IDisposable ShowScoresTotal(bool show, int scoresTotalValue, int showDuration)
        {
            ScoresIndicator.ScoresRect.SetActive(show);

            if (show)
            {
                ScoresIndicator.ScoresText.DOText(scoresTotalValue.ToString(), _scoresAnimDuration / 2, true, ScrambleMode.Numerals);

                if (_totalScoresShakeTween == null || !_totalScoresShakeTween.IsPlaying())
                {
                    _totalScoresShakeTween = ScoresIndicator.ScoresRect
                            .DOShakeAnchorPos(showDuration, _shakeStrengthHard, 10, 45, false, false, ShakeRandomnessMode.Harmonic)
                            .OnComplete(() =>
                            {
                                ScoresIndicator.ScoresRect.position = _scoresInitialPos;
                                _totalScoresShakeTween = null;

                                ScoresIndicator.ScoresRect.SetActive(false);
                            });
                }
            }
            else
            {
                _totalScoresShakeTween?.Complete(true);
                _totalScoresShakeTween = null;
            }

            return Disposable.Create(() =>
            {
                if (_totalScoresShakeTween != null)
                {
                    _totalScoresShakeTween.Complete(true);
                    _totalScoresShakeTween = null;
                }
            });
        }

        public IDisposable ShowDriftScores(bool show, int scoresValue, float factorValue, float scoresCountTime, bool animateFactor = false)
        {

            if (CurrentDriftIndicator == null || CurrentDriftIndicator.isFinalizing)
            {
                if (_driftScoresStack.Count == 0)
                {
                    GameObject indicatorGo = Instantiate(DriftScoresPrefab, ScoresIndicator.DriftScoresRect.transform, false);
                    CurrentDriftIndicator = indicatorGo.GetComponent<DriftScoresIndicatorView>();
                }
                else
                {
                    CurrentDriftIndicator = _driftScoresStack.Pop();
                    CurrentDriftIndicator.SetActive(true);
                }

                CurrentDriftIndicator.isFinalizing = false;
            }

            CurrentDriftIndicator.ScoreText.SetActive(show);
            CurrentDriftIndicator.ScoreText.text = scoresValue.ToString();

            CurrentDriftIndicator.SignText.SetActive(show);

            CurrentDriftIndicator.MultiplierText.SetActive(show);
            CurrentDriftIndicator.MultiplierText.text = factorValue.ToString();

            if (animateFactor)
            {
                _multiplyerTextTween = CurrentDriftIndicator.MultiplierText.transform.DOPunchScale(_punchValueLight, _scoresAnimDuration / 2, 10, 0);
            }

            CurrentDriftIndicator.TotalText.SetActive(!show);

            ScoresIndicator.DriftScoresRect.SetActive(true);

            if (_driftScoresShakeTween == null || !_driftScoresShakeTween.IsPlaying())
            {
                m_DriftScoresInitalPos = ScoresIndicator.DriftScoresRect.position;

                _driftScoresShakeTween = ScoresIndicator.DriftScoresRect
                        .DOShakeAnchorPos(scoresCountTime, _shakeStrengthLight, 10, 45, false, false, ShakeRandomnessMode.Harmonic)
                        .OnComplete(() =>
                        {
                            ScoresIndicator.DriftScoresRect.position = m_DriftScoresInitalPos;
                            _driftScoresShakeTween = null;
                        });
            }

            if (!show)
            {
                CurrentDriftIndicator.TotalText.text = scoresValue.ToString();

                _driftScoresShakeTween?.Complete(true);
                _driftScoresShakeTween = null;

                AnimateDriftScoresFinalization(CurrentDriftIndicator)?.AddTo(this);
                CurrentDriftIndicator = null;
            }

            return Disposable.Create(() =>
            {
                _multiplyerTextTween?.Complete(true);
                _multiplyerTextTween = null;

                _driftScoresShakeTween?.Complete(true);
                _driftScoresShakeTween = null;
            });
        }

        public void ShowCollisionScores(RaceScoresType scoresType, int scoresValue)
        {
            ExtraScoresIndicatorView indicator;
            if (_collisionScoresStack.Count == 0)
            {
                GameObject indicatorGo = Instantiate(ExtraScoresPrefab, ScoresIndicator.ExtraScoresRect.transform, false);
                indicator = indicatorGo.GetComponent<ExtraScoresIndicatorView>();
            }
            else
            { 
                indicator = _collisionScoresStack.Pop();
                indicator.SetActive(true);
            }

            string titleText = scoresType.ToString().ToUpper();
            indicator.ExtraScoresTitle.text = titleText;
            indicator.ExtraScoresText.text = scoresValue.ToString();

            ScoresIndicator.ExtraScoresRect.SetActive(true);
            AnimateCollisionScoresFinalization(indicator).AddTo(this);
        }

        public void ShowPause(bool show, int pauseTime = 0)
        {
            if (!_showDriftPauseTimer)
            {
                ScoresIndicator.PauseTimerText.SetActive(false);
                return;
            }

            ScoresIndicator.PauseTimerText.SetActive(show);

            string text = show ? pauseTime.ToString() : string.Empty;
            ScoresIndicator.PauseTimerText.text = text;
        }

        public void ShowSpeed()
        {
            int speed = Mathf.RoundToInt(_currentSpeed);

            _inRaceUI.SpeedIndicator.SpeedValueText.text = speed.ToString();
        }

        public void HandlePositionIndication()
        {
            _inRaceUI.PositionIndicator.PositionText.text = _currentPosition.ToString();
        }

        public void HandleProgressBar() 
        {
            _inRaceUI.RaceProgressBar.ProgressImage.fillAmount = _trackProgress;
        }

        public void HandleAcceleration(bool accelerating) 
        {
            if (accelerating == m_Accelerating) return;

            m_Accelerating = accelerating;
            StopAllCoroutines();
            StartCoroutine(HandleAccelerationRepresentation(accelerating));
        }

        public void OnAdsRewardAction()
        {
            _finishUIHandler.OnWatchRewardedAdsSuccess()?.AddTo(this);
        }

        public void UpdateDataFrom(DriverProfile profile)
        {
            _currentSpeed = profile.CarCurrentSpeed;
            _trackProgress = profile.TrackProgress;
            _currentPosition = profile.PositionInRace == 0
                ? _driversCount
                : (int)profile.PositionInRace;
        }

        #endregion

        #region Private Functions

        private IDisposable AnimateDriftScoresFinalization(DriftScoresIndicatorView indicator)
        {
            m_InitialTotalColor = indicator.TotalText.color;
            m_InitialTitleColor = indicator.TitleText.color;

            indicator.isFinalizing = true;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(indicator.Rect.DOMove(ScoresIndicator.DriftScoresMoveToRect.position, _scoresAnimDuration * 2));
            sequence.Join(indicator.Rect.DOPunchScale(_punchValueHard, _scoresAnimDuration / 2, 10, 0));
            sequence.Append(indicator.TotalText.DOFade(0f, _scoresAnimDuration));
            sequence.Join(indicator.TitleText.DOFade(0f, _scoresAnimDuration));
            sequence.OnComplete(OnComplete);

            void OnComplete()
            { 
                indicator.TotalText.color = m_InitialTotalColor;
                indicator.TitleText.color = m_InitialTitleColor;
                indicator.Transform.position = ScoresIndicator.DriftScoresRect.position;
                indicator.isFinalizing = false;

                indicator.SetActive(false);
                _driftScoresStack.Push(indicator);
            }

            return Disposable.Create(() =>
            { 
                sequence?.Complete(true);
                sequence = null;
            });
        }

        private IDisposable AnimateCollisionScoresFinalization(ExtraScoresIndicatorView indicator)
        {
            m_InitialTextColor = indicator.ExtraScoresText.color;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(indicator.Rect.DOMove(ScoresIndicator.ScoresRect.transform.position, _scoresAnimDuration));
            sequence.Insert(_scoresAnimDuration / 2, indicator.ExtraScoresText.DOFade(0f, _scoresAnimDuration / 2));
            sequence.Insert(_scoresAnimDuration / 2, indicator.ExtraScoresTitle.DOFade(0f, _scoresAnimDuration / 2));
            sequence.OnComplete(OnComplete);

            void OnComplete()
            {
                indicator.ExtraScoresText.color = m_InitialTextColor;
                indicator.ExtraScoresTitle.color = m_InitialTextColor;
                indicator.Transform.position = ScoresIndicator.ExtraScoresTransform.position;
                indicator.SetActive(false);
                _collisionScoresStack.Push(indicator);
            }

            return Disposable.Create(() =>
            {
                sequence?.Complete(true);
                sequence = null;
            });
        }

        private IEnumerator HandleAccelerationRepresentation(bool isAccelerating)
        {
            int toValue = isAccelerating ? 1 : 0;

            while (!Mathf.Approximately(_inRaceUI.SpeedIndicator.AccelerationIntenseImage.fillAmount, toValue))
            {
                _inRaceUI.SpeedIndicator.AccelerationIntenseImage.fillAmount = 
                    Mathf.Lerp
                    (
                        _inRaceUI.SpeedIndicator.AccelerationIntenseImage.fillAmount, 
                        toValue, 
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

        private void ShowRewards()
        {
            if (ShowSimpleFinish)
            {
                _finishUIHandler.ShowSimpleFinish()?.AddTo(this);
            }
            else
            {
                _finishUIHandler.ShowMoneyRewardPanel(_rewardInfo)?.AddTo(this);
            }
        }

        private async void FinalizeRace()
        {
            while (_finishUIHandler.HasJob)
                await Task.Yield();

            if (!Advertisement.PlayerRewarded && !ShowSimpleFinish)
            {
                Advertisement.ShowInterstitialAd();
            }

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

        private void InitializeFpsPanel()
        {
            _performancePanelView.SetActive(_showPerformance);

            if (_showPerformance)
            {
                _fpsDisplayer = new PerformanceDisplayer(new PerformanceDisplayData()
                {
                    averageText = _performancePanelView.AverageText,
                    highestText = _performancePanelView.HighestText,
                    lowestText = _performancePanelView.LowestText,
                    monoUsedText = _performancePanelView.MonoUsedText,
                    monoHeapText = _performancePanelView.MonoHeapText,
                    MaxFpsToDisplay = _maxFpsToDisplay,
                });

                this.UpdateAsObservable().Subscribe(_ =>
                {
                    _fpsDisplayer.Display(Time.unscaledDeltaTime);
                }).AddTo(this);
            }
        }

        #endregion
    }
}
