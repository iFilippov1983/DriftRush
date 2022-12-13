using RaceManager.Cars;
using UnityEngine;

namespace RaceManager.Effects
{
	/// <summary>
	/// Car sound controller, for play car sound effects
	/// </summary>

	[RequireComponent(typeof(Car))]
	public class CarSoundController : MonoBehaviour
	{
		[Header("Engine sounds")]
		[SerializeField] AudioClip EngineIdleClip;
		[SerializeField] AudioClip EngineBackFireClip;
		[SerializeField] float PitchOffset = 0.5f;
		[SerializeField] AudioSource EngineSource;

		[Header("Slip sounds")]
		[SerializeField] AudioSource SlipSource;
		[SerializeField] float MinSlipSound = 0.15f;
		[SerializeField] float MaxSlipForSound = 1f;
		[Space]
		[SerializeField] private GameObject SoundsAndEffectsObject;

		Car CarController;

		float MaxRPM { get { return CarController.GetMaxRPM; } }
		float EngineRPM { get { return CarController.EngineRPM; } }

		private void Awake()
		{
			CarController = GetComponent<Car>();
			CarController.BackFireAction += PlayBackfire;
		}

		private void OnEnable()
		{
			SoundsAndEffectsObject.SetActive(true);
		}

		private void OnDisable()
		{
			SoundsAndEffectsObject.SetActive(false);
		}

		void Update()
		{
			PlaySlipSound();
		}

		private void PlaySlipSound()
		{
            //Engine PRM sound
            EngineSource.pitch = (EngineRPM / MaxRPM) + PitchOffset;

            //Slip sound logic
            if (CarController.CurrentMaxSlip > MinSlipSound
            )
            {
                if (!SlipSource.isPlaying)
                {
                    SlipSource.Play();
                }
                var slipVolumeProcent = CarController.CurrentMaxSlip / MaxSlipForSound;
                SlipSource.volume = slipVolumeProcent * 0.5f;
                SlipSource.pitch = Mathf.Clamp(slipVolumeProcent, 0.75f, 1);
            }
            else
            {
                SlipSource.Stop();
            }
        }

		void PlayBackfire()
		{
            EngineSource.PlayOneShot(EngineBackFireClip);
		}

		private void OnDestroy()
		{
            CarController.BackFireAction -= PlayBackfire;
        }
	}
}
