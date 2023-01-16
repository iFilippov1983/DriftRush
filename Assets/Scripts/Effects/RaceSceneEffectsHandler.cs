using RaceManager.Root;
using UnityEngine;
using Zenject;
using RaceManager.UI;
using RaceManager.Cameras;
using RaceManager.Cars;
using RaceManager.Race;
using Random = UnityEngine.Random;
using IInitializable = RaceManager.Root.IInitializable;

namespace RaceManager.Effects
{
    public class RaceSceneEffectsHandler : MonoBehaviour, IInitializable
    {
        [Tooltip("Player's Car speed, when speed effect starts")]
        [SerializeField] private int SpeedEffectThreshold = 70;

        private RaceUI _raceUI;
        private GameSettingsContainer _settingsContainer;
        private Driver _playerDriver;
        private AudioType _currentTrackType;

        private float _carPreviousSpeed;
        private bool _speedEffectActing;

        private GameEffectsController EffectsController => Singleton<GameEffectsController>.Instance;
        private RaceCamerasHandler CamerasHandler => Singleton<RaceCamerasHandler>.Instance;
        private Wheel[] Wheels => _playerDriver.Car.Wheels;
        private float CarCurrentSpeed => _playerDriver.DriverProfile.CarCurrentSpeed;
        private float CarMaxSpeed => _playerDriver.Car.CarConfig.MaxSpeed;

        #region Initial Functions

        [Inject]
        private void Construct(RaceUI raceUI, GameSettingsContainer settingsContainer)
        { 
            _raceUI = raceUI;
            _settingsContainer = settingsContainer;
        }

        public void Initialize()
        {
            EffectsController.InstallSettings(_settingsContainer);
            StartPlayingRandomRaceTrack();

            _raceUI.OnButtonPressed += PlayButtonPressedEffect;
        }

        public void HandleEffectsFor(Driver driver, IRaceLevel raceLevel)
        {
            _playerDriver = driver;

            CamerasHandler.FollowAndLookAt(driver.CarCameraFollowTarget, driver.CarCameraLookTarget);

            CamerasHandler.FollowCam.position = raceLevel.FollowCamInitialPosition;
            CamerasHandler.StartCam.position = raceLevel.StartCamInitialPosition;
            CamerasHandler.FinishCam.position = raceLevel.FinishCamInitialPosition;
        }

        #endregion

        #region Unity Functions

        private void Update()
        {
            HandleWheels();
            HandleCarSpeed();
        }

        #endregion

        #region Private Functions

        private void HandleWheels()
        {
            for (int i = 0; i < Wheels.Length; i++)
            {
                if (Wheels[i].CurrentGroundConfig.ExtraVfxRequired)
                {
                    CamerasHandler.InvokeCameraShakeEffect();
                    EffectsController.PlayEffect(HapticType.Soft);
                }
                else
                {
                    CamerasHandler.StopCameraShakeEffect();
                }
            }
        }

        private void HandleCarSpeed()
        {
            //float speedPercent = (CarCurrentSpeed / CarMaxSpeed) * 100;
            if (CarCurrentSpeed >= SpeedEffectThreshold)
            {
                //"[Speed Effect] => START".Log(Logger.ColorRed);

                bool doShake = CarCurrentSpeed > _carPreviousSpeed || Mathf.Approximately(CarCurrentSpeed, CarMaxSpeed);

                CamerasHandler.InvokeSpeedEffect(CarCurrentSpeed, CarMaxSpeed, doShake);
                _speedEffectActing = true;
            }
            else if(_speedEffectActing)
            {
                //Debug.Log($"[Speed Effect] => STOP");

                CamerasHandler.StopSpeedEffect();
                _speedEffectActing = false;
            }

            _carPreviousSpeed = CarCurrentSpeed;
        }

        private void StartPlayingRandomRaceTrack()
        {
            if (EffectsController.AudioTable.ContainsKey(AudioType.RaceTrack_00))
            {
                GameEffectsController.AudioTrack audioTrack = EffectsController.AudioTable[AudioType.RaceTrack_00] as GameEffectsController.AudioTrack;
                _currentTrackType = audioTrack.Audio[Random.Range(0, audioTrack.Audio.Length)].Type;

                EffectsController.PlayEffect(_currentTrackType, true);
            }
        }

        private void PlayButtonPressedEffect() => EffectsController.PlayEffect(AudioType.SFX_ButtonPressed, HapticType.Selection);

        //private void StopPlayingCurrentTrack() => EffectsController.StopAudio(_currentTrackType);

        public void OnDestroy()
        {
            //StopPlayingCurrentTrack();

            if(_raceUI != null)
                _raceUI.OnButtonPressed -= PlayButtonPressedEffect;

            StopAllCoroutines();
        }

        #endregion
    }
}

