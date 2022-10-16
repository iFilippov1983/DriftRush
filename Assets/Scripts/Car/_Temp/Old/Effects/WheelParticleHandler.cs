using UnityEngine;

namespace RaceManager.Alt
{
    public class WheelParticleHandler : MonoBehaviour
    {
        [SerializeField] private WheelCollider _attachedWheelCollider;
        private const float EmissionLerpSpeed = 5f;
        private const float BrakeEmissionAmount = 300f;
        private const float SkidEmissionAmountFactor = 5f;

        private float _emissionRate = 0;

        private CarController _carController;
        private ParticleSystem _particleSystem;
        private ParticleSystem.EmissionModule _emissionModule;

        private void Awake()
        {
            _carController = GetComponentInParent<CarController>();
            _particleSystem = GetComponent<ParticleSystem>();
            _emissionModule = _particleSystem.emission;

            _emissionModule.rateOverTime = 0;
        }

        private void Update()
        {
            _emissionRate = Mathf.Lerp(_emissionRate, 0, Time.deltaTime * EmissionLerpSpeed);
            _emissionModule.rateOverTime = _emissionRate;

            WheelHit wheelhit;
            _attachedWheelCollider.GetGroundHit(out wheelhit);

            if (_carController.AreTiresScreeching(out float lateralVelocity, out bool isBraking) && wheelhit.normal != Vector3.zero)
            {
                if (isBraking)
                    _emissionRate = BrakeEmissionAmount;
                else
                    _emissionRate = Mathf.Abs(lateralVelocity) * SkidEmissionAmountFactor;
            }
        }
    }
}
