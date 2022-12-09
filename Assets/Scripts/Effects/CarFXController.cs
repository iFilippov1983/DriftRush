using RaceManager.Root;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Effects
{
	/// <summary>
	/// Cars vFX and sFX handler.
	/// </summary>
	public class CarFXController : MonoBehaviour
	{

		[Header("Particles settings")]
		[SerializeField] ParticleSystem AsphaltSmokeParticles;      

		[Header("Trail settings")]
		[SerializeField] TrailRenderer TrailRef;                    
		[SerializeField] Transform TrailsHolder;                    

		public void Awake()
		{
			//Hide ref objects.
			TrailRef.gameObject.SetActive(false);
		}

		#region Particles

		public ParticleSystem AspahaltParticles => AsphaltSmokeParticles;


		#endregion //Particles

		#region Trail

		Queue<TrailRenderer> FreeTrails = new Queue<TrailRenderer>();

		/// <summary>
		/// Get first free trail and set start position.
		/// </summary>
		public TrailRenderer GetTrail(Vector3 startPos)
		{
			TrailRenderer trail = null;
			if (FreeTrails.Count > 0)
			{
				trail = FreeTrails.Dequeue();
			}
			else
			{
				trail = Instantiate(TrailRef, TrailsHolder);
			}

			trail.transform.position = startPos;
			trail.gameObject.SetActive(true);

			return trail;
		}

		/// <summary>
		/// Set trail as free and wait life time.
		/// </summary>
		public void SetFreeTrail(TrailRenderer trail)
		{
			StartCoroutine(WaitVisibleTrail(trail));
		}

		/// <summary>
		/// The trail is considered busy until it disappeared.
		/// </summary>
		private IEnumerator WaitVisibleTrail(TrailRenderer trail)
		{
			trail.transform.SetParent(TrailsHolder);
			yield return new WaitForSeconds(trail.time);
			trail.Clear();
			trail.gameObject.SetActive(false);
			FreeTrails.Enqueue(trail);
		}

        #endregion //Trail

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}

