using System.Collections.Generic;

namespace RaceManager.UI
{
    public interface IAnimatablePanel
    {
        public List<AnimatablePanelView> AnimatablePanels { get; }
        public void Accept(IAnimatablePanelsHandler handler);
    }
}

