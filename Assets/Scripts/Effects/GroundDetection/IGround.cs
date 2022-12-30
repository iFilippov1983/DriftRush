using UnityEngine;

namespace RaceManager.Effects
{
    public interface IGround
    { 
        public LayerMask LayerMask { get; }
        public GroundConfig Config { get; }
    }
}
