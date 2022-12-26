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
        public float Radius => WheelCollider.radius;
		public float RPM => WheelCollider.rpm;
        public WheelHit GetHit => _hit;
		
		public bool HasForwardSlip => CurrentForwardSlip > WheelCollider.forwardFriction.asymptoteSlip;
		public bool HasSideSlip => CurrentSidewaysSlip > WheelCollider.sidewaysFriction.asymptoteSlip;
		public bool IsGrounded => WheelCollider.isGrounded;

        WheelHit _hit;
		TrailRenderer _currentTrail;
		GroundConfig _currentGroundConfig;
		WheelColliderHandler _wcHandler;
        Vector3 HitPoint;
        float GroundStiffness;

        GroundDetection GroundDetection => Singleton<GroundDetection>.Instance;
		GroundConfig DefaultGroundConfig => GroundDetection.DefaultGroundConfig;
		CarFXController FXController => Singleton<CarFXController>.Instance;
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
						GroundStiffness = _currentGroundConfig.WheelStiffness;
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

        /// <summary>
        /// Update gameplay logic.
        /// </summary>
        public void FixedUpdate()
		{

			if (WheelCollider.GetGroundHit(out _hit))
			{
				var prevForwar = CurrentForwardSlip;
				var prevSide = CurrentSidewaysSlip;

				CurrentForwardSlip = (prevForwar + Mathf.Abs(_hit.forwardSlip)) / 2;
				CurrentSidewaysSlip = (prevSide + Mathf.Abs(_hit.sidewaysSlip)) / 2;
			}
			else
			{
				CurrentForwardSlip = 0;
				CurrentSidewaysSlip = 0;
			}
		}

		private void InitSelf()
		{ 
			
		}

		/// <summary>
		/// Update visual logic (Transform, FX).
		/// </summary>
		public void UpdateVisual()
		{
			UpdateTransform();

			if (WheelCollider.isGrounded && CurrentMaxSlip > SlipForGenerateParticle)
			{
				//Emit particle.
				var particles = FXController.AspahaltParticles;
				var point = WheelCollider.transform.position;
				point.y = _hit.point.y;
				particles.transform.position = point;
				particles.Emit(1);

				if (_currentTrail == null)
				{
					//Get free or create trail.
					HitPoint = WheelCollider.transform.position;
					HitPoint.y = _hit.point.y;
					_currentTrail = FXController.GetTrail(HitPoint);
					_currentTrail.transform.SetParent(WheelCollider.transform);
					_currentTrail.transform.localPosition += TrailOffset;
				}
			}
			else if (_currentTrail != null)
			{
				//Set trail as free.
				FXController.SetFreeTrail(_currentTrail);
				_currentTrail = null;
			}
		}

		public void UpdateTransform()
		{
			Vector3 pos;
			Quaternion quat;
			WheelCollider.GetWorldPose(out pos, out quat);
			WheelView.position = pos;
			WheelView.rotation = quat;
		}

		public void UpdateFrictionConfig(WheelColliderConfig config)
		{
			WheelColliderHandler.UpdateConfig(config);
		}
	}
}

