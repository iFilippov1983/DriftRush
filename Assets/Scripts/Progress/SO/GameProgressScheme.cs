using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Progress
{
    [Serializable]
    [CreateAssetMenu(menuName = "Progress/GameProgressScheme", fileName = "GameProgressScheme", order = 1)]
    public class GameProgressScheme : SerializedScriptableObject
    {
        [SerializeField]
        [DictionaryDrawerSettings(KeyLabel = "Cups To Reach", ValueLabel = "Progress Step")]
        public Dictionary<int, ProgressStep> ProgressSteps = new Dictionary<int, ProgressStep>();
    }

    [Serializable]
    public class ProgressStep
    {
        [SerializeField] private bool _needsBigPrefab;

        public List<IReward> Rewards = new List<IReward>();

        public bool BigPrefab => _needsBigPrefab;
    }
}

