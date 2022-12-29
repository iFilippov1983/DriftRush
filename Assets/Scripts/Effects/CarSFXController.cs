using RaceManager.Cars;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Effects
{
	[RequireComponent(typeof(Car))]
	public class CarSfxController : MonoBehaviour
	{
        private const float MaxSoundsVolumeDividerValue = 40f;
        private const int MinFrictionVectorSqrMag = 25;

		[Header("Engine sounds")]
        [SerializeField] private float _pitchOffset = 0.5f;
        [SerializeField] private AudioSource _engineSource;
        [SerializeField] private AudioClip _engineIdleClip;
		[SerializeField] private AudioClip _engineBackFireClip;
        [Space]
		[Header("Wheels slip sounds")]
        [SerializeField] private float _minSlipSound = 0.15f;
        [SerializeField] private float _maxSlipForSound = 1f;
        [SerializeField] private AudioSource _slipSource;
        [SerializeField] private Dictionary<LayerMask, AudioClip> _groundSlipSounds = new Dictionary<LayerMask, AudioClip>();
        [Space]
        [Header("Wheels ground sounds")]
        [SerializeField] private List<GroundSounds> _groundSounds = new List<GroundSounds>();
        [Space]
		[Header("Collision sounds")]
        [SerializeField] private float _minTimeBetweenCollisions = 0.1f;
        [SerializeField] private float _defaultMagnitudeDivider = 20;
        [SerializeField] private AudioSource _collisionSource;
        [SerializeField] private CollisionEvent _defaultCollisionEvent;
        [SerializeField] private List<CollisionEvent> _colissionEvents = new List<CollisionEvent>();
        [Space]
        [Header("Friction sounds")]
        [SerializeField] private float _playFrictionTime = 0.5f;
        [SerializeField] private AudioSource _frictionSource;
        [SerializeField] private CollisionEvent _defaultFrictionEvent;
        [SerializeField] private List<CollisionEvent> _frictionEvents = new List<CollisionEvent>();
		[Space]
		[SerializeField] private GameObject _sfxObject;

        private Dictionary<Wheel, WheelSoundData> _wheelsSoundData = new Dictionary<Wheel, WheelSoundData>();

		private Car _car;
        private CollisionEvent _currentFrictionEvent;
        private FrictionSoundData _currentFrictionSoundData;
        private float _currentFrictionVolume;
        private float _lastColTime;

		private float MaxRPM => _car.GetMaxRPM;
		private float EngineRPM =>_car.EngineRPM; 

        [Serializable]
        private struct CollisionEvent
        {
            public LayerMask CollisionMask;
            public AudioClip CollisionClip;
            public float MinMagnitudeCollision;
            public float MaxMagnitudeCollision;
        }

        [Serializable]
        private class GroundSounds
        {
            public LayerMask Layer;
            public AudioClip IdleSound;
            public AudioClip SlipSound;
        }

        private class FrictionSoundData
        {
            public AudioSource Source;
            public float LastFrictionTime;
        }

        private class WheelSoundData
        {
            public AudioSource Source;
            public float Slip;
            public int WheelsCount;
        }

        #region Unity Functions

        private void Awake()
		{
			_car = GetComponent<Car>();

			_car.BackFireAction += PlayBackfire;
            _car.CollisionAction += PlayCollisionSound;
            _car.CollisionStayAction += PlayCollisionStaySound;
        }

        private void OnEnable()
		{
			_sfxObject.SetActive(true);
		}

		private void OnDisable()
		{
			_sfxObject.SetActive(false);
		}

		void Update()
		{
			PlaySlipSound();
            UpdateFrictions();
		}

        private void OnDestroy()
        {
            _car.BackFireAction -= PlayBackfire;
            _car.CollisionAction -= PlayCollisionSound;
            _car.CollisionStayAction -= PlayCollisionStaySound;
        }

        #endregion

        #region Private Functions

        private void PlaySlipSound()
		{
            //Engine PRM sound
            _engineSource.pitch = (EngineRPM / MaxRPM) + _pitchOffset;

            //Slip sound logic
            if (_car.CurrentMaxSlip > _minSlipSound)
            {
                if (!_slipSource.isPlaying)
                {
                    _slipSource.Play();
                }
                var slipVolumeProcent = _car.CurrentMaxSlip / _maxSlipForSound;
                _slipSource.volume = slipVolumeProcent * 0.5f;
                _slipSource.pitch = Mathf.Clamp(slipVolumeProcent, 0.75f, 1);
            }
            else
            {
                _slipSource.Stop();
            }
        }

		private void PlayBackfire()
		{
            if (!isActiveAndEnabled)
                return;

            _engineSource.PlayOneShot(_engineBackFireClip);
		}

        private void PlayCollisionSound(Car car, Collision collision)
        {
            $"Col enter: {collision.gameObject.name}".Log();

            if (!car.IsVisible || collision == null || !isActiveAndEnabled)
                return;

            int collisionLayer = collision.gameObject.layer;

            if (Time.time - _lastColTime < _minTimeBetweenCollisions)
            {
                return;
            }

            float collisionMagnitude = collision.rigidbody == null
                ? collision.relativeVelocity.magnitude
                : (_car.RB.velocity - collision.rigidbody.velocity).magnitude;

            CollisionEvent colEvent = GetEventForCollision(collisionLayer, collisionMagnitude, out float magnitudeDivider);
            float volume = Mathf.Clamp01(collisionMagnitude / magnitudeDivider.Clamp(0, MaxSoundsVolumeDividerValue));

            PlayOneShot(colEvent, volume, collision.contacts[0].point);
        }

        /// <summary>
        /// Find the desired collision event based on the collision magnitude and the collision layer.
        /// </summary>
        /// <param name="layer">Collision layer</param>
        /// <param name="collisionMagnitude">Collision magnitude.</param>
        /// <param name="magnitudeDivider">Divider to calculate collision volume.</param>
        /// <returns></returns>
        private CollisionEvent GetEventForCollision(int layer, float collisionMagnitude, out float magnitudeDivider)
        {
            foreach (var colEvent in _colissionEvents)
            {
                if (colEvent.CollisionMask.LayerInMask(layer) 
                    && collisionMagnitude >= colEvent.MinMagnitudeCollision 
                    && collisionMagnitude < colEvent.MaxMagnitudeCollision)
                {
                    magnitudeDivider = colEvent.MaxMagnitudeCollision == float.PositiveInfinity
                        ? _defaultMagnitudeDivider
                        : colEvent.MaxMagnitudeCollision;

                    return colEvent;
                }
            }

            magnitudeDivider = _defaultMagnitudeDivider;
            return _defaultCollisionEvent;
        }

        private void PlayOneShot(CollisionEvent colEvent, float volume, Vector3 point)
        {
            _collisionSource.volume = volume;
            _collisionSource.transform.position = point;
            _collisionSource.PlayOneShot(colEvent.CollisionClip);
        }

        private void PlayCollisionStaySound(Car car, Collision collision)
        {
            $"Col stay: {collision.gameObject.name}".Log();

            if (!isActiveAndEnabled)
                return;

            if (_car.CurrentSpeed >= 1 
                && (collision.rigidbody == null || (collision.rigidbody.velocity - _car.RB.velocity).sqrMagnitude > MinFrictionVectorSqrMag))
            {
                PlayFrictionSound(collision, collision.relativeVelocity.magnitude);
            }
        }

        private void PlayFrictionSound(Collision collision, float magnitude)
        {
            if (_car.CurrentSpeed >= 1)
            {
                _currentFrictionEvent = GetEventForFriction(collision.collider.gameObject.layer, magnitude);

                float collisionMagnitude = collision.rigidbody == null
                    ? collision.relativeVelocity.magnitude
                    : (_car.RB.velocity - collision.rigidbody.velocity).magnitude ;

                float magnitudeDivider = _currentFrictionEvent.MaxMagnitudeCollision == float.PositiveInfinity
                    ? _defaultMagnitudeDivider
                    : _currentFrictionEvent.MaxMagnitudeCollision;

                _currentFrictionVolume = Mathf.Clamp01(collisionMagnitude / magnitudeDivider.Clamp(0, MaxSoundsVolumeDividerValue));

                _currentFrictionSoundData ??= new FrictionSoundData() { Source = _frictionSource, LastFrictionTime = Time.time };

                if (!_currentFrictionSoundData.Source.isPlaying 
                    || _currentFrictionEvent.CollisionClip != _currentFrictionSoundData.Source.clip)
                {
                    _currentFrictionSoundData.Source.Stop();
                    _currentFrictionSoundData.Source.volume = _currentFrictionVolume;
                    _currentFrictionSoundData.Source.clip = _currentFrictionEvent.CollisionClip;
                    _currentFrictionSoundData.Source.Play();
                }
            }
        }

        private CollisionEvent GetEventForFriction(int layer, float collisionMagnitude)
        {
            foreach (var fEvent in _frictionEvents)
            {
                if (fEvent.CollisionMask.LayerInMask(layer)
                    && collisionMagnitude >= fEvent.MinMagnitudeCollision
                    && collisionMagnitude < fEvent.MaxMagnitudeCollision)
                {
                    return fEvent;
                }
            }

            return _defaultFrictionEvent;
        }

        private void UpdateFrictions()
        {
            if (!_frictionSource.isPlaying)
                return;

            float time = Time.time - _currentFrictionSoundData.LastFrictionTime;
            if (time > _playFrictionTime)
            {
                _frictionSource.Stop();
                _currentFrictionSoundData = null;
            }
        }

        #endregion
    }
}
