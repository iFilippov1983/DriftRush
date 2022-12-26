﻿using RaceManager.Cars;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace RaceManager.Effects
{
    public class CarVfxController : MonoBehaviour
	{
        private const float TrailOffset = 0.05f;

		[SerializeField] private float _minTimeBetweenCollisions = 0.1f;
		[SerializeField] private TrailRenderer _trailPrefab;
		[SerializeField] private ParticleSystem _defaultColisionParticle;
		[SerializeField] private List<CollisionParticles> _collisionParticles = new List<CollisionParticles>();
		[SerializeField] private List<ParticleSystem> _backFireParticles = new List<ParticleSystem>();

        private Car _car;
        private Transform _trailParent;
        private Queue<TrailRenderer> _freeTrails = new Queue<TrailRenderer>();

        private float _lastCollisionTime;

        private Dictionary<Wheel, TrailRenderer> ActiveTrails { get; set; }

        [System.Serializable]
        private struct CollisionParticles
        {
            public ParticleSystem Particles;
            public LayerMask CollisionLayer;
            public float MinMagnitudeCollision;
            public float MaxMagnitudeCollision;
        }

        #region Unity Functions

        protected virtual void Awake()
        {
            _trailPrefab.gameObject.SetActive(false);
            _car = GetComponentInParent<Car>();

            if (_car == null)
            {
                Debug.LogErrorFormat("[{0}] VehicleVFX without VehicleController in parent", name);
                enabled = false;
                return;
            }

            _car.ResetVehicleAction += ResetAllTrails;
            _car.CollisionAction += PlayCollisionParticles;
            _car.CollisionStayAction += CollisionStay;
            _car.BackFireAction += OnBackFire;

            _trailParent = new GameObject(string.Format("Effects for {0}", _car.name)).transform;

            ActiveTrails = new Dictionary<Wheel, TrailRenderer>();
            foreach (var wheel in _car.Wheels)
            {
                ActiveTrails.Add(wheel, null);
            }
        }

        private void Update()
        {
            HandleWheels();
        }

        private void OnDestroy()
        {
            _car.ResetVehicleAction -= ResetAllTrails;
            _car.CollisionAction -= PlayCollisionParticles;
            _car.CollisionStayAction -= CollisionStay;
        }

        #endregion

        private void HandleWheels()
        {
            EmitParams emitParams;
            float rndValue = Random.Range(0, 1f);
            for (int i = 0; i < _car.Wheels.Length; i++)
            {
                var wheel = _car.Wheels[i];
                var groundConfig = wheel.CurrentGroundConfig;
                var hasSlip = wheel.HasForwardSlip || wheel.HasSideSlip;

                //Emit particle.
                if (_car.IsVisible && groundConfig != null)
                {
                    var particles = hasSlip ? groundConfig.SlipParticles : groundConfig.IdleParticles;
                    if (particles)
                    {
                        float sizeAndLifeTimeMultiplier = (groundConfig.SpeedDependent
                            ? (Mathf.Max(_car.CurrentSpeed, (wheel.Radius * Mathf.PI * wheel.RPM / 30)) / 30).Clamp()
                            : 1)
                            * rndValue;

                        var point = wheel.transform.position;
                        point.y = wheel.GetHit.point.y;

                        var particleVelocity = -wheel.GetHit.forwardDir * wheel.GetHit.forwardSlip;
                        particleVelocity += wheel.GetHit.sidewaysDir * wheel.GetHit.sidewaysSlip;
                        particleVelocity += _car.RB.velocity;

                        emitParams = new EmitParams();

                        emitParams.position = point;
                        emitParams.velocity = particleVelocity;
                        emitParams.startSize = Mathf.Max(1f, particles.main.startSize.constant * sizeAndLifeTimeMultiplier);
                        emitParams.startLifetime = particles.main.startLifetime.constant * sizeAndLifeTimeMultiplier;
                        emitParams.startColor = particles.main.startColor.color;

                        particles.Emit(emitParams, 1);
                    }
                }

                //Emit trail
                UpdateTrail(wheel, wheel.IsGrounded && hasSlip);
            }
        }

        private void OnBackFire()
        {
            foreach (var particles in _backFireParticles)
            {
                particles.Emit(1);
            }
        }

        #region Trail Functions

        public void UpdateTrail(Wheel wheel, bool hasSlip)
        {
            var trail = ActiveTrails[wheel];

            if (hasSlip)
            {
                if (trail == null)
                {
                    //Get free or create trail.
                    trail = GetTrail(wheel.WheelView.position + (wheel.transform.up * (-wheel.Radius + TrailOffset)));
                    trail.transform.SetParent(wheel.transform);
                    ActiveTrails[wheel] = trail;
                }
                else
                {
                    //Move the trail to the desired position
                    trail.transform.position = wheel.WheelView.position + (wheel.transform.up * (-wheel.Radius + TrailOffset));
                }
            }
            else if (ActiveTrails[wheel] != null)
            {
                //Set trail as free.
                SetTrailAsFree(trail);
                trail = null;
                ActiveTrails[wheel] = trail;
            }
        }

        private void ResetAllTrails()
        {
            TrailRenderer trail;
            for (int i = 0; i < _car.Wheels.Length; i++)
            {
                trail = ActiveTrails[_car.Wheels[i]];
                if (trail)
                {
                    SetTrailAsFree(trail);
                    trail = null;
                    ActiveTrails[_car.Wheels[i]] = trail;
                }
            }
        }

        /// <summary>
        /// Get first free trail and set start position.
        /// </summary>
        public TrailRenderer GetTrail(Vector3 startPos)
        {
            TrailRenderer trail;
            if (_freeTrails.Count > 0)
            {
                trail = _freeTrails.Dequeue();
            }
            else
            {
                trail = Instantiate(_trailPrefab, _trailParent);
            }

            trail.transform.position = startPos;
            trail.gameObject.SetActive(true);
            trail.Clear();

            return trail;
        }

        /// <summary>
        /// Set trail as free and wait while life time.
        /// </summary>
        private void SetTrailAsFree(TrailRenderer trail)
        {
            StartCoroutine(WaitVisibleTrail(trail));
        }

        /// <summary>
        /// The trail is considered busy until it disappeared.
        /// </summary>
        private IEnumerator WaitVisibleTrail(TrailRenderer trail)
        {
            trail.transform.SetParent(_trailParent);
            yield return new WaitForSeconds(trail.time);
            trail.Clear();
            trail.gameObject.SetActive(false);
            _freeTrails.Enqueue(trail);
        }

        #endregion

        #region Collision Functions

        private void CollisionStay(Car car, Collision collision)
        {
            if (_car.CurrentSpeed >= 1 && (collision.rigidbody == null || (collision.rigidbody.velocity - car.RB.velocity).sqrMagnitude > 25))
            {
                PlayCollisionParticles(car, collision);
            }
        }

        private void PlayCollisionParticles(Car car, Collision collision)
        {
            if (!car.IsVisible || collision == null || Time.time - _lastCollisionTime < _minTimeBetweenCollisions)
            {
                return;
            }

            _lastCollisionTime = Time.time;
            var magnitude = collision.relativeVelocity.magnitude * Vector3.Dot(collision.relativeVelocity.normalized, collision.contacts[0].normal).Abs();
            var particles = GetParticlesForCollision(collision.gameObject.layer, magnitude);

            for (int i = 0; i < collision.contacts.Length; i++)
            {
                particles.transform.position = collision.contacts[i].point;
                particles.Play(withChildren: true);
            }
        }

        private ParticleSystem GetParticlesForCollision(int layer, float collisionMagnitude)
        {
            for (int i = 0; i < _collisionParticles.Count; i++)
            {
                if (_collisionParticles[i].CollisionLayer.LayerInMask(layer)
                    && collisionMagnitude >= _collisionParticles[i].MinMagnitudeCollision
                    && collisionMagnitude < _collisionParticles[i].MaxMagnitudeCollision)
                {
                    return _collisionParticles[i].Particles;
                }
            }

            return _defaultColisionParticle;
        }

        #endregion
    }
}
