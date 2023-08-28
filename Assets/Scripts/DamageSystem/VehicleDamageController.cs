using ModestTree;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RaceManager.DamageSystem
{
    /// <summary>
    /// Car damage controller. Visual and technical effects of damage.
    /// </summary>
    [RequireComponent(typeof(Cars.Car))]
    public class VehicleDamageController : MonoBehaviour
    {
#pragma warning disable 0649

        [Range(0, 5)]
        [SerializeField] private float _damageFactor = 1;
        [SerializeField] private float _maxCollisionMagnitude = 50;
        [SerializeField] private float _maxImpulseMagnitude = 35;

        [Tooltip("Radius of influence on the vertices (At maximum magnitude).")]
        [SerializeField] private float _maxDeformRadiusInMaxMag = 3;

        [Tooltip("Radius of influence on the vertices (At maximum magnitude).")]
        [SerializeField] private float _deformMultiplier = 0.3f;

        [Tooltip("The maximum number of contacts per collision.")]
        [SerializeField] private int _maxContactPoints = 2;

        [Tooltip("The minimum interval between collisions.")]
        [SerializeField] private float _minTimeBetweenCollisions = 0.1f;

        [Tooltip("To calculate the intensity of damage (In order to use sqrMagnitude for optimization).")]
        [SerializeField] private AnimationCurve _damageDistanceCurve;

        [SerializeField] private bool _useNoise = true;
        [SerializeField] private int _noiseSize = 10;
        [SerializeField] private bool _calculateNormals = true;

        //The search for meshes and colliders is automatic. Lists are needed to catch exceptions. For example to catch a wheel renderer or a wheel collider.
        [SerializeField] private List<MeshFilter> IgnoreDeformMeshes = new List<MeshFilter>();
        [SerializeField] private List<MeshCollider> IgnoreDeformColliders = new List<MeshCollider>();
        [SerializeField] private List<Transform> IgnoreFindInChildsMeshesAndColliders = new List<Transform>();

        [Tooltip("Сollision log and gizmo for debug.")]
        [SerializeField] private bool _enableLogAndGizmo;

#pragma warning restore 0649

        private Transform _thisTransform;
        private Rigidbody _thisRb;

        private DamageableMeshData[] _damageableMeshes;
        private DamageableObjectData[] _damageableObjects;
        private IDetachable[] _detachables;

        private float _lastHitTime;
        private Vector3 _localCenterPoint;

        #region Minor variables

        private List<DamageableObjectData> _tempListDod;
        private List<DamageableMeshData> _tempListDmd;
        private List<IDetachable> _tempListDetach;

        private ContactPoint m_Contact;
        private Vector3 m_Force = Vector3.zero;
        private Vector3 m_Normal = Vector3.zero;
        private Vector3 m_ForceResult;
        private Vector3 m_NormalResult;
        private Rigidbody m_Rb;

        #endregion

        private Vector3 CenterPoint => _thisTransform.TransformPoint(_localCenterPoint);

        public event System.Action<DamageData> OnDamageAction;

        #region Unity Functions

        void Start()
        {
            _thisTransform = transform;
            _thisRb = GetComponent<Rigidbody>();

            SetUpMeshColliders(SetDamageableMeshesData());
        }

        public void OnCollisionEnter(Collision col)
        {
            OnCollision(col, false);
        }

        public void OnCollisionStay(Collision collision)
        {
            OnCollision(collision, true);
        }

        #endregion

        #region Private Functions

        private List<DamageableMeshData> SetDamageableMeshesData()
        {
            DamageableMeshData meshData;

            var damageableMeshes = new List<DamageableMeshData>();

            var deformFilters = GetComponentsInChildren<MeshFilter>().Where(m =>
                !IgnoreDeformMeshes.Contains(m) &&
                m.mesh.isReadable &&
                IgnoreFindInChildsMeshesAndColliders.All(p => !m.transform.CheckParent(p))
            );

            if (deformFilters.Count() == 0)
            {
                Debug.LogError("DeformMeshes not found (Maybe the 'Read/Write Enabled' checkbox is disabled)");
            }

            //Set up mesh data.
            foreach (var filter in deformFilters)
            {
                meshData = new DamageableMeshData();
                meshData.Transform = filter.transform;
                meshData.Mesh = filter.mesh;

                var collider = filter.GetComponent<MeshCollider>();
                if (collider != null && !IgnoreDeformColliders.Contains(collider))
                {
                    collider.sharedMesh = meshData.Mesh;
                    meshData.MeshCollider = collider;
                }

                meshData.Verts = meshData.Mesh.vertices;
                meshData.Damaged = false;

                damageableMeshes.Add(meshData);
            }

            return damageableMeshes;
        }

        private void SetUpMeshColliders(List<DamageableMeshData> damageableMeshes)
        {
            DamageableMeshData meshData;

            var deformColliders = GetComponentsInChildren<MeshCollider>().Where(c =>
                !IgnoreDeformColliders.Contains(c) &&
                c.sharedMesh != null &&
                c.sharedMesh.isReadable &&
                IgnoreFindInChildsMeshesAndColliders.All(p => !c.transform.CheckParent(p)) &&
                !damageableMeshes.Any(m => m.MeshCollider == c)
                );

            //Set up mesh collider data.
            foreach (var collider in deformColliders)
            {
                meshData = new DamageableMeshData();
                meshData.MeshCollider = collider;
                meshData.Transform = collider.transform;
                meshData.Mesh = (Mesh)Instantiate(collider.sharedMesh);
                meshData.Verts = collider.sharedMesh.vertices;
                meshData.Damaged = false;
                damageableMeshes.Add(meshData);
            }

            _damageableMeshes = damageableMeshes.ToArray();

            _detachables = GetComponentsInChildren<IDetachable>();

            var damageableObjects = GetComponentsInChildren<IDamageable>();
            _damageableObjects = new DamageableObjectData[damageableObjects.Length];
            for (int i = 0; i < damageableObjects.Length; i++)
            {
                _damageableObjects[i] = new DamageableObjectData(damageableObjects[i]);
            }

            _localCenterPoint = Vector3.zero;
            int vertsCount = 0;
            foreach (var damageableMesh in _damageableMeshes)
            {
                foreach (var vert in damageableMesh.Mesh.vertices)
                {
                    _localCenterPoint += damageableMesh.Transform.localPosition + vert;
                    vertsCount++;
                }
            }

            _localCenterPoint /= vertsCount;
        }

        private void OnCollision(Collision col, bool fromStayCollistion)
        {
            if (Time.time - _lastHitTime < _minTimeBetweenCollisions)
            {
                return;
            }

            //ContactPoint contact;
            m_Force = Vector3.zero;
            m_Normal = Vector3.zero;
            //Vector3 forceResult;
            //Vector3 normalResult;
            float massFactor;
            //Rigidbody rb;
            int contactsCount = Mathf.Min(col.contacts.Length, _maxContactPoints);

            float impuls = (col.impulse.magnitude * 0.001f).Clamp(0, _maxImpulseMagnitude);
            float relativeVelocityMagnitude = col.relativeVelocity.magnitude;

            m_Rb = col.rigidbody;
            if (m_Rb != null && !m_Rb.isKinematic)
            {
                var percent = (m_Rb.mass / _thisRb.mass).Clamp();
                massFactor = percent <= 0.1f ? Mathf.Sqrt(percent) : 1;    //To prevent damage from small objects (eg cones).
            }
            else
            {
                massFactor = 1;
            }

            if (Mathf.Max(relativeVelocityMagnitude, fromStayCollistion ? impuls : 0) * _damageFactor > 1)
            {
                if (fromStayCollistion && impuls > relativeVelocityMagnitude)
                {
                    m_Force = Vector3.ClampMagnitude(col.impulse * 0.001f, _maxImpulseMagnitude);
                    m_Normal = col.impulse.normalized;
                }

                for (int i = 0; i < contactsCount; i++)
                {
                    m_Contact = col.contacts[i];

                    if (fromStayCollistion)
                    {
                        if (relativeVelocityMagnitude > impuls)
                        {
                            m_Normal = m_Contact.normal;
                            m_Force = col.relativeVelocity;
                        }

                        if (fromStayCollistion && Vector3.Dot(m_Force, m_Contact.point - CenterPoint) > 0)
                        {
                            m_ForceResult = -m_Force;
                            m_NormalResult = -m_Normal;
                        }
                        else
                        {
                            m_ForceResult = m_Force;
                            m_NormalResult = m_Normal;
                        }
                    }
                    else
                    {
                        m_ForceResult = col.relativeVelocity;
                        m_NormalResult = m_Contact.normal;
                    }

                    SetDamage(new DamageData()
                    {
                        DamagePoint = m_Contact.point,
                        DamageForce = m_ForceResult,
                        SurfaceNormal = m_NormalResult,
                        MassFactor = massFactor,
                    });

                    if (_enableLogAndGizmo)
                    {
                        CurrentGizmoIndex = MathExtentions.Repeat(CurrentGizmoIndex + 1, 0, GizmosData.Length - 1);
                        GizmosData[CurrentGizmoIndex].ContactPoint = m_Contact.point;
                        GizmosData[CurrentGizmoIndex].Force = m_ForceResult;
                        GizmosData[CurrentGizmoIndex].Normal = m_NormalResult;
                    }
                }

                if (_enableLogAndGizmo)
                {
                    if (!fromStayCollistion || relativeVelocityMagnitude > impuls)
                    {
                        Debug.LogFormat("relativeVelocity {0}", relativeVelocityMagnitude);
                    }
                    else
                    {
                        Debug.LogFormat("impulse {0}", impuls);
                    }
                }

                FinalizeDamage();
                _lastHitTime = Time.time;
            }
        }

        /// <summary>
        /// Set damage to the meshes and all damaged objects.
        /// </summary>
        private void SetDamage(DamageData data)
        {
            Vector3 damagePoint = data.DamagePoint;
            Vector3 damageForce = data.DamageForce;
            Vector3 surfaceNormal = data.SurfaceNormal;

            //Declaring shared variables.
            DamageableMeshData curDamageMesh;

            float sqrDist;
            float percent;

            Vector3 localDamagePoint;
            Vector3 localDamageForceAndSurfaceDot;

            //Сalculate all the necessary values.
            Vector3 clampForce = Vector3.ClampMagnitude(damageForce, _maxCollisionMagnitude);                                //Limiting force if force exceeds maximum.
            Vector3 normalizedForce = clampForce.normalized;
            float forceMagFactor = clampForce.magnitude * _damageFactor * data.MassFactor;                                   //Accept all existing factors.
            float maxDamageRadius = _maxDeformRadiusInMaxMag * (forceMagFactor / _maxCollisionMagnitude);
            float sqrMaxDamageRadius = Mathf.Pow(maxDamageRadius, 2);
            float maxDeformDist = _deformMultiplier * (forceMagFactor / _maxCollisionMagnitude);
            float sqrMaxDeformDist = Mathf.Pow(maxDeformDist, 2);                                                          //Calculation of the square of the maximum damage distance.
            float surfaceDot = Mathf.Clamp01(Vector3.Dot(surfaceNormal, normalizedForce)) *
                (Vector3.Dot((CenterPoint - damagePoint).normalized, normalizedForce) + 1) * 0.3f;                         //Calculation of surfaceDot to reduce the tangential damage force.
            float deformSurfaceDot = surfaceDot * 0.01f * _damageFactor;                                       //Applying all multipliers and decreasing by 100 surfaceDot for an adequate result.

            if (surfaceDot <= 0.02f)
            {
                return;
            }

            Bounds damageBounds = new Bounds(Vector3.zero, new Vector3(maxDamageRadius, maxDamageRadius, maxDamageRadius));

            //Damage to all meshes.
            for (int i = 0; i < _damageableMeshes.Length; i++)
            {
                curDamageMesh = _damageableMeshes[i];
                try
                {
                    damageBounds.center = curDamageMesh.Transform.InverseTransformPoint(damagePoint);
                }
                catch
                {
                    _tempListDmd = new List<DamageableMeshData>(_damageableMeshes);
                    _tempListDmd.Remove(_damageableMeshes[i]);
                    _damageableMeshes = _tempListDmd.ToArray();
                    continue;
                }
                
                if (!curDamageMesh.Mesh.bounds.Intersects(damageBounds))
                {
                    continue;
                }

                localDamagePoint = curDamageMesh.Transform.InverseTransformPoint(damagePoint);
                localDamageForceAndSurfaceDot = curDamageMesh.Transform.InverseTransformDirection(clampForce) * deformSurfaceDot;

                for (int j = 0; j < _damageableMeshes[i].Verts.Length; j++)
                {
                    //The squares of lengths are used everywhere for optimization, 
                    //then the percentage of the distance from the square of the maximum distance is found. 
                    //The calculation is not accurate, but visually the damage looks good.
                    sqrDist = (_damageableMeshes[i].Verts[j] - localDamagePoint).sqrMagnitude;
                    if (sqrDist < sqrMaxDamageRadius)
                    {
                        percent = _deformMultiplier * _damageDistanceCurve.Evaluate(sqrDist / sqrMaxDamageRadius);

                        _damageableMeshes[i].Verts[j] += localDamageForceAndSurfaceDot * percent *
                            (_useNoise ? 0.5f + Mathf.PerlinNoise(_damageableMeshes[i].Verts[j].x * _noiseSize, _damageableMeshes[i].Verts[j].y * _noiseSize) : 1);

                        _damageableMeshes[i].Damaged = true;
                    }
                }
            }

            //Further, similar algorithms are applied to all similar objects.

            IDamageable damageableObject;

            for (int i = 0; i < _damageableObjects.Length; i++)
            {
                damageableObject = _damageableObjects[i].DamageableObject;

                if (damageableObject == null || damageableObject.IsDead)
                {
                    continue;
                }

                try
                {
                    sqrDist = (damageableObject.Transform.TransformPoint(damageableObject.LocalCenterPoint) - damagePoint).sqrMagnitude;

                    if (sqrDist < sqrMaxDamageRadius)
                    {
                        percent = _damageDistanceCurve.Evaluate(sqrDist / sqrMaxDamageRadius);
                        _damageableObjects[i].TrySetMaxDamage(forceMagFactor * percent * surfaceDot);
                    }
                }
                catch
                {
                    _tempListDod = new List<DamageableObjectData>(_damageableObjects);
                    _tempListDod.Remove(_damageableObjects[i]);
                    _damageableObjects = _tempListDod.ToArray();
                    continue;
                }
            }

            IDetachable detachableObject;
            localDamagePoint = Vector3.zero;

            for (int i = 0; i < _detachables.Length; i++)
            {
                detachableObject = _detachables[i];
                try
                {
                    localDamagePoint = detachableObject.Transform.InverseTransformPoint(damagePoint);
                }
                catch
                { 
                    _tempListDetach = new List<IDetachable>(_detachables);
                    _tempListDetach.Remove(_detachables[i]);
                    _detachables = _tempListDetach.ToArray();
                    continue;
                }

                localDamageForceAndSurfaceDot = detachableObject.Transform.InverseTransformDirection(clampForce) * deformSurfaceDot;

                for (int j = 0; j < detachableObject.DamageCheckPoints.Length; j++)
                {
                    sqrDist = (detachableObject.DamageCheckPoints[j] - localDamagePoint).sqrMagnitude;
                    if (sqrDist < sqrMaxDamageRadius)
                    {
                        percent = _damageDistanceCurve.Evaluate(sqrDist / sqrMaxDamageRadius);
                        detachableObject.SetDamageForce(forceMagFactor * surfaceDot * percent);
                    }
                }
            }

            OnDamageAction.SafeInvoke(data);
        }

        /// <summary>
        /// Apply all damage to meshes and colliders.
        /// </summary>
        private void FinalizeDamage()
        {
            for (int i = 0; i < _damageableMeshes.Length; i++)
            {
                if (_damageableMeshes[i].Damaged)
                {
                    if (_damageableMeshes[i].Mesh)
                    {
                        _damageableMeshes[i].Mesh.vertices = _damageableMeshes[i].Verts;

                        if (_calculateNormals)
                        {
                            _damageableMeshes[i].Mesh.RecalculateNormals();
                        }

                        _damageableMeshes[i].Mesh.RecalculateBounds();
                    }

                    if (_damageableMeshes[i].MeshCollider)
                    {
                        _damageableMeshes[i].MeshCollider.sharedMesh = null;
                        _damageableMeshes[i].MeshCollider.sharedMesh = _damageableMeshes[i].Mesh;
                    }

                    _damageableMeshes[i].Damaged = false;
                }
            }

            for (int i = 0; i < _damageableObjects.Length; i++)
            {
                _damageableObjects[i].ApplyDamage();
            }
        }

        #endregion

        #region Draw Gizmos Functions

        private int CurrentGizmoIndex;
        private GizmoData[] GizmosData = new GizmoData[20];

        private void OnDrawGizmosSelected()
        {
            if (_enableLogAndGizmo)
            {
                foreach (var gizmoData in GizmosData)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(gizmoData.ContactPoint, 0.1f);

                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(gizmoData.ContactPoint, gizmoData.ContactPoint + gizmoData.Normal);

                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(gizmoData.ContactPoint, gizmoData.ContactPoint + (gizmoData.Force * 0.1f));
                }
            }
        }

        private struct GizmoData
        {
            public Vector3 ContactPoint;
            public Vector3 Normal;
            public Vector3 Force;
        }

        #endregion
    }
}
