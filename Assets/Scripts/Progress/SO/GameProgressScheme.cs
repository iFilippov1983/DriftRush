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
        private Dictionary<int, ProgressStep> ProgressSteps = new Dictionary<int, ProgressStep>();

        [SerializeField]
        [DictionaryDrawerSettings(KeyLabel = "Reward To Eaxchange", ValueLabel = "One Unit Rate")]
        private Dictionary<GameUnitType, ExchangeRateData> RewardExchangeScheme = new Dictionary<GameUnitType, ExchangeRateData>();

        public bool HasUnreceivedRewards
        {
            get 
            { 
                foreach (var p in ProgressSteps)
                {
                    if(p.Value.IsReached && p.Value.RewardsReceived == false)
                        return true;
                }
                return false;
            }
        }

        public IReadOnlyDictionary<int, ProgressStep> Steps => ProgressSteps;

        public KeyValuePair<int, ProgressStep> LastGlobalGoal => ProgressSteps.First(p => p.Value.IsLast == true);

        public ProgressStep GetStepWhithGoal(int goalCupsAmount)
        {
            if (ProgressSteps.ContainsKey(goalCupsAmount))
            {
                return ProgressSteps[goalCupsAmount];
            }
            else
            {
                return ProgressSteps.Last().Value;
            }
        }

        public ExchangeRateData GetExchangeRateFor(GameUnitType type)
        {
            if (RewardExchangeScheme.ContainsKey(type))
            {
                return RewardExchangeScheme[type];
            }
            else
            {
                return new ExchangeRateData() { AltRewardType = type, OneUnitRate = 1 };
            }
        }

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
        public void ResetAllSteps()
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

            Debug.Log("All ProgressStep.IsReached are set to False | All Rewards.IsReceived are set to False");
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

        [Serializable]
        public class ExchangeRateData
        {
            public GameUnitType AltRewardType;
            public int OneUnitRate;
        }
    }
}

