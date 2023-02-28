using RaceManager.Race;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using UniRx.Triggers;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using UnityEngine.UIElements;

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
            _moneyRewardPanel.ContinueButton.onClick.AddListener(HideMoneyRewardPanel);
            _moneyRewardPanel.MultiplyRewardPanel.WatchAdsButton.onClick.AddListener(OnWatchAdsInit);
        }

        public void Handle(CupsRewardPanel cupsRewardPanel)
        {
            _cupsRewardPanel = cupsRewardPanel;
            _cupsRewardPanel.SetActive(false);
            _cupsRewardPanel.ContinueButton.onClick.AddListener(HideCupsRewardPanel);
        }

        public void Handle(LootboxRewardPanel lootboxRewardPanel)
        {
            _lootboxRewardPanel = lootboxRewardPanel;
            _lootboxRewardPanel.SetActive(false);
            _lootboxRewardPanel.ClaimButton.onClick.AddListener(HideLootboxRewardPanel);
        }

        public void SetLootboxRewardPanel(bool grantLootbox, Sprite lootboxSprite)
        { 
            _grantLootbox = grantLootbox;
            _lootboxRewardPanel.LootboxImage.sprite = lootboxSprite;
        }

        public void ShowMoneyRewardPanel(RaceRewardInfo info)
        {
            _rewardInfo = info;
            HasJob = true;
            _moneyRewardPanel.SetActive(true);
            _moneyRewardPanel.ContinueButton.interactable = false;
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

            _titleRect.DOMove(_moneyRewardPanel.TitlePosition.position, _duration);

            Sequence appearSequence = DOTween.Sequence();

            foreach (var sPanel in _moneyRewardPanel.ShowPanels)
            {
                appearSequence.Append(sPanel.ShowRect.DOMove(sPanel.HideRect.position, _duration / 2f).From());
            }

            float d = appearSequence.Duration();

            foreach (var rPanel in _moneyRewardPanel.RewardPanels)
            {
                appearSequence
                    .Insert(d, rPanel.ShowRect.DOMove(rPanel.HideRect.position, _duration).From())
                    .AppendCallback(() => ScrambleScoresTotal(rPanel.ScoreType));

                d += _duration;
            }

            appearSequence.AppendCallback(() =>
            {
                _moneyRewardPanel.ContinueButton.interactable = true;
                _moneyRewardPanel.MultiplyRewardPanel.WatchAdsButton.interactable = true;
                HasJob = false;
            });

            appearSequence.Play();
        }

        
        private void HideMoneyRewardPanel()
        {
            HasJob = true;
            _moneyRewardPanel.ContinueButton.interactable = false;
            _moneyRewardPanel.MultiplyRewardPanel.WatchAdsButton.interactable = false;

            Sequence disappearSequence = DOTween.Sequence();

            float d = 0f;

            foreach (var hPanel in _moneyRewardPanel.HidePanels)
            {
                disappearSequence.Append(hPanel.ShowRect.DOMove(hPanel.HideRect.position, _duration / 2f));
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

                ShowCupsRewardPanel();
            });

            disappearSequence.Play();

            OnButtonPressed.OnNext
                ((
                    bName: _moneyRewardPanel.ContinueButton.name, 
                    isFinal: false
                ));
        }

        private void ShowCupsRewardPanel()
        {
            HasJob = true;
            _cupsRewardPanel.SetActive(true);
            _cupsRewardPanel.ContinueButton.interactable = false;

            _cupsRewardPanel.CupsRewardText.text = _rewardInfo.CupsRewardAmount.ToString();
            _cupsRewardPanel.CupsTotalText.text = _rewardInfo.CupsTotalAmount.ToString();

            _titleRect.DOJump(_cupsRewardPanel.TitlePosition.position, _duration, 1, _duration * 2);

            Sequence appearSequence = DOTween.Sequence();

            foreach (var sPanel in _cupsRewardPanel.ShowPanels)
            {
                appearSequence.Append(sPanel.ShowRect.DOMove(sPanel.HideRect.position, _duration / 2f).From());
            }

            float d = appearSequence.Duration();

            appearSequence.AppendCallback(() =>
            {
                _cupsRewardPanel.ContinueButton.interactable = true;
                HasJob = false;
            });

            appearSequence.Play();
        }

        private void HideCupsRewardPanel()
        {
            HasJob = true;
            _cupsRewardPanel.ContinueButton.interactable = false;

            Sequence disappearSequence = DOTween.Sequence();

            float d = 0f;

            foreach (var hPanel in _cupsRewardPanel.HidePanels)
            {
                disappearSequence.Append(hPanel.ShowRect.DOMove(hPanel.HideRect.position, _duration / 2f));
            }

            bool final = !_grantLootbox;

            disappearSequence.AppendCallback(() =>
            {
                _cupsRewardPanel.SetActive(false);

                HasJob = false;

                if(!final)
                    ShowLootboxRewardPanel();
            });

            disappearSequence.Play();

            OnButtonPressed.OnNext
                ((
                    bName: _cupsRewardPanel.ContinueButton.name,
                    isFinal: final
                ));
        }

        private void ShowLootboxRewardPanel()
        { 
        
        }

        private void HideLootboxRewardPanel() 
        {
            OnButtonPressed.OnNext
                ((
                    bName: _lootboxRewardPanel.ClaimButton.name,
                    isFinal: true
                ));
        }

        private void ScrambleScoresTotal(RaceScoresType scoresType)
        {
            int value = scoresType switch
            {
                RaceScoresType.Drift => _rewardInfo.MoneyRewardDrift,
                RaceScoresType.Bump => _rewardInfo.MoneyRewardBump,
                RaceScoresType.Crush => _rewardInfo.MoneyRewardCrush,
                RaceScoresType.Finish => _rewardInfo.MoneyRewardFinishPos,
                _ => 0,
            };

            if (value == 0) return;

            _totalScores += value;
            ScrambleText
                (
                _totalScores,
                _moneyRewardPanel.IntermediateText,
                _moneyRewardPanel.MoneyTotalText,
                _duration
                );

            ScrambleText
                (
                _totalScores * _rewardInfo.MoneyMultiplyer,
                _moneyRewardPanel.MultiplyRewardPanel.IntermediateText,
                _moneyRewardPanel.MultiplyRewardPanel.RewardTotalText,
                _duration
                );
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

        public void OnWatchAdsSuccess()
        {
            _moneyRewardPanel.MultiplyRewardPanel.ShowRect
                .DOMove(_moneyRewardPanel.MultiplyRewardPanel.HideRect.position, _duration / 3);

            ScrambleText
                (
                _totalScores * _rewardInfo.MoneyMultiplyer,
                _moneyRewardPanel.IntermediateText,
                _moneyRewardPanel.MoneyTotalText,
                _duration
                );
        }
    }
}
