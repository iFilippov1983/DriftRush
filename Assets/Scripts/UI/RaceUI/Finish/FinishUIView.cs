using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace RaceManager.UI
{
    public class FinishUIView : MonoBehaviour
    {
        [Title("Finish Title")]
        [SerializeField] private RectTransform _titleRect;
        [SerializeField] private TMP_Text _finishTitleText;
        [SerializeField] private TMP_Text _positionText;

        [Title("Reward Panels")]
        [SerializeField] private MoneyRewardPanel _moneyRewardPanel;
        [SerializeField] private CupsRewardPanel _cupsRewardPanel;
        [SerializeField] private LootboxRewardPanel _lootboxRewardPanel;

        private List<IAnimatablePanel> _animatablePanels;

        public RectTransform TitleRect => _titleRect;
        public TMP_Text FinishTitleText => _finishTitleText;
        public TMP_Text PositionText => _positionText;

        public List<IAnimatablePanel> AnimatablePanels
        {
            get 
            {
                if (_animatablePanels is null)
                {
                    _animatablePanels = new List<IAnimatablePanel>
                    {
                        _moneyRewardPanel,
                        _cupsRewardPanel,
                        _lootboxRewardPanel
                    };
                }
                return _animatablePanels;
            }
        }
    }
}
