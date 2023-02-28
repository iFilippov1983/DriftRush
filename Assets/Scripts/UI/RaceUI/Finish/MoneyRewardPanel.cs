using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class MoneyRewardPanel : MonoBehaviour, IAnimatablePanel
    {
        [SerializeField] private TMP_Text _moneyTotalText;
        [SerializeField] private Text _intermediateScoresText;
        [SerializeField] private RectTransform _titlePosition;
        [SerializeField] private Button _continueButton;
        [SerializeField] private MultiplyRewardAnimPanel _multiplyRewardPanel;

        [SerializeField] private List<AnimatablePanelView> _showPanels = new List<AnimatablePanelView>();
        [SerializeField] private List<AnimatablePanelView> _hidePanles = new List<AnimatablePanelView>();
        [SerializeField] private List<RewardAnimatablePanel> _rewardPanels = new List<RewardAnimatablePanel>();

        public TMP_Text MoneyTotalText => _moneyTotalText;
        public Text IntermediateText => _intermediateScoresText;
        public RectTransform TitlePosition => _titlePosition;
        public Button ContinueButton => _continueButton;
        public MultiplyRewardAnimPanel MultiplyRewardPanel => _multiplyRewardPanel;
        public List<AnimatablePanelView> ShowPanels => _showPanels;
        public List<AnimatablePanelView> HidePanels => _hidePanles;
        public List<RewardAnimatablePanel> RewardPanels => _rewardPanels;

        public void Accept(IAnimatablePanelsHandler handler)
        {
            handler.Handle(this);
        }
    }
}

