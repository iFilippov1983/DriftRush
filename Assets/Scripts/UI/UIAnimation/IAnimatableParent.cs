using System;

namespace RaceManager.UI
{
    public interface IAnimatableParent
    {
        public event Action OnAnimationInitialize;
        public event Action OnAnimationFinish;

        public void AnimationFinishCallback();
    }
}
