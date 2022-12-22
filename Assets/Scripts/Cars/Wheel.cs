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
	public struct Wheel
	{
		public WheelCollider WheelCollider;
		public Transform WheelView;
		[ReadOnly]
		public float SlipForGenerateParticle;
		[ReadOnly]
		public Vector3 TrailOffset;

		public float CurrentMaxSlip { get { return Mathf.Max(CurrentForwardSleep, CurrentSidewaysSleep); } }
		public float CurrentForwardSleep { get; private set; }
		public float CurrentSidewaysSleep { get; private set; }
		public WheelHit GetHit { get { return _hit; } }

		WheelHit _hit;
		TrailRenderer _currentTrail;
		GroundConfig _currentGroundConfig;
		WheelColliderHandler m_PGWC;

		public WheelColliderHandler WcHandler => m_PGWC;

		public GroundConfig CurrentGroundConfig => _currentGroundConfig;

		public WheelColliderHandler WheelColliderHandler
		{
			get
			{
				if (m_PGWC == null)
				{
					m_PGWC = WheelCollider.GetComponent<WheelColliderHandler>();
				}
				if (m_PGWC == null)
				{
					m_PGWC = WheelCollider.gameObject.AddComponent<WheelColliderHandler>();
					m_PGWC.CheckFirstEnable();
				}
				return m_PGWC;
			}
		}

		GroundDetection GroundDetection => Singleton<GroundDetection>.Instance;
		GroundConfig DefaultGroundConfig => GroundDetection.DefaultGroundConfig;
		CarFXController FXController => Singleton<CarFXController>.Instance;
		Vector3 HitPoint;

		/// <summary>
		/// Update gameplay logic.
		/// </summary>
		public void FixedUpdate()
		{

			if (WheelCollider.GetGroundHit(out _hit))
			{
				var prevForwar = CurrentForwardSleep;
				var prevSide = CurrentSidewaysSleep;

				CurrentForwardSleep = (prevForwar + Mathf.Abs(_hit.forwardSlip)) / 2;
				CurrentSidewaysSleep = (prevSide + Mathf.Abs(_hit.sidewaysSlip)) / 2;
			}
			else
			{
				CurrentForwardSleep = 0;
				CurrentSidewaysSleep = 0;
			}
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

