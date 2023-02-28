using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class MoneyRewardPanel : MonoBehaviour, IAnimatablePanel
    {
        [SerializeField] private TMP_Text _moneyTotalText;
        [SerializeField] private RectTransform _titlePosition;
        [SerializeField] private Button _continueButton;
        [SerializeField] private MultiplyRewardAnimPanel _multiplyRewardPanel;

        [SerializeField] private List<AnimatablePanelView> _animatablePanels = new List<AnimatablePanelView>();
        [SerializeField] private List<RewardAnimatablePanel> _rewardPanels = new List<RewardAnimatablePanel>();

        public TMP_Text MoneyTotalText => _moneyTotalText;
        public RectTransform TitlePosition => _titlePosition;
        public Button ContinueButton => _continueButton;
        public MultiplyRewardAnimPanel MultiplyRewardPanel => _multiplyRewardPanel;
        public List<AnimatablePanelView> AnimatablePanels => _animatablePanels;
        public List<RewardAnimatablePanel> RewardPanels => _rewardPanels;

        public void Accept(IAnimatablePanelsHandler handler)
        {
            handler.Handle(this);
        }
    }
}

