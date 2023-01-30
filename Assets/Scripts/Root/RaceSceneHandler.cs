using UnityEngine;
using Zenject;
using RaceManager.UI;
using RaceManager.Cameras;
using RaceManager.Cars;
using RaceManager.Race;
using Random = UnityEngine.Random;
using RaceManager.Effects;
using AudioType = RaceManager.Effects.AudioType;
using UniRx;

namespace RaceManager.Root
{
    public class RaceSceneHandler : MonoBehaviour, IInitializable
    {
        [Tooltip("Player's Car speed, when speed effect starts")]
        [SerializeField] private int SpeedEffectThreshold = 70;

        private GameEvents _gameEvents;
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
        private void Construct(GameEvents gameEvents, RaceUI raceUI, GameSettingsContainer settingsContainer)
        { 
            _gameEvents = gameEvents;
            _raceUI = raceUI;
            _settingsContainer = settingsContainer;
        }

        public void Initialize()
        {
            EffectsController.InstallSettings(_settingsContainer);
            StartPlayingRandomRaceTrack();

            EventsHub<RaceEvent>.Subscribe(RaceEvent.START, SendStartNotification);
            _raceUI.OnButtonPressed += PlayButtonPressedEffect;
        }

        public void HandleEffectsFor(Driver driver, IRaceLevel raceLevel)
        {
            _playerDriver = driver;
            _gameEvents.ScreenTaped.Subscribe((u) => SetImmediateStart());

            CamerasHandler.FollowAndLookAt(driver.CarCameraFollowTarget, driver.CarCameraLookTarget);

            CamerasHandler.FollowCam.position = raceLevel.FollowCamInitialPosition;
            CamerasHandler.StartCam.position = raceLevel.StartCamInitialPosition;
            CamerasHandler.FinishCam.position = raceLevel.FinishCamInitialPosition;
        }

        #endregion

        #region Unity Functions

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                _gameEvents.ScreenTaped.OnNext();

            if (Input.GetMouseButton(0))
                _gameEvents.ScreenTapHold.OnNext();

            if (Input.GetMouseButtonUp(0))
                _gameEvents.ScreenTapReleased.OnNext();

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

        private void PlayButtonPressedEffect(string buttonName)
        {
            EffectsController.PlayEffect(AudioType.SFX_ButtonPressed, HapticType.Selection);
            _gameEvents.ButtonPressed.OnNext(buttonName);
        }

        private void SetImmediateStart() => _playerDriver.DriverProfile.CarState.Value = CarState.CanStart;

        private void SendStartNotification()
        {
            _gameEvents.Notification.OnNext(NotificationType.Start.ToString());
            EventsHub<RaceEvent>.Unsunscribe(RaceEvent.START, SendStartNotification);
        }

        private void StopPlayingCurrentTrack()
        { 
            if(EffectsController != null)
                EffectsController.StopAudio(_currentTrackType);
        }

        public void OnDestroy()
        {
            StopPlayingCurrentTrack();

            if(_raceUI != null)
                _raceUI.OnButtonPressed -= PlayButtonPressedEffect;

            StopAllCoroutines();
        }

        #endregion
    }
}

