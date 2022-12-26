using RaceManager.Root;
using UniRx;
using System;
using UnityEngine;
using Zenject;
using RaceManager.UI;
using Random = UnityEngine.Random;

namespace RaceManager.Effects
{
    public class RaceSceneEffectsHandler : MonoBehaviour, Root.IInitializable
    {
        private RaceUI _raceUI;
        private EffectsSettingsContainer _settingsContainer;
        private EffectsController _effectsController;

        private AudioType _currentTrackType;

        #region Initial Functions

        [Inject]
        private void Construct(RaceUI raceUI, EffectsSettingsContainer settingsContainer)
        { 
            _raceUI = raceUI;
            _settingsContainer = settingsContainer;
        }

        public void Initialize()
        {
            _effectsController = Singleton<EffectsController>.Instance;
            _effectsController.InstallSettings(_settingsContainer);
            StartPlayingRandomRaceTrack();

            _raceUI.OnButtonPressed += PlayButtonPressedEffect;
        }

        #endregion

        #region Private Functions

        private void StartPlayingRandomRaceTrack()
        {
            if (_effectsController.AudioTable.ContainsKey(AudioType.RaceTrack_00))
            {
                EffectsController.AudioTrack audioTrack = _effectsController.AudioTable[AudioType.RaceTrack_00] as EffectsController.AudioTrack;
                _currentTrackType = audioTrack.Audio[Random.Range(0, audioTrack.Audio.Length)].Type;

                _effectsController.PlayEffect(_currentTrackType, true);
            }
        }

        private void PlayButtonPressedEffect() => _effectsController.PlayEffect(AudioType.SFX_ButtonPressed, HapticType.Selection);

        private void StopPlayingCurrentTrack() => _effectsController.StopAudio(_currentTrackType);

        public void OnDestroy()
        {
            StopPlayingCurrentTrack();

            if(_raceUI != null)
                _raceUI.OnButtonPressed -= PlayButtonPressedEffect;
        }

        #endregion
    }
}

