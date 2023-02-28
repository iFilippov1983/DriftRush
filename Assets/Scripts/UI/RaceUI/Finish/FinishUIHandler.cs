using RaceManager.Race;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.CustomPlugins;
using DG.Tweening.Plugins;
using TMPro;
using UniRx;
using UnityEngine;

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

        public FinishUIHandler(RectTransform titleRect, TMP_Text titleText, TMP_Text posText, float animationDuration = 1f)
        {
            _titleRect = titleRect;
            _titleText = titleText;
            _posText = posText;
            _duration = animationDuration;
        }

        public Subject<(string bName, bool isFinal)> OnButtonPressed = new Subject<(string bName, bool isFinal)>();

        public void Handle(MoneyRewardPanel moneyRewardPanel)
        {
            _moneyRewardPanel = moneyRewardPanel;
            _moneyRewardPanel.ContinueButton.onClick.AddListener(HideMoneyRewardPanel);
        }

        public void Handle(CupsRewardPanel cupsRewardPanel)
        {
            _cupsRewardPanel = cupsRewardPanel;
            _cupsRewardPanel.ContinueButton.onClick.AddListener(HideCupsRewardPanel);
        }

        public void Handle(LootboxRewardPanel lootboxRewardPanel)
        {
            _lootboxRewardPanel = lootboxRewardPanel;
            _lootboxRewardPanel.ClaimButton.onClick.AddListener(HideLootboxRewardPanel);
        }

        public void SetLootboxRewardPanel(bool grantLootbox, Sprite lootboxSprite)
        { 
            _grantLootbox = grantLootbox;
            _lootboxRewardPanel.LootboxImage.sprite = lootboxSprite;
        }

        public void ShowMoneyRewardPanel(RaceRewardInfo info)
        {
            HasJob = true;
            _rewardInfo = info;

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

            foreach (var aPanel in _moneyRewardPanel.AnimatablePanels)
            {
                appearSequence.Append(aPanel.ShowRect.DOMove(aPanel.HideRect.position, _duration / 2).From());
            }

            float d = appearSequence.Duration() / 2;

            foreach (var rPanel in _moneyRewardPanel.RewardPanels)
            {
                appearSequence.Insert(d,
                    rPanel.ShowRect.DOMove(rPanel.HideRect.position, _duration)
                    .From()
                    .OnComplete(() => ScrambleScoresTotal(rPanel.ScoreType)));

                d += _duration;
            }
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

            _totalScores += value;

            
        }

        private void HideMoneyRewardPanel()
        {
            OnButtonPressed.OnNext
                ((
                    bName: _moneyRewardPanel.ContinueButton.name, 
                    isFinal: false
                ));
        }

        private void HideCupsRewardPanel()
        {
            bool final = !_grantLootbox;
            OnButtonPressed.OnNext
                ((
                    bName: _cupsRewardPanel.ContinueButton.name,
                    isFinal: final
                ));
        }

        private void HideLootboxRewardPanel() 
        {
            OnButtonPressed.OnNext
                ((
                    bName: _lootboxRewardPanel.ClaimButton.name,
                    isFinal: true
                ));
        }
    }
}
