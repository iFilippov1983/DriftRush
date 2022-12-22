using Sirenix.OdinInspector;
using UnityEngine;

namespace RaceManager.DamageSystem
{
    /// <summary>
    /// Joint data, has the same variables as the standard joint.
    /// </summary>
    [System.Serializable]
    public struct PartJoint             
    {
        public Vector3 hingeAnchor;
        public Vector3 hingeAxis;
        public bool useLimits;
        public float minLimit;
        public float maxLimit;
        public float bounciness;
        public bool useSpring;
        public float springTargetPosition;
        public float springForce;
        public float springDamper;
        [Tooltip("For detachable part logic at speed (such as hood)")]
        public bool UseDetachSpeed;                    
        [ShowIf("UseDetachSpeed")]
        public float springTargetPositionMaxSpeed;
        [ShowIf("UseDetachSpeed")]
        public float springForceMaxSpeed;
        [ShowIf("UseDetachSpeed")]
        public float maxDetachSpeed;
        [ShowIf("UseDetachSpeed")]
        public int carDirection;
        [ShowIf("UseDetachSpeed")]
        public float DetachTime;
    }
}
