using UnityEngine;

namespace RaceManager.DamageSystem
{
    public interface IDamageable
    {
        public bool IsDead { get; }
        public Transform Transform { get; }
        public Vector3 LocalCenterPoint { get; }
        public void SetDamage(float damage);
        public void Kill();
    }
}
