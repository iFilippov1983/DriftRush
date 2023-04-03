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

		private Transform[] _childsView;
        private WheelHit _hit;
		private GroundConfig _currentGroundConfig;
		private WheelColliderHandler _wcHandler;
        private float _stiffnessMultiplier;

		private WheelFrictionCurve _initialForwardFriction;
		private WheelFrictionCurve _initialSidewaysFriction;

        private GroundDetection GroundDetection => Singleton<GroundDetection>.Instance;
		private GroundConfig DefaultGroundConfig => GroundDetection.DefaultGroundConfig;

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
		}

		private void LateUpdate()
		{
			UpdateTransform();
		}

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

                GroundConfig groundConfig = DefaultGroundConfig;
                if (GroundDetection.TryGetGroundEntity(_hit.collider.gameObject, out IGround groundEntity))
                {
                    groundConfig = groundEntity.Config;
                }

				CurrentGroundConfig = groundConfig;
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
            //float stiffness = _stiffnessMultiplier;
            //var friction = WheelCollider.forwardFriction;
            //friction.stiffness = stiffness;
            //WheelCollider.forwardFriction = friction;

            //friction = WheelCollider.sidewaysFriction;
            //friction.stiffness = stiffness * Mathf.Lerp(0.3f, 1, Mathf.InverseLerp(2, 1, ForwardSlipNormalized));
            //WheelCollider.sidewaysFriction = friction;

            float multiplier = _stiffnessMultiplier;

            var fFriction = _initialForwardFriction;
            fFriction.stiffness *= multiplier;

            var sFriction = _initialSidewaysFriction;
            sFriction.stiffness *= multiplier * Mathf.Lerp(0.3f, 1, Mathf.InverseLerp(2, 1, ForwardSlipNormalized));

			WheelColliderHandler.UpdateStiffness(fFriction.stiffness, sFriction.stiffness);
        }

        public void UpdateTransform()
		{
			Vector3 pos;
			Quaternion quat;
			WheelCollider.GetWorldPose(out pos, out quat);
            WheelView.position = pos;
            WheelView.rotation = quat;
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

