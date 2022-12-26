using UnityEngine;

namespace RaceManager.DamageSystem
{
    public interface IDetachable
    {
        public Vector3[] DamageCheckPoints { get; }
        public Transform Transform { get; }
        public void SetDamageForce(float force);
    }
}
