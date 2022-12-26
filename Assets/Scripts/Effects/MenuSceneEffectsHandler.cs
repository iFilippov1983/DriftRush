using RaceManager.Root;
using UniRx;
using UnityEngine;
using Zenject;
using RaceManager.UI;
using Random = UnityEngine.Random;

namespace RaceManager.Effects
{
    public class MenuSceneEffectsHandler : MonoBehaviour, Root.IInitializable
    {
        private MainUI _mainUI;
        private EffectsSettingsContainer _settingsContainer;
        private EffectsController _effectsController;
        private SaveManager _saveManager;

        private AudioType _currentTrackType;

        private enum EffectType { Music, Sfx, Haptic}

        #region Initial Functions

        [Inject]
        private void Construct(MainUI mainUI, EffectsSettingsContainer settingsContainer, SaveManager saveManager)
        {
            _mainUI = mainUI;
            _settingsContainer = settingsContainer;
            _saveManager = saveManager;
        }

        public void Initialize()
        {
            InitializeEffectsController();
            InitializeMainUI();
        }

        private void InitializeEffectsController()
        {
            _effectsController = Singleton<EffectsController>.Instance;
            _effectsController.InstallSettings(_settingsContainer);
            StartPlayingRandomInMenuTrack();
        }

        private void InitializeMainUI()
        {
            SettingsData settingsData = new SettingsData()
            {
                playSounds = _settingsContainer.PlaySounds,
                playMusic = _settingsContainer.PlayMusic,
                useHaptics = _settingsContainer.UseHaptics
            };

            _mainUI.OnSettingsInitialize.OnNext(settingsData);
            _mainUI.OnSoundsSettingChange.Subscribe((v) => ToggleEffectSetting(v, EffectType.Sfx));
            _mainUI.OnMusicSettingChange.Subscribe((v) => ToggleEffectSetting(v, EffectType.Music));
            _mainUI.OnVibroSettingChange.Subscribe((v) => ToggleEffectSetting(v, EffectType.Haptic));

            _mainUI.OnButtonPressed += PlayButtonPressedEffect;
        }

        #endregion

        #region Private Functions

        private void StartPlayingRandomInMenuTrack()
        {
            if (_effectsController.AudioTable.ContainsKey(AudioType.MenuTrack_00))
            {
                EffectsController.AudioTrack audioTrack = _effectsController.AudioTable[AudioType.MenuTrack_00] as EffectsController.AudioTrack;
                _currentTrackType = audioTrack.Audio[Random.Range(0, audioTrack.Audio.Length)].Type;

                _effectsController.PlayEffect(_currentTrackType, true);
            }
        }

        private void StopPlayingCurrentTrack() => _effectsController.StopAudio(_currentTrackType);

        private void PlayButtonPressedEffect() => _effectsController.PlayEffect(AudioType.SFX_ButtonPressed, HapticType.Selection);

        private void ToggleEffectSetting(float toggleValue, EffectType type)
        {
            bool can = toggleValue > 0.5f ? true : false;

            switch (type)
            {
                case EffectType.Music:
                    ToggleMusic(can);
                    break;
                case EffectType.Sfx:
                    ToggleSfx(can);
                    break;
                case EffectType.Haptic:
                    ToggleHaptics(can);
                    break;
            }
           
            _effectsController.InstallSettings(_settingsContainer);
            _saveManager.Save();
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
                _effectsController.StopAudio(EffectsController.AudioTrackType.SFX);
            _settingsContainer.CanPlaySounds(canPlay);
        }

        private void ToggleHaptics(bool canUse)
        {
            _settingsContainer.CanUseHaptics(canUse);
        }

        public void OnDestroy()
        {
            StopPlayingCurrentTrack();

            if(_mainUI != null)
                _mainUI.OnButtonPressed -= PlayButtonPressedEffect;
        }

        #endregion
    }
}

