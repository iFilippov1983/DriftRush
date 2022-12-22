﻿using RaceManager.Root;
using System;
using UnityEngine;

namespace RaceManager.Effects
{
    [Serializable]
    [CreateAssetMenu(menuName = "Containers/EffectsSettingsContainer", fileName = "EffectsSettingsContainer", order = 1)]
    public class EffectsSettingsContainer : ScriptableObject, IEffectsSettings, ISaveable
    {
        [SerializeField] private bool _playSounds = true;
        [SerializeField] private bool _playMusic = true;
        [SerializeField] private bool _useHaptics = true;

        public bool PlaySounds => _playSounds;
        public bool PlayMusic => _playMusic;
        public bool UseHaptics => _useHaptics;

        public void CanPlaySounds(bool can) => _playSounds = can;
        public void CanPlayMusic(bool can) => _playMusic = can;
        public void CanUseHaptics(bool can) => _useHaptics = can;

        public void ResetToDefault()
        {
            _playSounds = true;
            _playMusic = true;
            _useHaptics = true;

            Debug.Log("Effects settings are reset to default");
        }

        public Type DataType() => typeof(SaveData);

        public void Load(object data)
        {
            SaveData saveData = (SaveData)data;
            _playSounds = saveData.playSounds;
            _playMusic = saveData.playMusic;
            _useHaptics = saveData.useHaptics;
        }

        public object Save()
        {
            return new SaveData()
            {
                playSounds = _playSounds,
                playMusic = _playMusic,
                useHaptics = _useHaptics
            };
        }

        public class SaveData
        {
            public bool playSounds;
            public bool playMusic;
            public bool useHaptics;
        }
    }

    public interface IEffectsSettings
    { 
        public bool PlaySounds { get; }
        public bool PlayMusic { get; }
        public bool UseHaptics { get; }
    }
}