using UnityEngine;
using Sirenix.OdinInspector;
using RaceManager.Root;
using RaceManager.Effects;

namespace RaceManager.Cars
{
	/// <summary>
	/// Wheel settings and update logic.
	/// </summary>
	[System.Serializable]
	public class Wheel : MonoBehaviour
	{
        private const int WheelPosCheckInterval = 3;

		public WheelCollider WheelCollider;
		public Transform WheelView;
		[ReadOnly]
		public float SlipForGenerateParticle;
		[ReadOnly]
		public Vector3 TrailOffset;

		public float CurrentMaxSlip { get { return Mathf.Max(CurrentForwardSlip, CurrentSidewaysSlip); } }
		public float CurrentForwardSlip { get; private set; }
		public float CurrentSidewaysSlip { get; private set; }
        public float ForwardSlipNormalized { get; private set; }
        public float SidewaysSlipNormalized { get; private set; }
        public float SlipNormalized { get; private set; }

        public float Radius => WheelCollider.radius;
		public float RPM => WheelCollider.rpm;
        public WheelHit GetHit => _hit;
		public bool HasForwardSlip => CurrentForwardSlip > WheelCollider.forwardFriction.asymptoteSlip;
		public bool HasSideSlip => CurrentSidewaysSlip > WheelCollider.sidewaysFriction.asymptoteSlip;
		public bool IsGrounded => WheelCollider.isGrounded;
        public bool IsSteering { get; set; }

		private Transform[] _childsView;
        private Transform _thisTransform;
        private WheelHit _hit;
		private GroundConfig _currentGroundConfig;
		private WheelColliderHandler _wcHandler;
        private float _stiffnessMultiplier;

		private WheelFrictionCurve _initialForwardFriction;
		private WheelFrictionCurve _initialSidewaysFriction;

        private int _wheelPosCheckCounter;

        #region Minor variables

        private GroundConfig m_GroundConfigNew;

        private WheelFrictionCurve m_ForwardFriction;
        private WheelFrictionCurve m_SidewaysFriction;

        private Vector3 m_WheelPos;
        private Quaternion m_WheelRot;

        private float m_FrictionMultiplier;

        #endregion

        private GroundDetection GroundDetection => Singleton<GroundDetection>.Instance;
		private GroundConfig DefaultGroundConfig => GroundDetection.DefaultGroundConfig;

        public Transform Transform => _thisTransform;
        public GroundConfig CurrentGroundConfig
		{
			get => _currentGroundConfig;
			set 
			{
				if (_currentGroundConfig != value)
				{
					_currentGroundConfig = value;
					if (_currentGroundConfig != null)
					{
						_stiffnessMultiplier = _currentGroundConfig.WheelStiffnessMultiplier;
                    }
				}
			}
		}

        public WheelColliderHandler WheelColliderHandler
        {
            get
            {
                if (_wcHandler == null)
                {
                    _wcHandler = WheelCollider.GetComponent<WheelColliderHandler>();
                }
                if (_wcHandler == null)
                {
                    _wcHandler = WheelCollider.gameObject.AddComponent<WheelColliderHandler>();
                    _wcHandler.CheckFirstEnable();
                }
                return _wcHandler;
            }
        }

		private void FixedUpdate()
		{
			UpdateGameplayLogic();
            UpdateTransform();
        }

		//private void LateUpdate()
		//{
		//	UpdateTransform();
		//}

		public void InitializeSelf()
		{ 
			if(WheelCollider == null)
				WheelCollider = GetComponent<WheelCollider>();

			_childsView = new Transform[WheelView.childCount];
            for (int i = 0; i < _childsView.Length; i++)
            {
                _childsView[i] = WheelView.GetChild(i);
            }

			_initialForwardFriction = WheelCollider.forwardFriction;
			_initialSidewaysFriction = WheelCollider.sidewaysFriction;
            _thisTransform = transform;

			CurrentGroundConfig = DefaultGroundConfig;
        }

		private void UpdateGameplayLogic()
		{
            if (WheelCollider.GetGroundHit(out _hit))
            {
                CurrentForwardSlip = (CurrentForwardSlip + Mathf.Abs(_hit.forwardSlip)) / 2;
                CurrentSidewaysSlip = (CurrentSidewaysSlip + Mathf.Abs(_hit.sidewaysSlip)) / 2;

                ForwardSlipNormalized = CurrentForwardSlip / WheelCollider.forwardFriction.extremumSlip;
                SidewaysSlipNormalized = CurrentSidewaysSlip / WheelCollider.sidewaysFriction.extremumSlip;

                SlipNormalized = Mathf.Max(ForwardSlipNormalized, SidewaysSlipNormalized);

                m_GroundConfigNew = DefaultGroundConfig;
                if (GroundDetection.TryGetGroundEntity(_hit.collider.gameObject, out IGround groundEntity))
                {
                    m_GroundConfigNew = groundEntity.Config;
                }

				CurrentGroundConfig = m_GroundConfigNew;
            }
            else
            {
                CurrentForwardSlip = 0;
                CurrentSidewaysSlip = 0;
                ForwardSlipNormalized = 0;
                SidewaysSlipNormalized = 0;
                SlipNormalized = 0;
                CurrentGroundConfig = DefaultGroundConfig;
            }

			UpdateFriction();
        }

		private void UpdateFriction()
        {
            m_FrictionMultiplier = _stiffnessMultiplier;

            m_ForwardFriction = _initialForwardFriction;
            m_ForwardFriction.stiffness *= m_FrictionMultiplier;

            m_SidewaysFriction = _initialSidewaysFriction;
            m_SidewaysFriction.stiffness *= m_FrictionMultiplier * Mathf.Lerp(0.3f, 1, Mathf.InverseLerp(2, 1, ForwardSlipNormalized));

			WheelColliderHandler.UpdateStiffness(m_ForwardFriction.stiffness, m_SidewaysFriction.stiffness);
        }

        public void UpdateTransform()
		{
            _wheelPosCheckCounter++;

            if (_wheelPosCheckCounter == WheelPosCheckInterval)
            {
                _wheelPosCheckCounter = 0;

                WheelCollider.GetWorldPose(out m_WheelPos, out m_WheelRot);
                WheelView.position = m_WheelPos;
                WheelView.rotation = m_WheelRot;
            }
        }

        public void OnResetAction()
        {
            CurrentForwardSlip = 0;
            CurrentSidewaysSlip = 0;
            SlipNormalized = 0;
            CurrentGroundConfig = DefaultGroundConfig;
        }

        public void UpdateFrictionConfig(WheelColliderConfig config)
		{
			WheelColliderHandler.UpdateConfig(config);
		}
	}
}

