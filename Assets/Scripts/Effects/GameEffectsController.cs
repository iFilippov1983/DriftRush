using Sirenix.OdinInspector;
using System;
using System.Collections;
using Lofelt.NiceVibrations;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaceManager.Effects
{
    public class GameEffectsController : MonoBehaviour
    {
        public bool debug;
        [SerializeField] private AudioTrack[] _tracks;

        [ShowInInspector, ReadOnly]
        [Tooltip("Relationship between audio types (key) and audio tracks (value)")]
        private Hashtable _audioTable;

        [ShowInInspector, ReadOnly]
        [Tooltip("Relationship between audio types (key) and jobs (value) (Coroutine, IEnumerator)")]
        private Hashtable _jobTableAudio;

        private IEffectsSettings _settings;

        private Dictionary<HapticType, HapticPatterns.PresetType> _haptics = new Dictionary<HapticType, HapticPatterns.PresetType>()
        {
            { HapticType.Selection, HapticPatterns.PresetType.Selection},
            { HapticType.Success, HapticPatterns.PresetType.Success },
            { HapticType.Warning, HapticPatterns.PresetType.Warning },
            { HapticType.Failure, HapticPatterns.PresetType.Failure },
            { HapticType.Light, HapticPatterns.PresetType.LightImpact },
            { HapticType.Medium, HapticPatterns.PresetType.MediumImpact },
            { HapticType.Heavy, HapticPatterns.PresetType.HeavyImpact },
            { HapticType.Rigid, HapticPatterns.PresetType.RigidImpact },
            { HapticType.Soft, HapticPatterns.PresetType.SoftImpact }
        };

        public Hashtable AudioTable => _audioTable;

        public enum AudioTrackType { MUSIC, SFX }
        private enum EffectAction { START, STOP, RESTART }

        [Serializable]
        public class AudioObject
        {
            public AudioType Type;
            public AudioClip Clip;
            [Range(0f, 1f)]
            public float Volume;
        }

        [Serializable]
        public class AudioTrack
        {
            public AudioTrackType Type;
            public AudioSource Source;
            public AudioObject[] Audio;
        }

        private class AudioJob
        {
            public EffectAction Action;
            public AudioType Type;
            public bool Fade;
            public WaitForSeconds Delay;

            public AudioJob(EffectAction action, AudioType type, bool fade, float secondsDelay)
            {
                Action = action;
                Type = type;
                Fade = fade;
                Delay = secondsDelay > 0f ? new WaitForSeconds(secondsDelay) : null;
            }
        }

        #region Unity Functions

        private void Awake()
        {
            DontDestroyOnLoad(this);
            Configure();
        }

        private void OnDisable()
        {
            Dispose();
        }

        #endregion

        #region Public Functions

        public void InstallSettings(IEffectsSettings settings) => _settings = settings;

        public void PlayEffect(AudioType type, bool fade = false, float secondsDelay = 0f)
        {
            AddJob(new AudioJob(EffectAction.START, type, fade, secondsDelay));
        }

        public async void PlayEffect(AudioType audioType, HapticType hapticType, bool fade = false, int secondsDelay = 0)
        { 
            PlayEffect(audioType, fade, secondsDelay);
            if (_settings.UseHaptics)
            {
                await Task.Delay(secondsDelay * 1000);
                HapticPatterns.PlayPreset(_haptics[hapticType]);
            }
        }

        public async void PlayEffect(HapticType hapticType, int millisecondsDelay = 0)
        {
            if (_settings.UseHaptics)
            {
                await Task.Delay(millisecondsDelay);
                HapticPatterns.PlayPreset(_haptics[hapticType]);
            }
        }

        public void StopAudio(AudioType type, bool fade = false, float delay = 0f)
        {
            AddJob(new AudioJob(EffectAction.STOP, type, fade, delay));
        }

        public void StopAudio(AudioTrackType trackType)
        {
            foreach (AudioTrack audioTrack in _tracks)
            {
                if (audioTrack.Type == trackType)
                    foreach (AudioObject aObj in audioTrack.Audio)
                        StopAudio(aObj.Type);
            }
        }

        public void RestartAudio(AudioType type, bool fade = false, float delay = 0f)
        {
            AddJob(new AudioJob(EffectAction.RESTART, type, fade, delay));
        }

        #endregion

        #region Private Functions

        private void Configure()
        { 
            _audioTable = new Hashtable();
            _jobTableAudio = new Hashtable();

            GenerateAudioTable();
        }

        private void Dispose()
        {
            foreach (DictionaryEntry entry in _jobTableAudio)
            { 
                Coroutine job = (Coroutine)entry.Value;
                StopCoroutine(job);
            }
        }

        private void AddJob(AudioJob job)
        {
            bool cantPlayMusic =
                GetAudioTrack(job.Type).Type == AudioTrackType.MUSIC && _settings.PlayMusic == false;

            bool cantPlaySounds =
                GetAudioTrack(job.Type).Type == AudioTrackType.SFX && _settings.PlaySounds == false;

            if (cantPlayMusic || cantPlaySounds)
                return;

            RemoveConflictingJob(job.Type);

            Coroutine jobRunner = StartCoroutine(RunAudioJob(job));
            _jobTableAudio.Add(job.Type, jobRunner);

            Log($"Starting job on [{job.Type}] with operation: {job.Action}");
        }

        private void RemoveJob(AudioType type)
        {
            if (!_jobTableAudio.ContainsKey(type))
            {
                LogWarning($"You're trying to stop a job [{type}] that isn't running");
                return;
            }

            Coroutine runningJob = (Coroutine)_jobTableAudio[type];
            StopCoroutine(runningJob);
            _jobTableAudio.Remove(type);
        }

        private void RemoveConflictingJob(AudioType type)
        {
            if (_jobTableAudio.ContainsKey(type))
                RemoveJob(type);

            AudioType conflictAudio = AudioType.None;
            AudioTrack audioTrackNeeded = GetAudioTrack(type, "Get Audio Track Needed");

            foreach (DictionaryEntry entry in _jobTableAudio)
            {
                AudioType audioType = (AudioType)entry.Key;
                AudioTrack audioTrackInUse = GetAudioTrack(audioType, "Get Audio Track In Use");

                if (audioTrackInUse.Source == audioTrackNeeded.Source)
                {
                    conflictAudio = audioType;
                    break;
                }  
            }

            if (conflictAudio != AudioType.None)
                RemoveJob(conflictAudio);
        }

        private IEnumerator RunAudioJob(AudioJob job)
        {
            if (job.Delay != null)
                yield return job.Delay;

            AudioTrack track = GetAudioTrack(job.Type);
            track.Source.clip = GetAudioClipFromAudioTrack(job.Type, track, out float volume);

            float initial = 0f;
            float target = volume;
            switch (job.Action)
            {
                case EffectAction.START:
                    track.Source.Play();
                    break;
                case EffectAction.STOP when !job.Fade:
                    track.Source.Stop();
                    break;
                case EffectAction.STOP:
                    initial = volume;
                    target = 0f;
                    break;
                case EffectAction.RESTART:
                    track.Source.Stop();
                    track.Source.Play();
                    break;
            }

            if (job.Fade)
            {
                float duration = 1f;
                float timer = 0f;

                while (timer <= duration)
                {
                    track.Source.volume = Mathf.Lerp(initial, target, timer / duration);
                    timer += Time.deltaTime;
                    yield return null;
                }

                // if _timer was 0.9999 and Time.deltaTime was 0.01 we would not have reached the target
                // make sure the volume is set to the value we want
                track.Source.volume = target;

                if (job.Action == EffectAction.STOP)
                    track.Source.Stop();
            }

            _jobTableAudio.Remove(job.Type);
            Log($"Job count {_jobTableAudio.Count}");
            yield return null;
        }

        private void GenerateAudioTable()
        {
            foreach(AudioTrack track in _tracks)
            {
                foreach (AudioObject aObj in track.Audio)
                {
                    if (_audioTable.ContainsKey(aObj.Type))
                    {
                        LogWarning($"You're trying to register audio [{aObj.Type}] that already been regisered.");
                    }
                    else
                    {
                        _audioTable.Add(aObj.Type, track);
                        Log($"Registering audio [{aObj.Type}]");
                    }
                }
            }
        }

        private AudioTrack GetAudioTrack(AudioType type, string job = "")
        {
            if (!_audioTable.ContainsKey(type))
            {
                LogWarning($"You are trying to <color=#fff>{job}</color> for [{type}] but no track was found supporting this audio type");
                return null;
            }
            return (AudioTrack)_audioTable[type];
        }

        private AudioClip GetAudioClipFromAudioTrack(AudioType type, AudioTrack track, out float volume)
        {
            foreach(AudioObject aObj in track.Audio)
            {
                if (aObj.Type == type)
                {
                    volume = aObj.Volume;
                    return aObj.Clip;
                }
            }

            volume = 1f;
            return null;
        }

        private void Log(string msg)
        { 
            if(!debug) return;
            Debug.Log($"[Audio Controller]: {msg}");
        }

        private void LogWarning(string msg)
        {
            if (!debug) return;
            Debug.LogWarning($"[Audio Controller]: {msg}");
        }

        #endregion

        // Haptic types
        //
        // Selection : a light vibration on Android, and a light impact on iOS
        // Success : a light then heavy vibration on Android, and a success impact on iOS
        // Warning : a heavy then medium vibration on Android, and a warning impact on iOS
        // Failure : a medium / heavy / heavy / light vibration pattern on Android, and a failure impact on iOS
        // Light : a light impact on iOS and a short and light vibration on Android.
        // Medium : a medium impact on iOS and a medium and regular vibration on Android
        // Heavy : a heavy impact on iOS and a long and heavy vibration on Android
        // Rigid : a short and hard impact
        // Soft : a slightly longer and softer impact
    }
}
