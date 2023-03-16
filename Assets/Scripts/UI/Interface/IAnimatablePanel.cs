using System.Collections.Generic;

namespace RaceManager.UI
{
    public interface IAnimatablePanel
    {
        public List<AnimatablePanelView> ShowPanels { get; }
        public List<AnimatablePanelView> HidePanels { get; }
        public void Accept(IAnimatablePanelsHandler handler);
    }
}

