using RaceManager.Root;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Cars
{
    [Serializable]
    [CreateAssetMenu(menuName = "Cars/OpponentsTuneScheme", fileName = "OpponentsTuneScheme", order = 1)]
    public class OpponentsTuneScheme : ScriptableObject, ISaveable
    {
        [Title("SPEED")]
        public bool UseSpeedAdjust;
        [ShowIf("UseSpeedAdjust")]
        [Range(0.01f, 1.1f)]
        public float MaxSpeedPercentage = 1f;
        [ShowIf("UseSpeedAdjust")]
        [Range(0.01f, 1.1f)]
        public float MinSpeedPercentage = 0.5f;
        [ShowIf("UseSpeedAdjust")]
        [Range(0.01f, 1f)]
        public float SpeedPercentageValueRange = 0.1f;
        [ShowIf("UseSpeedAdjust")]
        [ReadOnly]
        public float MinSpeedPercentageCurrent;

        [Space]
        [Title("MOBILITY")]
        public bool UseMobilityAdjust;
        [ShowIf("UseMobilityAdjust")]
        [Range(0.01f, 1.1f)]
        public float MaxMobilityPercentage = 1f;
        [ShowIf("UseMobilityAdjust")]
        [Range(0.01f, 1.1f)]
        public float MinMobilityPercentage = 0.5f;
        [ShowIf("UseMobilityAdjust")]
        [Range(0.01f, 1f)]
        public float MobilityPercentageValueRange = 0.1f;
        [ShowIf("UseMobilityAdjust")]
        [ReadOnly]
        public float MinMobilityPercentageCurrent;

        [Space]
        [Title("DURABILITY")]
        public bool UseDurabilityAdjust;
        [ShowIf("UseDurabilityAdjust")]
        [Range(0.01f, 1.1f)]
        public float MaxDurabilityPercentage = 1f;
        [ShowIf("UseDurabilityAdjust")]
        [Range(0.01f, 1.1f)]
        public float MinDurabilityPercentage = 0.5f;
        [ShowIf("UseDurabilityAdjust")]
        [Range(0.01f, 1f)]
        public float DurabilityPercentageValueRange = 0.1f;
        [ShowIf("UseDurabilityAdjust")]
        [ReadOnly]
        public float MinDurabilityPercentageCurrent;

        [Space]
        [Title("ACCELERATION")]
        public bool UseAccelerationAdjust;
        [ShowIf("UseAccelerationAdjust")]
        [Range(0.01f, 1.1f)]
        public float MaxAccelerationPercentage = 1f;
        [ShowIf("UseAccelerationAdjust")]
        [Range(0.01f, 1.1f)]
        public float MinAccelerationPercentage = 0.5f;
        [ShowIf("UseAccelerationAdjust")]
        [Range(0.01f, 1f)]
        public float AccelerationPercentageValueRange = 0.1f;
        [ShowIf("UseAccelerationAdjust")]
        [ReadOnly]
        public float MinAccelerationPercentageCurrent;

        [Space]
        [Title("GENERAL")]
        [ShowIf("ShowGenerals")]
        [Range(0.01f, 0.2f)]
        public float VictoryPercentageStep = 0.05f;
        [ShowIf("ShowGenerals")]
        [Range(0.01f, 0.2f)]
        public float LoosePercentageStep = 0.1f;
        [ShowIf("ShowGenerals")]
        public bool RandomizeOpponentsView = true;
        [ShowIf("ShowGenerals")]
        public bool AdjustFromStart;
        [ShowIf("ShowThreshold")]
        public int VictoriesThreshold = 9;

        private bool ShowGenerals => UseSpeedAdjust || UseMobilityAdjust || UseDurabilityAdjust || UseAccelerationAdjust;
        private bool ShowThreshold => !AdjustFromStart && ShowGenerals;

        public Type DataType() => typeof(SaveData);

        public void Load(object dataObject)
        {
            SaveData data = (SaveData)dataObject;
            MinSpeedPercentageCurrent = data.minSpeedPercentageCurrent;
            MinMobilityPercentageCurrent = data.minMobilityPercentageCurrent;
            MinDurabilityPercentageCurrent = data.minDurabilityPercentageCurrent;
            MinAccelerationPercentageCurrent = data.minAccelerationPercentageCurrent;
        }

        public object Save()
        {
            SaveData saveData = new SaveData()
            { 
                minSpeedPercentageCurrent = MinSpeedPercentageCurrent,
                minMobilityPercentageCurrent = MinMobilityPercentageCurrent,
                minDurabilityPercentageCurrent = MinDurabilityPercentageCurrent,
                minAccelerationPercentageCurrent = MinAccelerationPercentageCurrent
            };

            return saveData;
        }

        public class SaveData
        {
            public float minSpeedPercentageCurrent;
            public float minMobilityPercentageCurrent;
            public float minDurabilityPercentageCurrent;
            public float minAccelerationPercentageCurrent;
        }

        [PropertySpace(20)]
        [Button]
        public void ResetScheme()
        {
            UseSpeedAdjust = true;
            MaxSpeedPercentage = 1f;
            MinSpeedPercentage = 0.5f;
            SpeedPercentageValueRange = 0.1f;
            MinSpeedPercentageCurrent = MinSpeedPercentage;

            UseMobilityAdjust = true;
            MaxMobilityPercentage = 1f;
            MinMobilityPercentage = 0.5f;
            MobilityPercentageValueRange = 0.1f;
            MinMobilityPercentageCurrent = MinMobilityPercentage;

            UseDurabilityAdjust = false;
            MaxDurabilityPercentage = 1f;
            MinDurabilityPercentage = 0.5f;
            DurabilityPercentageValueRange = 0.1f;
            MinDurabilityPercentageCurrent = MinDurabilityPercentage;

            UseAccelerationAdjust = true;
            MaxAccelerationPercentage = 1f;
            MinAccelerationPercentage = 0.5f;
            AccelerationPercentageValueRange = 0.1f;
            MinAccelerationPercentageCurrent = MinAccelerationPercentage;

            VictoryPercentageStep = 0.05f;
            LoosePercentageStep = 0.1f;
            AdjustFromStart = true;
            VictoriesThreshold = 9;

            Debug.Log($"Opponents Tune Scheme has been reset => " +
                $"[Use Speed Adjust: {UseSpeedAdjust}] " +
                $"[Use Mobility Adjust: {UseMobilityAdjust}] " +
                $"[Use Durability Adjust: {UseDurabilityAdjust}] " +
                $"[Use Acceleration Adjust: {UseAccelerationAdjust}] " +
                $"[Adjust from start: {AdjustFromStart}]");
        }
    }
}
