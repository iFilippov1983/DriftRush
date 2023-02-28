using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class LootboxRewardPanel : MonoBehaviour, IAnimatablePanel
    {
        [SerializeField] private Image _looboxImage;
        [SerializeField] private Image _effectImage;
        [SerializeField] private Button _claimButton;
        [SerializeField] private RectTransform _titlePosition;

        [SerializeField] private List<AnimatablePanelView> _animatablePanels = new List<AnimatablePanelView>();

        public Image LootboxImage => _looboxImage;
        public Image EffectImage => _effectImage;
        public Button ClaimButton => _claimButton;
        public RectTransform TitlePosition => _titlePosition;
        public List<AnimatablePanelView> AnimatablePanels => _animatablePanels;

        public void Accept(IAnimatablePanelsHandler handler)
        {
            handler.Handle(this);
        }
    }
}

