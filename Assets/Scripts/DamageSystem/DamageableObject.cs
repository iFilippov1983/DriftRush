using RaceManager.Effects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.DamageSystem
{
    /// <summary>
    /// Damageable object base class
    /// </summary>
    public class DamageableObject : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _health = 100;

        [Tooltip("Maximum damage done at one time")]
        [SerializeField] private float _maxDamage = float.PositiveInfinity;

        private MeshFilter _meshFilter;
        private Vector3? _localCenterPoint;     
        
        private MeshFilter MeshFilter
        {
            get
            {
                if (!_meshFilter)
                    _meshFilter = GetComponent<MeshFilter>();
                return _meshFilter;
            }
        }

        /// <summary>
        /// Local center of mass of vertices, if there is a mesh filter.
        /// </summary>
        public Vector3 LocalCenterPoint
        {
            get
            {
                if (!MeshFilter)
                {
                    return Vector3.zero;
                }
                if (!_localCenterPoint.HasValue)
                {
                    Vector3 sum = Vector3.zero;
                    foreach (var vert in MeshFilter.sharedMesh.vertices)
                        sum += vert;
                    _localCenterPoint = sum / MeshFilter.sharedMesh.vertices.Length;
                }

                return _localCenterPoint.Value;
            }
        }

        public bool IsDead => _health <= 0; 
        public bool IsInited { get; private set; }

        public float InitHealth { get; private set; }
        public float HealthPercent { get; private set; }

        public event Action<float> OnChangeHealth;
        public event Action OnDeath;

        private void Awake()
        {
            InitDamageObject();
        }

        protected virtual void InitDamageObject()
        {
            if (!IsInited)
            {
                IsInited = true;
                InitHealth = _health;
                HealthPercent = 1;
            }
        }

        protected float GetClampedDamage(float damage)
        {
            return damage.Clamp(0, _maxDamage);
        }

        public virtual void SetDamage(float damage)
        {
            damage = GetClampedDamage(damage);
            if (IsDead)
                return;

            _health -= damage;
            HealthPercent = (_health / InitHealth).Clamp();
            OnChangeHealth.SafeInvoke(-damage);

            if (_health <= 0)
            {
                Death();
            }
        }

        /// <summary>
        /// Destroy the object completely.
        /// </summary>
        public void Kill()
        {
            if (!IsDead)
            {
                OnChangeHealth.SafeInvoke(-_health);
                HealthPercent = 0;
                _health = 0;
                Death();
            }
        }

        /// <summary>
        /// The method called after death.
        /// </summary>
        private void Death()
        {
            OnDeath.SafeInvoke();
            DoDeath();
        }

        /// <summary>
        /// Method to specify object death
        /// </summary>
        protected virtual void DoDeath() { }
    }
}

