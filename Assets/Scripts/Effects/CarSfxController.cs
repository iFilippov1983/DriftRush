using RaceManager.Cars;
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
        private const int MinWheelsAudioSourcesAmount = 2;

		[Header("Engine sounds")]
        [SerializeField] private float _pitchOffset = 0.5f;
        [SerializeField] private AudioSource _engineSource;
        [SerializeField] private AudioClip _engineIdleClip;
		[SerializeField] private AudioClip _engineBackFireClip;
        [Space]
		[Header("Wheels sounds")]
        //[SerializeField] private float _minSlipSound = 0.15f;
        [SerializeField] private float _maxSlipForSound = 1f;
        //[SerializeField] private AudioSource _slipSource;
        [SerializeField] private AudioSource[] _audioSourcesForWheels;
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

        private Dictionary<Wheel, AudioSource> _wheelsAudioSources = new Dictionary<Wheel, AudioSource>();

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

        #region Unity Functions

        private void Awake()
		{
			_car = GetComponent<Car>();

			_car.BackFireAction += PlayBackfire;
            _car.CollisionAction += PlayCollisionSound;
            _car.CollisionStayAction += PlayCollisionStaySound;
            _car.CollisionExitAction += StopFrictionSound;
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
            HandleEngineSound();
			HandleWheelsSound();
            UpdateFrictions();
		}

        private void OnDestroy()
        {
            _car.BackFireAction -= PlayBackfire;
            _car.CollisionAction -= PlayCollisionSound;
            _car.CollisionStayAction -= PlayCollisionStaySound;
            _car.CollisionExitAction -= StopFrictionSound;
        }

        #endregion

        #region Public Functions

        public void Initialize()
        {
            Wheel[] wheelsArray = _audioSourcesForWheels.Length <= MinWheelsAudioSourcesAmount
                ? _car.RearAxis
                : _car.Wheels;

            for (int i = 0; i < _audioSourcesForWheels.Length; i++)
            {
                Wheel wheel = wheelsArray[i];
                AudioSource source = _audioSourcesForWheels[i];

                if (wheel != null && source != null)
                    _wheelsAudioSources.Add(wheel, source);
            }
        }

        #endregion

        #region Private Functions
        private void HandleWheelsSound()
		{
            if (!_car.IsVisible)
                return;

            foreach (var kvp in _wheelsAudioSources)
            {
                Wheel wheel = kvp.Key;
                AudioSource source = kvp.Value;
                bool hasSlip = wheel.HasForwardSlip || wheel.HasSideSlip;

                LayerMask layer = wheel.CurrentGroundConfig.LayerMask;
                GroundSounds groundSounds = _groundSounds.Find(g => g.Layer.LayerInMask(layer));

                if (groundSounds == null)
                {
                    $"GroundSounds doesn't contain sounds for layer: {LayerMask.LayerToName(layer)}".Log(Logger.ColorYellow);
                    continue;
                }

                AudioClip clip;
                if (hasSlip && groundSounds.SlipSound != null)
                {
                    clip = groundSounds.SlipSound;
                    if (!source.isPlaying || source.clip != clip)
                    {
                        source.clip = clip;
                        source.Play();
                    }

                }
                else if (groundSounds.IdleSound != null)
                {
                    clip = groundSounds.IdleSound;
                    if (!source.isPlaying || source.clip != clip)
                    {
                        source.clip = groundSounds.IdleSound;
                        source.Play();
                    }
                }
                else
                {
                    source.Stop();
                }

                var slipVolumePercent = _car.CurrentMaxSlip / _maxSlipForSound;
                source.volume = slipVolumePercent * 0.5f;
                source.pitch = Mathf.Clamp(slipVolumePercent, 0.75f, 1);
            }
        }

        private void HandleEngineSound()
        {
            _engineSource.pitch = (EngineRPM / MaxRPM) + _pitchOffset;
        }

		private void PlayBackfire()
		{
            if (!isActiveAndEnabled)
                return;

            _engineSource.PlayOneShot(_engineBackFireClip);
		}

        private void PlayCollisionSound(Car car, Collision collision)
        {
            //$"Col ENTER => {collision.gameObject.name}".Log();

            //List<ContactPoint> contactPoints = new List<ContactPoint>();
            //collision.GetContacts(contactPoints);
            //foreach (ContactPoint cp in contactPoints)
            //{
            //    Debug.Log($"this col: {cp.thisCollider.gameObject.name}");
            //}

            if (!car.IsVisible
                || collision == null
                || !isActiveAndEnabled
                //|| collision.transform.parent.gameObject.GetInstanceID() == car.gameObject.GetInstanceID()
                || Time.time - _lastColTime < _minTimeBetweenCollisions)
                return;

            //if (!car.IsVisible) return;
            //if (collision == null) return;
            //if (!isActiveAndEnabled) return;
            //if (collision.transform.parent.gameObject.GetInstanceID() == car.gameObject.GetInstanceID()) return;
            //if (Time.time - _lastColTime < _minTimeBetweenCollisions) return;

            int collisionLayer = collision.gameObject.layer;

            float collisionMagnitude = collision.rigidbody == null
                ? 0 //collision.relativeVelocity.magnitude
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
            //$"Col STAY: {collision.gameObject.name}".Log();

            if (!isActiveAndEnabled)
                return;

            if (_car.CurrentSpeed >= 1 
                && (collision.rigidbody == null || (collision.rigidbody.velocity - _car.RB.velocity).sqrMagnitude > MinFrictionVectorSqrMag))
            {
                PlayFrictionSound(collision, collision.relativeVelocity.magnitude);
            }
        }

        private void StopFrictionSound(Car car, Collision collision)
        {
            //$"Col EXIT => {collision.gameObject.name}".Log();

            if (_currentFrictionSoundData != null)
            {
                _currentFrictionSoundData.Source.Stop();
            } 
        }

        private void PlayFrictionSound(Collision collision, float magnitude)
        {
            if (_car.CurrentSpeed >= 1)
            {
                _currentFrictionEvent = GetEventForFriction(collision.collider.gameObject.layer, magnitude);

                float collisionMagnitude = collision.rigidbody == null
                    ? 0 //collision.relativeVelocity.magnitude
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
