using System.Collections.Generic;

namespace RaceManager.UI
{
    public interface IAnimatableSubject
    { 
        public string Name { get; }
        public AnimationSettings Settings { get; }
        public List<AnimationData> AnimationDataList { get; }
    }
}

