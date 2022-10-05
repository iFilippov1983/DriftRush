using UnityEngine;

namespace RaceManager.Cars.Effects
{
    public class WheelParticleHandler : MonoBehaviour
    {
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

            if (_carController.AreTiresScreeching(out float lateralVelocity, out bool isBraking))
            {
                if (isBraking)
                    _emissionRate = BrakeEmissionAmount;
                else
                    _emissionRate = Mathf.Abs(lateralVelocity) * SkidEmissionAmountFactor;
            }
        }
    }
}
