using System.Collections.Generic;

namespace RaceManager.UI
{
    public interface IAnimatableSubject
    { 
        public string Name { get; }
        public List<AnimationData> AnimationDataList { get; }
    }
}

