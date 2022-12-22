using UnityEngine;

namespace RaceManager.Effects
{
    [System.Serializable]
    public class GroundConfig 
    {
        [SerializeField] private EffectData _effectData;
        [SerializeField] private float _slipModificator;
        [SerializeField] private float _steerModificator;

        public EffectData EffectData => _effectData;
        public float SlipModificator => _slipModificator;
        public float SteeringModificator => _steerModificator;
    }
}
