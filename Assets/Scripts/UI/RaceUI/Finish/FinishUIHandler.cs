using RaceManager.Race;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UniRx.Triggers;
using UnityEngine.UI;
using RaceManager.Tools;
using System;

namespace RaceManager.UI
{
    public class FinishUIHandler : IAnimatablePanelsHandler
    {
        private RectTransform _titleRect;
        private TMP_Text _titleText;
        private TMP_Text _posText;

        private MoneyRewardPanel _moneyRewardPanel;
        private CupsRewardPanel _cupsRewardPanel;
        private LootboxRewardPanel _lootboxRewardPanel;

        private RaceRewardInfo _rewardInfo;
        private bool _grantLootbox = false;
        private float _duration;

        private int _totalScores;

        public bool HasJob { get; private set; }

        public FinishUIHandler(RectTransform titleRect, TMP_Text titleText, TMP_Text posText, float animationDuration = 0.7f)
        {
            _titleRect = titleRect;
            _titleText = titleText;
            _posText = posText;
            _duration = animationDuration;
        }

        public Subject<(string bName, bool isFinal)> OnButtonPressed = new Subject<(string bName, bool isFinal)>();
        public Subject<Unit> OnWatchAds = new Subject<Unit>();

        public void Handle(MoneyRewardPanel moneyRewardPanel)
        {
            _moneyRewardPanel = moneyRewardPanel;
            _moneyRewardPanel.SetActive(false);
            _moneyRewardPanel.ContinueButton.onClick.AddListener(() => HideMoneyRewardPanel()?.AddTo(_moneyRewardPanel));
            _moneyRewardPanel.MultiplyRewardPanel.WatchAdsButton.onClick.AddListener(OnWatchAdsInit);
        }

        public void Handle(CupsRewardPanel cupsRewardPanel)
        {
            _cupsRewardPanel = cupsRewardPanel;
            _cupsRewardPanel.SetActive(false);
            _cupsRewardPanel.ContinueButton.onClick.AddListener(() => HideCupsRewardPanel()?.AddTo(_cupsRewardPanel));
        }

        public void Handle(LootboxRewardPanel lootboxRewardPanel)
        {
            _lootboxRewardPanel = lootboxRewardPanel;
            _lootboxRewardPanel.SetActive(false);
            _lootboxRewardPanel.ClaimButton.onClick.AddListener(() => HideLootboxRewardPanel()?.AddTo(_lootboxRewardPanel));
        }

        public void SetLootboxRewardPanel(bool grantLootbox, Sprite lootboxSprite)
        { 
            _grantLootbox = grantLootbox;
            _lootboxRewardPanel.LootboxImage.sprite = lootboxSprite;
        }

        public IDisposable ShowMoneyRewardPanel(RaceRewardInfo info)
        {
            _rewardInfo = info;
            HasJob = true;
            _moneyRewardPanel.SetActive(true);
            _moneyRewardPanel.MultiplyRewardPanel.WatchAdsButton.interactable = false;
            _moneyRewardPanel.MultiplyRewardPanel.MultiplyerValueText.text = string.Concat("x", info.MoneyMultiplyer.ToString());

            foreach (var panel in _moneyRewardPanel.RewardPanels)
            {
                int value = panel.ScoreType switch
                {
                    RaceScoresType.Drift => _rewardInfo.MoneyRewardDrift,
                    RaceScoresType.Bump => _rewardInfo.MoneyRewardBump,
                    RaceScoresType.Crush => _rewardInfo.MoneyRewardCrush,
                    RaceScoresType.Finish => _rewardInfo.MoneyRewardFinishPos,
                    _ => 0,
                };

                panel.RewardAmountText.text = value.ToString();
            }

            Tween titleTween = _titleRect.DOMove(_moneyRewardPanel.TitlePosition.position, _duration);

            Sequence appearSequence = DOTween.Sequence();

            foreach (var sPanel in _moneyRewardPanel.ShowPanels)
            {
                sPanel.SetActive(true);
                appearSequence.Append(sPanel.ShowRect.DOMove(sPanel.HideRect.position, _duration / 2f).From());
            }

            float d = appearSequence.Duration();

            foreach (var rPanel in _moneyRewardPanel.RewardPanels)
            {
                appearSequence
                    .Insert(d, rPanel.ShowRect.DOMove(rPanel.HideRect.position, _duration).From())
                    .AppendCallback(() => ScrambleScoresTotal(rPanel.ScoreType)
                    ?.AddTo(rPanel.ShowRect));

                d += _duration;
            }

            appearSequence.AppendCallback(() =>
            {
                //_moneyRewardPanel.ContinueButton.interactable = true;
                _moneyRewardPanel.MultiplyRewardPanel.WatchAdsButton.interactable = true;
                HasJob = false;
            });

            return Disposable.Create(() =>
            {
                appearSequence.Complete();
                appearSequence = null;

                titleTween.Complete();
                titleTween = null;
            });
        }

        private IDisposable HideMoneyRewardPanel(Tween tweenInterrupt = null)
        {
            HasJob = true;
            _moneyRewardPanel.ContinueButton.onClick.RemoveAllListeners();
            _moneyRewardPanel.ContinueButton.onClick.AddListener(() => 
            {
                if (tweenInterrupt != null)
                {
                    tweenInterrupt.Complete();
                    tweenInterrupt = null;
                }
                    
                _moneyRewardPanel.ContinueButton.interactable = false;
                _moneyRewardPanel.SetActive(false);
                HasJob = false;
                ShowCupsRewardPanel()?.AddTo(_cupsRewardPanel);
            });

            _moneyRewardPanel.MultiplyRewardPanel.WatchAdsButton.interactable = false;

            Sequence disappearSequence = DOTween.Sequence();

            float d = 0f;

            foreach (var hPanel in _moneyRewardPanel.HidePanels)
            {
                Vector3 pos = hPanel.ShowRect.position;
                disappearSequence.Append(hPanel.ShowRect.DOMove(hPanel.HideRect.position, _duration / 2f));
                disappearSequence.AppendCallback(() =>
                {
                    hPanel.ShowRect.position = pos;
                    hPanel.SetActive(false);
                });
            }

            foreach (var rPanel in _moneyRewardPanel.RewardPanels)
            {
                disappearSequence.Insert(d, rPanel.ShowRect.DOMove(rPanel.HideRect.position, _duration / 2));
                d += _duration / 4;
            }

            disappearSequence.AppendCallback(() => 
            {
                _moneyRewardPanel.SetActive(false);

                HasJob = false;

                ShowCupsRewardPanel()?.AddTo(_cupsRewardPanel);
            });

            OnButtonPressed.OnNext
                ((
                    bName: _moneyRewardPanel.ContinueButton.name, 
                    isFinal: false
                ));

            return Disposable.Create(() =>
            {
                disappearSequence.Complete();
                disappearSequence = null;
            });
        }

        private IDisposable ShowCupsRewardPanel()
        {
            HasJob = true;
            _cupsRewardPanel.SetActive(true);

            _cupsRewardPanel.CupsRewardText.text = _rewardInfo.CupsRewardAmount.ToString();
            _cupsRewardPanel.CupsTotalText.text = _rewardInfo.CupsTotalAmount.ToString();

            Tween titleTween = _titleRect.DOJump(_cupsRewardPanel.TitlePosition.position, _duration, 1, _duration * 2);

            Sequence appearSequence = DOTween.Sequence();

            foreach (var sPanel in _cupsRewardPanel.ShowPanels)
            {
                sPanel.SetActive(true);
                appearSequence.Append(sPanel.ShowRect.DOMove(sPanel.HideRect.position, _duration / 2f).From());
            }

            appearSequence.AppendInterval(_duration);

            int newTotal = _rewardInfo.CupsRewardAmount + _rewardInfo.CupsTotalAmount;
            appearSequence.Append(_cupsRewardPanel.CupsRewardText.DOText("0", _duration, true, ScrambleMode.Numerals));
            appearSequence.Join(_cupsRewardPanel.CupsTotalText.DOText(newTotal.ToString(), _duration, true, ScrambleMode.Numerals));

            appearSequence.AppendCallback(() =>
            {
                _cupsRewardPanel.CupsRewardText.SetActive(false);
                HasJob = false;
            });

            return Disposable.Create(() =>
            {
                appearSequence.Complete();
                appearSequence = null;

                titleTween.Complete();
                titleTween = null;
            });
        }

        private IDisposable HideCupsRewardPanel(Tween tweenToInterrupt = null)
        {
            HasJob = true;
            bool final = !_grantLootbox;

            _cupsRewardPanel.ContinueButton.onClick.RemoveAllListeners();

            _cupsRewardPanel.ContinueButton.onClick.AddListener(() => 
            {
                if (tweenToInterrupt != null)
                { 
                    tweenToInterrupt.Complete();
                    tweenToInterrupt = null;
                }

                _cupsRewardPanel.SetActive(false);

                HasJob = false;

                if (_grantLootbox)
                    ShowLootboxRewardPanel()?.AddTo(_lootboxRewardPanel);

                OnButtonPressed.OnNext
                ((
                    bName: _cupsRewardPanel.ContinueButton.name,
                    isFinal: final
                ));
            });

            Sequence disappearSequence = DOTween.Sequence();

            foreach (var hPanel in _cupsRewardPanel.HidePanels)
            {
                Vector3 pos = hPanel.ShowRect.position;
                disappearSequence.Append(hPanel.ShowRect.DOMove(hPanel.HideRect.position, _duration / 4f));
                disappearSequence.AppendCallback(() =>
                {
                    hPanel.ShowRect.position = pos;
                    hPanel.SetActive(false);
                });
            }

            disappearSequence.AppendCallback(() =>
            {
                _cupsRewardPanel.SetActive(false);

                HasJob = false;

                if(_grantLootbox)
                    ShowLootboxRewardPanel()?.AddTo(_lootboxRewardPanel);
            });

            OnButtonPressed.OnNext
                ((
                    bName: _cupsRewardPanel.ContinueButton.name,
                    isFinal: final
                ));

            return Disposable.Create(() =>
            {
                disappearSequence.Complete();
                disappearSequence = null;
            });
        }

        private IDisposable ShowLootboxRewardPanel()
        {
            HasJob = true;
            _lootboxRewardPanel.SetActive(true);

            Sequence titleSequence = DOTween.Sequence();
            float initialScale = _titleRect.localScale.x;

            titleSequence.Append(_titleRect.DOScale(0, _duration / 2));
            titleSequence.AppendCallback(() =>
            {
                _posText.SetActive(false);
                _titleText.alignment = TextAlignmentOptions.Center;
                _titleText.text = TextConstant.Bonus.ToUpper();
            });
            titleSequence.Append(_titleRect.DOScale(initialScale, _duration / 2));
            titleSequence.Join(_titleRect.DOMove(_lootboxRewardPanel.TitlePosition.position, _duration / 2));

            titleSequence.Play();

            Sequence appearSequence = DOTween.Sequence();

            foreach (var sPanel in _lootboxRewardPanel.ShowPanels)
            {
                sPanel.SetActive(true);
                appearSequence.Append(sPanel.ShowRect.DOMove(sPanel.HideRect.position, _duration / 2).From());
            }

            appearSequence.Append(_lootboxRewardPanel.LootboxImage.rectTransform.DOScale(Vector3.zero, _duration / 2).From());
            appearSequence.Join(_lootboxRewardPanel.EffectImage.rectTransform.DOScale(Vector3.zero, _duration / 2).From());

            appearSequence.AppendCallback(() =>
            {
                HasJob = false;
            });

            float d = _duration * 15;
            Sequence rotateSequence = DOTween.Sequence();
            rotateSequence.Append(_lootboxRewardPanel.EffectImage.rectTransform.DORotate(new Vector3(0, 0, 360f), d, RotateMode.FastBeyond360));
            rotateSequence.Append(_lootboxRewardPanel.EffectImage.rectTransform.DORotate(Vector3.zero, d, RotateMode.FastBeyond360));
            rotateSequence.AppendCallback(() =>
            {
                if(_lootboxRewardPanel != null)
                    rotateSequence.Restart();
            });

            return Disposable.Create(() => 
            {
                titleSequence.Complete();
                titleSequence = null;

                appearSequence.Complete();
                appearSequence = null;

                rotateSequence.Complete();
                rotateSequence = null;
            });
        }

        private IDisposable HideLootboxRewardPanel(Tween tweenToInterrupt = null) 
        {
            HasJob = true;
            _lootboxRewardPanel.ClaimButton.onClick.RemoveAllListeners();

            _lootboxRewardPanel.ClaimButton.onClick.AddListener(() => 
            {
                if (tweenToInterrupt != null)
                { 
                    tweenToInterrupt.Complete();
                    tweenToInterrupt = null;
                }

                HasJob = false;
                _lootboxRewardPanel.SetActive(false);

                OnButtonPressed.OnNext
                ((
                    bName: _lootboxRewardPanel.ClaimButton.name,
                    isFinal: true
                ));
            });

            Sequence disappearSequence = DOTween.Sequence();

            foreach (var hPanel in _lootboxRewardPanel.HidePanels)
            {
                Vector3 pos = hPanel.ShowRect.position;
                disappearSequence.Append(hPanel.ShowRect.DOMove(hPanel.HideRect.position, _duration / 5));
                disappearSequence.AppendCallback(() =>
                { 
                    hPanel.ShowRect.position = pos;
                    hPanel.SetActive(false);
                });
            }

            disappearSequence.Append(_lootboxRewardPanel.LootboxImage.rectTransform.DOScale(Vector3.zero, _duration / 5));
            disappearSequence.Join(_lootboxRewardPanel.EffectImage.rectTransform.DOScale(Vector3.zero, _duration / 5));

            disappearSequence.AppendCallback(() =>
            {
                HasJob = false;
            });

            OnButtonPressed.OnNext
                ((
                    bName: _lootboxRewardPanel.ClaimButton.name,
                    isFinal: true
                ));

            return Disposable.Create(() =>
            {
                disappearSequence.Complete();
                disappearSequence = null;
            });
        }

        private IDisposable ScrambleScoresTotal(RaceScoresType scoresType)
        {
            int value = scoresType switch
            {
                RaceScoresType.Drift => _rewardInfo.MoneyRewardDrift,
                RaceScoresType.Bump => _rewardInfo.MoneyRewardBump,
                RaceScoresType.Crush => _rewardInfo.MoneyRewardCrush,
                RaceScoresType.Finish => _rewardInfo.MoneyRewardFinishPos,
                _ => 0,
            };

            if (value == 0) return Disposable.Empty;

            Sequence scrambleSequence = DOTween.Sequence();

            _totalScores += value;
            scrambleSequence.Append(_moneyRewardPanel.MoneyTotalText.DOText(_totalScores.ToString(), _duration, true, ScrambleMode.Numerals));

            int multyValue = _totalScores * _rewardInfo.MoneyMultiplyer;
            scrambleSequence.Join(_moneyRewardPanel.MultiplyRewardPanel.RewardTotalText.DOText(multyValue.ToString(), _duration, true, ScrambleMode.Numerals));

            return Disposable.Create(() =>
            {
                scrambleSequence.Complete();
                scrambleSequence = null;
            });
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

        private void OnWatchAdsInit()
        {
            string name = _moneyRewardPanel.MultiplyRewardPanel.WatchAdsButton.name;
            OnButtonPressed.OnNext((bName: name, isFinal: false));
            OnWatchAds.OnNext();
        }

        public IDisposable OnWatchAdsSuccess()
        {
            Sequence scrambleSequence = DOTween.Sequence();
            scrambleSequence.Append(_moneyRewardPanel.MultiplyRewardPanel.ShowRect
                .DOMove(_moneyRewardPanel.MultiplyRewardPanel.HideRect.position, _duration / 3));

            int multyValue = _totalScores * _rewardInfo.MoneyMultiplyer;
            scrambleSequence.Join(_moneyRewardPanel.MoneyTotalText.DOText(multyValue.ToString(), _duration, true, ScrambleMode.Numerals));

            return Disposable.Create(() =>
            {
                scrambleSequence.Complete();
                scrambleSequence = null;
            });
        }
    }
}
