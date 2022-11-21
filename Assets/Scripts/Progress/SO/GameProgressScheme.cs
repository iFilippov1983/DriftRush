using RaceManager.Root;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RaceManager.Progress
{
    [Serializable]
    [CreateAssetMenu(menuName = "Progress/GameProgressScheme", fileName = "GameProgressScheme", order = 1)]
    public class GameProgressScheme : SerializedScriptableObject, ISaveable
    {
        [SerializeField]
        [DictionaryDrawerSettings(KeyLabel = "Cups To Reach", ValueLabel = "Progress Step")]
        public Dictionary<int, ProgressStep> ProgressSteps = new Dictionary<int, ProgressStep>();

        public bool HasUnreceivedRewards => ProgressSteps.First(p => p.Value.IsReached == true && p.Value.RewardsReceived == false).Value != null;
        public KeyValuePair<int, ProgressStep> LastGlobalGoal => ProgressSteps.First(p => p.Value.IsLast == true);

        public ProgressStep GetStepWhithGoal(int goalCupsAmount) => ProgressSteps[goalCupsAmount];
        public Type DataType() => typeof(SaveData);

        public void Load(object data)
        {
            SaveData saveData = (SaveData)data;
            foreach (var container in saveData.StatusContainers)
            {
                var step = ProgressSteps[container.GoalCupsAmount];
                step.IsReached = container.StepReached;
                foreach (var reward in step.Rewards)
                    reward.IsReceived = container.StepRewardsReceived;
            }
        }

        public object Save()
        {
            List<ProgressStepStatusContainer> statusContainers = new List<ProgressStepStatusContainer>();
            foreach (var stepData in ProgressSteps)
            {
                int goalCups = stepData.Key;
                var step = stepData.Value;

                statusContainers.Add
                    (
                        new ProgressStepStatusContainer()
                        {
                            GoalCupsAmount = goalCups,
                            StepReached = step.IsReached,
                            StepRewardsReceived = step.RewardsReceived
                        }
                    );
            }

            return new SaveData { StatusContainers = statusContainers };
        }

        [Button]
        private void ResetAllSteps()
        {
            foreach (var stepPair in ProgressSteps)
            {
                var step = stepPair.Value;
                step.IsReached = false;
                foreach (var reward in step.Rewards)
                {
                    reward.IsReceived = false;
                }
            }

            Debug.Log("All ProgressStep.IsReached set to False; All Rewards.IsReceived set to False");
        }

        public class SaveData
        { 
            public List<ProgressStepStatusContainer> StatusContainers;
        }

        public class ProgressStepStatusContainer
        {
            public int GoalCupsAmount;
            public bool StepReached;
            public bool StepRewardsReceived;
        }
    }
}

