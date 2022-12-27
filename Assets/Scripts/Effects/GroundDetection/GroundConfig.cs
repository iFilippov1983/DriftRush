using UnityEngine;

namespace RaceManager.Effects
{
    [System.Serializable]
    public class GroundConfig 
    {
        [SerializeField] private LayerMask _itsLayer;
        [SerializeField] private ParticleSystem _idleParticles;
        [SerializeField] private ParticleSystem _slipParticles;
        [Tooltip("Dependence of particle system operation on the speed of the car.")]
        [SerializeField] private bool _speedDependent;

        [Tooltip("Wheel Stiffness Multiplier.")]
        [Range(0f, 1f)]
        public float WheelStiffnessMultiplier = 1f;

        public LayerMask LayerMask => _itsLayer;
        public ParticleSystem IdleParticles => _idleParticles;
        public ParticleSystem SlipParticles => _slipParticles;
        public bool SpeedDependent => _speedDependent;
    }
}
