using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace RaceManager.Cars.Effects
{
    public class CarSfxHandler : MonoBehaviour
    {
        private const float VelocityMagnitudeFactor = 0.05f;
        private const float MinVolume = 0.2f;
        private const float MaxVolume = 1f;
        private const float VolumeLerpFactor = 10f;
        private const float PitchFactor = 0.2f;
        private const float MinPitch = 0.5f;
        private const float MaxPitch = 1f;
        private const float PitchLerpFactor = 1.5f;
        private const float PitchScreechingLerpFactor = 10f;
        private const float PitchScreechingAbsFactor = 0.1f;
        private const float HitVolumeFactor = 0.1f;
        private const float HitMinPitch = 0.95f;
        private const float HitMaxPitch = 1.05f;

        private const float AudioMixerStartVolume = 0.5f;

        public AudioMixer audioMixer;
        public AudioSource tiresScreechingAS;
        public AudioSource engineAS;
        public AudioSource carHitAS;

        private CarController _carController;
        private float _desiredEnginePitch = 0.5f;
        private float _tireScreechPitch = 0.5f;

        private void Awake()
        {
            _carController = GetComponentInParent<CarController>();
        }

        private void Start()
        {
            audioMixer.SetFloat("SFXVolume", AudioMixerStartVolume);
        }

        // Update is called once per frame
        void Update()
        {
            UpdateEngineSFX();
            UpdateTiresScreechingSFX();
        }

        private void UpdateEngineSFX()
        {
            float velocityMagnitude = _carController.GetVelocityMagnitude();
            float desiredEngineVolume = velocityMagnitude * VelocityMagnitudeFactor;
            desiredEngineVolume = Mathf.Clamp(desiredEngineVolume, MinVolume, MaxVolume);
            engineAS.volume = Mathf.Lerp(engineAS.volume, desiredEngineVolume, Time.deltaTime * VolumeLerpFactor);
            _desiredEnginePitch = velocityMagnitude * PitchFactor;
            _desiredEnginePitch = Mathf.Clamp(_desiredEnginePitch, MinPitch, MaxPitch);
            engineAS.pitch = Mathf.Lerp(engineAS.pitch, _desiredEnginePitch, Time.deltaTime * PitchLerpFactor);
        }

        private void UpdateTiresScreechingSFX()
        {
            if (_carController.AreTiresScreeching(out float lateralVelocity, out bool isBraking))
            {
                if (isBraking)
                {
                    tiresScreechingAS.volume = Mathf.Lerp(tiresScreechingAS.volume, MaxVolume, Time.deltaTime * VolumeLerpFactor);
                    _tireScreechPitch = Mathf.Lerp(_tireScreechPitch, MinPitch, Time.deltaTime * PitchScreechingLerpFactor);
                }
                else
                {
                    tiresScreechingAS.volume = Mathf.Abs(lateralVelocity) * Time.deltaTime;
                    _tireScreechPitch = Mathf.Abs(lateralVelocity) * PitchScreechingAbsFactor;
                }
            }
            else
                tiresScreechingAS.volume = Mathf.Lerp(tiresScreechingAS.volume, 0f, Time.deltaTime * PitchScreechingLerpFactor);
        }

        private void OnCollisionEnter(Collision collision)
        {
            float relativeVelocity = collision.relativeVelocity.magnitude;
            float volume = relativeVelocity * HitVolumeFactor;

            carHitAS.pitch = Random.Range(HitMinPitch, HitMaxPitch);
            carHitAS.volume = volume;

            if (!carHitAS.isPlaying)
                carHitAS.Play();
        }
    }
}
