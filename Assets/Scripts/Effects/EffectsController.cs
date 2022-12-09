using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

namespace RaceManager.Effects
{
    public class EffectsController : MonoBehaviour
    {
        public bool debug;
        [SerializeField] private AudioTrack[] _tracks;

        [ShowInInspector, ReadOnly]
        [Tooltip("Relationship between audio types (key) and audio tracks (value)")]
        private Hashtable _audioTable;

        [ShowInInspector, ReadOnly]
        [Tooltip("Relationship between audio types (key) and jobs (value) (Coroutine, IEnumerator)")]
        private Hashtable _jobTable;

        #region Unity Functions

        private void Awake()
        {
            Configure();
        }

        private void OnDisable()
        {
            Dispose();
        }

        #endregion

        #region Public Functions

        internal void PlayEffect(AudioType type)
        {
            AddJob(new AudioJob(AudioAction.START, type));
        }

        internal void StopEffect(AudioType type)
        {
            AddJob(new AudioJob(AudioAction.STOP, type));
        }

        internal void RestartEffect(AudioType type)
        {
            AddJob(new AudioJob(AudioAction.RESTART, type));
        }

        #endregion

        #region Private Functions

        private void Configure()
        { 
            _audioTable = new Hashtable();
            _jobTable = new Hashtable();

            GenerateAudioTable();
        }

        private void Dispose()
        { 
        
        }

        private void GenerateAudioTable()
        {
            for (int i = 0; i < _tracks.Length; i++)
            {
                foreach (AudioObject aObj in _tracks[i].Audio)
                {
                    if (_audioTable.ContainsKey(aObj.Type))
                    {
                        LogWarning($"You're trying to register audio [{aObj.Type}] that already been regisered.");
                    }
                    else
                    {
                        _audioTable.Add(aObj.Type, _tracks[i]);
                        Log($"Registering audio [{aObj.Type}]");
                    }
                }
            }

        }

        private IEnumerator RunAudioJob(AudioJob job)
        { 
            AudioTrack track = (AudioTrack)_audioTable[job.Type];
            track.Source.clip = GetAudioClipFromAudioTrack(job.Type, track);

            switch (job.Action)
            {
                case AudioAction.START:
                    track.Source.Play();
                    break;
                case AudioAction.STOP:
                    track.Source.Stop();
                    break;
                case AudioAction.RESTART:
                    track.Source.Stop();
                    track.Source.Play(); 
                    break;
            }

            yield return null;

            _jobTable.Remove(job.Type);
            Log($"Job count {_jobTable.Count}");
        }

        private void AddJob(AudioJob job)
        {
            RemoveConflictingJob(job.Type);

            IEnumerator jobRunner = RunAudioJob(job);
            _jobTable.Add(job.Type, jobRunner);
            Log($"Starting job on [{job.Type}] with operation: {job.Action}");
        }

        private void RemoveJob(AudioType type)
        {
            if (_jobTable.ContainsKey(type))
            {
                LogWarning($"You're trying to stop a job [{type}] that isn't running");
                return;
            }

            IEnumerator runningJob = (IEnumerator)_jobTable[type];
            StopCoroutine(runningJob);
            _jobTable.Remove(type);
        }

        private void RemoveConflictingJob(AudioType type)
        {
            if (_jobTable.ContainsKey(type))
                RemoveJob(type);

            AudioType conflictAudio = AudioType.None;
            foreach (DictionaryEntry entry in _jobTable)
            {
                AudioType audioType = (AudioType)entry.Key;
                AudioTrack audioTrackInUse = (AudioTrack)_audioTable[audioType];
                AudioTrack audioTrackNeeded = (AudioTrack)_audioTable[type];

                if (audioTrackNeeded.Source == audioTrackInUse.Source)
                    conflictAudio = audioType;
            }

            if (conflictAudio != AudioType.None)
                RemoveJob(conflictAudio);
        }

        private AudioClip GetAudioClipFromAudioTrack(AudioType type, AudioTrack track)
        {
            foreach (AudioObject aObj in _audioTable)
            {
                if (aObj.Type == type)
                    return aObj.Clip;
            }

            return null;
        }

        private void Log(string msg)
        { 
            if(!debug) return;
            $"[Audio Controller]: {msg}".Log();
        }

        private void LogWarning(string msg)
        {
            if (!debug) return;
            $"[Audio Controller]: {msg}".Warning();
        }

        #endregion

        [Serializable]
        public class AudioObject
        {
            public AudioType Type;
            public AudioClip Clip;
        }

        [Serializable]
        public class AudioTrack
        { 
            public AudioSource Source;
            public AudioObject[] Audio;
        }

        private class AudioJob
        {
            public AudioAction Action;
            public AudioType Type;

            public AudioJob(AudioAction action, AudioType type)
            {
                Action = action;
                Type = type;    
            }
        }

        private enum AudioAction {  START, STOP, RESTART }
    }
}
