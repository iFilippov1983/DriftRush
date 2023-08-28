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

        #region Minor variables

        private Wheel m_Wheel;
        private GroundConfig m_GroundConfig;

        private TrailRenderer m_TrailCur;
        private TrailRenderer m_TrailNew;

        private ParticleSystem m_Particles;
        private Vector3 m_ParticlesPoint;
        private Vector3 m_ParticlesVelocity;

        private EmitParams m_EmitParams;

        #endregion

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
            for (int i = 0; i < _car.Wheels.Length; i++)
            {
                m_Wheel = _car.Wheels[i];
                m_GroundConfig = m_Wheel.CurrentGroundConfig;
                bool hasSlip = m_Wheel.HasForwardSlip || m_Wheel.HasSideSlip;

                //Emit particle.
                if (_car.IsVisible && m_GroundConfig != null)
                {
                    m_Particles = hasSlip ? m_GroundConfig.SlipParticles : m_GroundConfig.IdleParticles;
                    if (m_Particles)
                    {
                        float sizeAndLifeTimeMultiplier = (m_GroundConfig.SpeedDependent
                            ? (Mathf.Max(_car.CurrentSpeed, (m_Wheel.Radius * Mathf.PI * m_Wheel.RPM / SpeedDivider)) / SpeedDivider).Clamp()
                            : 1)
                            * Random.Range(0, 1f);

                        m_ParticlesPoint = m_Wheel.transform.position;
                        m_ParticlesPoint.y = m_Wheel.GetHit.point.y;

                        m_ParticlesVelocity = -m_Wheel.GetHit.forwardDir * m_Wheel.GetHit.forwardSlip;
                        m_ParticlesVelocity += m_Wheel.GetHit.sidewaysDir * m_Wheel.GetHit.sidewaysSlip;
                        m_ParticlesVelocity += _car.RB.velocity;

                        m_EmitParams = new EmitParams();

                        m_EmitParams.position = m_ParticlesPoint;
                        m_EmitParams.velocity = m_ParticlesVelocity;
                        m_EmitParams.startSize = Mathf.Max(1f, m_Particles.main.startSize.constant * sizeAndLifeTimeMultiplier);
                        m_EmitParams.startLifetime = m_Particles.main.startLifetime.constant * sizeAndLifeTimeMultiplier;
                        m_EmitParams.startColor = m_Particles.main.startColor.color;

                        m_Particles.Emit(m_EmitParams, 1);

                        _debugInfo[m_Wheel].HasSlip = hasSlip;
                        _debugInfo[m_Wheel].ParticleName = m_Particles.name;
                        _debugInfo[m_Wheel].P_Velocity = m_EmitParams.velocity;
                        _debugInfo[m_Wheel].P_StartSize = m_EmitParams.startSize;
                        _debugInfo[m_Wheel].P_StartLifetime = m_EmitParams.startLifetime;
                    }
                }

                //Emit trail
                UpdateTrail(m_Wheel, m_Wheel.IsGrounded && hasSlip);
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
            m_TrailCur = ActiveTrails[wheel];

            if (emmit)
            {
                if (m_TrailCur == null)
                {
                    //Get free or create trail.
                    m_TrailCur = GetTrail(wheel.WheelView.position + (wheel.transform.up * (-wheel.Radius + TrailOffset)));
                    m_TrailCur.transform.SetParent(wheel.transform);
                    ActiveTrails[wheel] = m_TrailCur;
                }
                else
                {
                    //Move the trail to the desired position
                    m_TrailCur.transform.position = wheel.WheelView.position + (wheel.transform.up * (-wheel.Radius + TrailOffset));
                }
            }
            else if (ActiveTrails[wheel] != null)
            {
                //Set trail as free.
                SetTrailAsFree(m_TrailCur);
                m_TrailCur = null;
                ActiveTrails[wheel] = m_TrailCur;
            }
        }

        private void ResetAllTrails()
        {
            //TrailRenderer trail;
            for (int i = 0; i < _car.Wheels.Length; i++)
            {
                m_TrailCur = ActiveTrails[_car.Wheels[i]];
                if (m_TrailCur)
                {
                    if (!_car.Wheels[i].IsGrounded)
                        m_TrailCur.Clear();

                    SetTrailAsFree(m_TrailCur);
                    m_TrailCur = null;
                    ActiveTrails[_car.Wheels[i]] = m_TrailCur;
                }
            }
        }

        /// <summary>
        /// Get first free trail and set start position.
        /// </summary>
        public TrailRenderer GetTrail(Vector3 startPos)
        {
            //TrailRenderer trail;
            if (_freeTrails.Count > 0)
            {
                m_TrailNew = _freeTrails.Dequeue();
            }
            else
            {
                m_TrailNew = Instantiate(_trailPrefab, _vfxParent);
            }

            m_TrailNew.transform.position = startPos;
            m_TrailNew.gameObject.SetActive(true);
            m_TrailNew.Clear();

            return m_TrailNew;
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

