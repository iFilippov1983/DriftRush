using RaceManager.Cars;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx.Triggers;
using static UnityEngine.ParticleSystem;
using UnityEditor;

namespace RaceManager.Effects
{
    public class CarVfxController : MonoBehaviour
	{
        private const float TrailOffset = 0.05f;
        private const int SpeedDivider = 30;
        private const float SpeedThresholdDif = 1.5f;

		[SerializeField] private float _minTimeBetweenCollisions = 0.1f;
		[SerializeField] private TrailRenderer _trailPrefab;
		[SerializeField] private ParticleSystem _defaultColisionParticle;
        [Space]
        [SerializeField] private MeshRenderer[] _stopLights;
        [Space]
		[SerializeField] private List<CollisionParticles> _collisionParticles = new List<CollisionParticles>();
		[SerializeField] private List<ParticleSystem> _backFireParticles = new List<ParticleSystem>();

        private Car _car;
        private Transform _vfxParent;
        private Queue<TrailRenderer> _freeTrails = new Queue<TrailRenderer>();

        private float _lastCollisionTime;
        private float _lastSpeed;

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
            
            if (!TryGetComponent<Car>(out _car))
            {
                Debug.LogErrorFormat("[{0}] CarVfxController whithout Car component in parent", name);
                enabled = false;
                return;
            }

            _car.ResetVehicleAction += ResetAllTrails;
            _car.CollisionAction += PlayCollisionParticles;
            _car.CollisionStayAction += CollisionStay;
            _car.BackFireAction += OnBackFire;

            _vfxParent = new GameObject(string.Format("Effects for {0}", _car.name)).transform;

            ActiveTrails = new Dictionary<Wheel, TrailRenderer>();
            foreach (var wheel in _car.Wheels)
            {
                ActiveTrails.Add(wheel, null);

                _debugInfo.Add(wheel, new VfxDebugInfo() 
                { 
                   HasSlip = false,
                   ParticleName = string.Empty
                });
            }
        }

        private void Update()
        {
            HandleWheels();
            HandleStopLights();
        }

        private void OnDestroy()
        {
            if (_car == null)
                return;

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
                            ? (Mathf.Max(_car.CurrentSpeed, (wheel.Radius * Mathf.PI * wheel.RPM / SpeedDivider)) / SpeedDivider).Clamp()
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

                        _debugInfo[wheel].HasSlip = hasSlip;
                        _debugInfo[wheel].ParticleName = particles.name;
                        _debugInfo[wheel].P_Velocity = emitParams.velocity;
                        _debugInfo[wheel].P_StartSize = emitParams.startSize;
                        _debugInfo[wheel].P_StartLifetime = emitParams.startLifetime;
                    }
                }

                //Emit trail
                UpdateTrail(wheel, wheel.IsGrounded && hasSlip);
            }
        }

        private void HandleStopLights()
        {
            if (_stopLights.Length != 0)
            {
                for (int i = 0; i < _stopLights.Length; i++)
                {
                    bool activate = 
                        _car.IsBraking 
                        && _lastSpeed < _car.CarConfig.MaxSpeed - SpeedThresholdDif 
                        && _lastSpeed >= _car.CarConfig.CruiseSpeed + SpeedThresholdDif;

                    _stopLights[i]?.SetActive(activate);
                }

                _lastSpeed = _car.SpeedInDesiredUnits;
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

        public void UpdateTrail(Wheel wheel, bool emmit)
        {
            var trail = ActiveTrails[wheel];

            if (emmit)
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
                    if (!_car.Wheels[i].IsGrounded)
                        trail.Clear();

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
                trail = Instantiate(_trailPrefab, _vfxParent);
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
            if(trail.emitting)
                StartCoroutine(WaitVisibleTrail(trail));
        }

        /// <summary>
        /// The trail is considered busy until it disappeared.
        /// </summary>
        private IEnumerator WaitVisibleTrail(TrailRenderer trail)
        {
            trail.transform.SetParent(_vfxParent);
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
            if (
                this.enabled == false 
                || !car.IsVisible 
                || collision == null 
                || Time.time - _lastCollisionTime < _minTimeBetweenCollisions
               )
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

        #region Debug 

        [System.Serializable]
        private class VfxDebugInfo
        {
            [ReadOnly] public bool HasSlip;
            [ReadOnly] public string ParticleName;
            [ReadOnly] public Vector3 P_Velocity;
            [ReadOnly] public float P_StartSize;
            [ReadOnly] public float P_StartLifetime;
        }

        [Header("DEBUG ONLY")]
        [ShowInInspector]
        private Dictionary<Wheel, VfxDebugInfo> _debugInfo = new Dictionary<Wheel, VfxDebugInfo>();

        #endregion
    }
}

