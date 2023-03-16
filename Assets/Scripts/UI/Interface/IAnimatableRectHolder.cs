using UnityEngine;

namespace RaceManager.UI
{
    public interface IAnimatableRectHolder
    { 
        public RectTransform ShowRect { get; }
        public RectTransform HideRect { get; }
    }
}

