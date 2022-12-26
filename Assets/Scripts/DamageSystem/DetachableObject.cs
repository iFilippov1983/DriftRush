using RaceManager.Cars;
using System.Threading.Tasks;
using UnityEngine;

namespace RaceManager.DamageSystem
{
    /// <summary>
    /// Detachable object (not damageable).
    /// </summary>
    [DisallowMultipleComponent]
    public class DetachableObject : MonoBehaviour, IDetachable
    {
        private const float SecondsBeforeDestroy = 10f;

        [Tooltip("Rigidbody data to tranfer to created RB after this part is lost")]
        [SerializeField] private float _mass = 0.1f;

        [Tooltip("Rigidbody data to tranfer to created RB after this part is lost")]
        [SerializeField] private float _drag;

        [Tooltip("Rigidbody data to tranfer to created RB after this part is lost")]
        [SerializeField] private float _angularDrag = 0.05f;

        [Tooltip("The force at which Joint is created. If it is <= 0 then the object will not detach.")]
        [SerializeField] private float LooseForce = 1;

        [Tooltip("Break force transferred to the created Joint.")]
        [SerializeField] private float _breakForce = 25;

        [Tooltip("Possible Joints, when a part is lost, a random one wil be selected.")]
        [SerializeField] private PartJoint[] _joints;

        [Tooltip("Break check points, there must be at least one point.")]
        [SerializeField] private Vector3[] _damageCheckPoints = new Vector3[1] { Vector3.zero };

        [Tooltip("Use for the additional damage effect, if the flag is not set, then to lose the part, you need to get LooseForce in one collision")]
        [SerializeField] private bool _useLoseHealth = true;                                       

        private bool _childsAreDestroyed;
        private IDamageable[] _destroyableChilds;
        private Collider[] _childsColliders;

        private Transform _thisTransform;
        private Rigidbody _thisRb;
        private Rigidbody _parentRb;
        private HingeJoint _hinge;
        private VehicleDamageController _vehicleDamageController;
        private Car _car;
        private Vector3 _initialJointPos;
        private Vector3 _thisInitialLocalPos;
        private float _looseHealth;

        private float _detachTimer = 0;
        private PartJoint _chosenJoint;

        /// <summary>
        /// Does this part have Rigidbody and is connected to parent by a Joint
        /// </summary>
        private bool _isLost;
        /// <summary>
        /// Is this part completely detached form the car
        /// </summary>
        private bool _isDetached;

        public Transform Transform => _thisTransform;
        public Vector3[] DamageCheckPoints => _damageCheckPoints;

        #region Unity Functions

        void OnEnable()
        {
            _thisTransform = transform;
            _destroyableChilds = GetComponentsInChildren<IDamageable>();
            _childsColliders = GetComponentsInChildren<Collider>();
            _parentRb = _thisTransform.GetTopmostParentComponent<Rigidbody>();
            _vehicleDamageController = GetComponentInParent<VehicleDamageController>();
            if (_vehicleDamageController)
            {
                _car = _vehicleDamageController.GetComponent<Car>();
            }
            _thisInitialLocalPos = _thisTransform.localPosition;
            _looseHealth = LooseForce;
            if (LooseForce == 0)
            {
                SetAsLost();
            }
        }

        void OnCollisionEnter(Collision col)
        {
            if (!_isDetached && _isLost)
            {
                //Damage to a car if there is a connection with it through a joint.
                _vehicleDamageController.OnCollisionEnter(col);
                if (_car)
                {
                    _car.OnCollisionEnter(col);
                }
            }
        }

        void Update()
        {
            DestroyChildsIfLost();
        }

        /// <summary>
        /// Forced Break Joint.
        /// </summary>
        private void OnJointBreak()
        {
            this.enabled = false;
            _isDetached = true;
            if (_hinge)
            {
                Destroy(_hinge);
                _thisRb.velocity *= 0.5f;
                _thisRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }

            _thisTransform.parent = null;

            Destroy(gameObject, SecondsBeforeDestroy);
        }

        private void OnDrawGizmosSelected()
        {
            if (!_thisTransform)
            {
                _thisTransform = transform;
            }

            if (LooseForce >= 0 && _joints.Length > 0)
            {
                Gizmos.color = Color.yellow;
                foreach (PartJoint curJoint in _joints)
                {
                    var pos = _thisTransform.TransformPoint(curJoint.hingeAnchor);

                    Gizmos.DrawRay(pos, _thisTransform.TransformDirection(curJoint.hingeAxis).normalized * 0.2f);
                    Gizmos.DrawWireSphere(pos, 0.02f);
                }
            }

            Gizmos.color = Color.green;
            foreach (var checkPoint in DamageCheckPoints)
            {
                Gizmos.DrawWireSphere(_thisTransform.TransformPoint(checkPoint), 0.05f);
            }
        }

        #endregion

        private void DestroyChildsIfLost()
        {
            //Destruction of all damaged child objects.
            if (_isLost)
            {
                if (!_childsAreDestroyed)
                {
                    for (int i = 0; i < _destroyableChilds.Length; i++)
                    {
                        _destroyableChilds[i].Kill();
                    }
                    _childsAreDestroyed = true;
                }
                bool needJointBreak = !_hinge || transform.parent != null && (transform.parent.TransformPoint(_initialJointPos) - transform.TransformPoint(_hinge.anchor)).sqrMagnitude > 0.25;

                if (_hinge && _chosenJoint.UseDetachSpeed)
                {
                    //Applying a spring to a detachable part
                    float speedNormalize = Mathf.InverseLerp(0, _chosenJoint.maxDetachSpeed, _car.CurrentSpeed);
                    float signDirection = Mathf.Sign(_chosenJoint.carDirection * _car.VehicleDirection);
                    var spring = _hinge.spring;
                    spring.targetPosition = signDirection > 0 ? _chosenJoint.springTargetPositionMaxSpeed : _chosenJoint.springTargetPosition;
                    spring.spring = Mathf.Lerp(_chosenJoint.springForce, _chosenJoint.springForceMaxSpeed, speedNormalize);
                    _hinge.spring = spring;
                    _hinge.useSpring = true;

                    if (_car.CurrentSpeed > _chosenJoint.maxDetachSpeed && signDirection > 0)
                    {
                        _detachTimer += Time.deltaTime;
                        needJointBreak |= _detachTimer >= _chosenJoint.DetachTime;
                    }
                    else
                    {
                        _detachTimer = 0;
                    }
                }

                if (needJointBreak)
                {
                    OnJointBreak();
                }
            }
        }

        public void SetDamageForce(float force)
        {
            if (!_isLost && LooseForce > 0)
            {
                if (_useLoseHealth)
                {
                    //Additional damage effect
                    _looseHealth -= force;
                    if (_looseHealth <= 0)
                    {
                        SetAsLost();
                    }
                }
                else
                {
                    //Check looseForce in one collision.
                    if (force >= LooseForce)
                    {
                        SetAsLost();
                    }
                }
            }
        }

        /// <summary>
        /// Loss of detail, here a ridge body is created, and a joint is created if the part is not completely lost and there is an available joint.
        /// </summary>
        private void SetAsLost()
        {
            if (!_isLost)
            {
                _isLost = true;
                _thisRb = gameObject.AddComponent<Rigidbody>();
                _thisRb.mass = _mass;
                _thisRb.drag = _drag;
                _thisRb.angularDrag = _angularDrag;

                for (int i = 0; i < _childsColliders.Length; i++)
                {
                    _childsColliders[i].enabled = true;
                }

                if (_parentRb)
                {
                    _parentRb.mass -= _mass;
                    _thisRb.velocity = _parentRb.GetPointVelocity(_thisTransform.position);
                    _thisRb.angularVelocity = _parentRb.angularVelocity;

                    if (_joints.Length > 0)
                    {
                        _chosenJoint = _joints[Random.Range(0, _joints.Length)];

                        _hinge = gameObject.AddComponent<HingeJoint>();
                        _hinge.autoConfigureConnectedAnchor = false;
                        _hinge.connectedBody = _parentRb;
                        _hinge.anchor = _chosenJoint.hingeAnchor;
                        _hinge.axis = _chosenJoint.hingeAxis;
                        _hinge.connectedAnchor = _thisInitialLocalPos + _chosenJoint.hingeAnchor;
                        _hinge.enableCollision = false;
                        _hinge.useLimits = _chosenJoint.useLimits;

                        JointLimits limits = new JointLimits();
                        limits.min = _chosenJoint.minLimit;
                        limits.max = _chosenJoint.maxLimit;
                        limits.bounciness = _chosenJoint.bounciness;
                        _hinge.limits = limits;
                        _hinge.useSpring = _chosenJoint.useSpring;

                        JointSpring spring = new JointSpring();
                        spring.targetPosition = _chosenJoint.springTargetPosition;
                        spring.spring = _chosenJoint.springForce;
                        spring.damper = _chosenJoint.springDamper;
                        _hinge.spring = spring;
                        _hinge.breakForce = _breakForce;
                        _hinge.breakTorque = float.PositiveInfinity;
                        if (_thisTransform.parent)
                        {
                            _initialJointPos = _thisTransform.parent.InverseTransformPoint(transform.TransformPoint(_chosenJoint.hingeAnchor));
                        }
                    }
                }
                else
                {
                    _thisTransform.parent = null;
                }
            }
        }
    }
}
