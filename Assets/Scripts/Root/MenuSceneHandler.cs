using UniRx;
using UnityEngine;
using Zenject;
using RaceManager.UI;
using Random = UnityEngine.Random;
using RaceManager.Effects;
using AudioType = RaceManager.Effects.AudioType;

namespace RaceManager.Root
{
    public class MenuSceneHandler : MonoBehaviour, IInitializable
    {
        private MainUI _mainUI;
        private GameSettingsContainer _settingsContainer;
        private GameEffectsController _effectsController;
        private SaveManager _saveManager;
        private GameEvents _gameEvents;

        private AudioType _currentTrackType;

        private enum EffectType { Music, Sfx, Haptic}
        private enum SettingsType { None, RaceLine }

        #region Initial Functions

        [Inject]
        private void Construct
            (
            MainUI mainUI, 
            GameSettingsContainer settingsContainer, 
            SaveManager saveManager,
            GameEvents gameEvents
            )
        {
            _mainUI = mainUI;
            _settingsContainer = settingsContainer;
            _saveManager = saveManager;
            _gameEvents = gameEvents;
        }

        public void Initialize()
        {
            InitializeEffectsController();
            InitializeMainUI();
        }

        private void InitializeEffectsController()
        {
            _effectsController = Singleton<GameEffectsController>.Instance;
            _effectsController.InstallSettings(_settingsContainer);
            StartPlayingRandomInMenuTrack();
        }

        private void InitializeMainUI()
        {
            SettingsData settingsData = new SettingsData()
            {
                playSounds = _settingsContainer.PlaySounds,
                playMusic = _settingsContainer.PlayMusic,
                useHaptics = _settingsContainer.UseHaptics,
                useRaceLine = _settingsContainer.UseRaceLine
            };

            _mainUI.OnSettingsInitialize.OnNext(settingsData);
            _mainUI.OnSoundsSettingChange.Subscribe((v) => ToggleEffectSetting(v, EffectType.Sfx));
            _mainUI.OnMusicSettingChange.Subscribe((v) => ToggleEffectSetting(v, EffectType.Music));
            _mainUI.OnVibroSettingChange.Subscribe((v) => ToggleEffectSetting(v, EffectType.Haptic));
            _mainUI.OnRaceLineSettingsChange.Subscribe((v) => ToggleSettings(v, SettingsType.RaceLine));

            _mainUI.OnButtonPressed += HandleButton;
        }

        #endregion

        #region Private Functions

        private void StartPlayingRandomInMenuTrack()
        {
            if (_effectsController.AudioTable.ContainsKey(AudioType.MenuTrack_00))
            {
                GameEffectsController.AudioTrack audioTrack = _effectsController.AudioTable[AudioType.MenuTrack_00] as GameEffectsController.AudioTrack;
                _currentTrackType = audioTrack.Audio[Random.Range(0, audioTrack.Audio.Length)].Type;

                _effectsController.PlayEffect(_currentTrackType, true);
            }
        }

        private void StopPlayingCurrentTrack()
        { 
            if(_effectsController)
                _effectsController.StopAudio(_currentTrackType);
        }

        private void HandleButton(string buttonName)
        {
            _effectsController.PlayEffect(AudioType.SFX_ButtonPressed, HapticType.Selection);
            _gameEvents.ButtonPressed.OnNext(buttonName);
            //$"Button pressed notification => [{buttonName}]".Log();
        }

        private void ToggleEffectSetting(bool toggleValue, EffectType type)
        {
            bool use = toggleValue;

            switch (type)
            {
                case EffectType.Music:
                    ToggleMusic(use);
                    break;
                case EffectType.Sfx:
                    ToggleSfx(use);
                    break;
                case EffectType.Haptic:
                    ToggleHaptics(use);
                    break;
            }
           
            _effectsController.InstallSettings(_settingsContainer);
            _saveManager.Save();
        }

        private void ToggleSettings(bool toggleValue, SettingsType settingsType)
        {
            bool use = toggleValue;

            switch (settingsType)
            {
                case SettingsType.None:
                    break;
                case SettingsType.RaceLine:
                    ToggleRaceLine(use);
                    break;
            }
        }

        private void ToggleMusic(bool canPlay)
        {
            if (canPlay)
            {
                _settingsContainer.CanPlayMusic(canPlay);
                StartPlayingRandomInMenuTrack();
            }  
            else
            {
                StopPlayingCurrentTrack();
                _settingsContainer.CanPlayMusic(canPlay);
            }
        }

        private void ToggleSfx(bool canPlay)
        {
            if (!canPlay)
                _effectsController.StopAudio(GameEffectsController.AudioTrackType.SFX);
            _settingsContainer.CanPlaySounds(canPlay);
        }

        private void ToggleHaptics(bool canUse)
        {
            _settingsContainer.CanUseHaptics(canUse);
        }

        private void ToggleRaceLine(bool canUse)
        {
            _settingsContainer.CanUseRaceLine(canUse);
        }

        public void OnDestroy()
        {
            StopPlayingCurrentTrack();

            if (_mainUI != null)
            {
                _mainUI.OnButtonPressed -= HandleButton;
            }

            _gameEvents.ScreenTaped.OnNext();
        }

        #endregion
    }
}

