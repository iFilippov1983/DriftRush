using UnityEngine;

namespace RaceManager.Effects
{
    [System.Serializable]
    public class GroundConfig 
    {
        [SerializeField] private GroundType _type;
        [SerializeField] private ParticleSystem _idleParticles;
        [SerializeField] private ParticleSystem _slipParticles;
        [Tooltip("Dependence of particle system operation on the speed of the car.")]
        [SerializeField] private bool _speedDependent;

        [Tooltip("Wheel friction multiplier.")]
        public float WheelStiffness;

        public GroundType Type => _type;
        public ParticleSystem IdleParticles => _idleParticles;
        public ParticleSystem SlipParticles => _slipParticles;
        public bool SpeedDependent => _speedDependent;
    }
}
