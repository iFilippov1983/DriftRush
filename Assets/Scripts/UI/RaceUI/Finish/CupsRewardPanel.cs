using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class CupsRewardPanel : MonoBehaviour, IAnimatablePanel
    {
        [SerializeField] private TMP_Text _cupsTotalText;
        [SerializeField] private TMP_Text _cupsRewardText;
        [SerializeField] private Button _continueButton;
        [SerializeField] private RectTransform _titlePosition;

        [SerializeField] private List<AnimatablePanelView> _animatablePanels = new List<AnimatablePanelView>();

        public TMP_Text CupsTotalText => _cupsTotalText;
        public TMP_Text CupsRewardText => _cupsRewardText;
        public Button ContinueButton => _continueButton;
        public RectTransform TitlePosition => _titlePosition;
        public List<AnimatablePanelView> AnimatablePanels => _animatablePanels;

        public void Accept(IAnimatablePanelsHandler handler)
        {
            handler.Handle(this);
        }
    }
}

