using UnityEngine;

namespace RaceManager.Effects
{
    [System.Serializable]
    public class GroundConfig 
    {
        [Tooltip("Dependence of particle system operation on the speed of the car.")]
        [SerializeField] private bool _speedDependent = true;
        [Tooltip("Wheel Stiffness Multiplier.")]
        [Range(0f, 1f)]
        public float WheelStiffnessMultiplier = 1f;
        [Space]
        [SerializeField] private ParticleSystem _idleParticles;
        [SerializeField] private ParticleSystem _slipParticles;

        private LayerMask _layerMask;

        public bool SpeedDependent => _speedDependent;
        public ParticleSystem IdleParticles => _idleParticles;
        public ParticleSystem SlipParticles => _slipParticles;
        public LayerMask LayerMask => _layerMask;

        public void SetGroundLayer(IGround ground)
        {
            _layerMask.value = ground.LayerMask;
            $"Layer is set => [{_layerMask.value}]".Log(Logger.ColorBlue);
        }
    }
}
