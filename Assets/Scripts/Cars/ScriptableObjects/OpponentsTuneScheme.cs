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
        [Title("HANDLING")]
        public bool UseHandlingAdjust;
        [ShowIf("UseHandlingAdjust")]
        [Range(0.01f, 1.1f)]
        public float MaxHandlingPercentage = 1f;
        [ShowIf("UseHandlingAdjust")]
        [Range(0.01f, 1.1f)]
        public float MinHandlingPercentage = 0.5f;
        [ShowIf("UseHandlingAdjust")]
        [Range(0.01f, 1f)]
        public float HandlingPercentageValueRange = 0.1f;
        [ShowIf("UseHandlingAdjust")]
        [ReadOnly]
        public float MinHandlingPercentageCurrent;

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
        [Title("FRICTION")]
        public bool UseFrictionAdjust;
        [ShowIf("UseFrictionAdjust")]
        [Range(0.01f, 1.1f)]
        public float MaxFrictionPercentage = 1f;
        [ShowIf("UseFrictionAdjust")]
        [Range(0.01f, 1.1f)]
        public float MinFrictionPercentage = 0.5f;
        [ShowIf("UseFrictionAdjust")]
        [Range(0.01f, 1f)]
        public float FrictionPercentageValueRange = 0.1f;
        [ShowIf("UseFrictionAdjust")]
        [ReadOnly]
        public float MinFrictionPercentageCurrent;

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

        private bool ShowGenerals => UseSpeedAdjust || UseHandlingAdjust || UseAccelerationAdjust || UseFrictionAdjust || UseDurabilityAdjust;
        private bool ShowThreshold => !AdjustFromStart && ShowGenerals;

        public Type DataType() => typeof(SaveData);

        public void Load(object dataObject)
        {
            SaveData data = (SaveData)dataObject;
            MinSpeedPercentageCurrent = data.minSpeedPercentageCurrent;
            MinHandlingPercentageCurrent = data.minHandlingPercentageCurrent;
            MinAccelerationPercentageCurrent = data.minAccelerationPercentageCurrent;
            MinFrictionPercentageCurrent = data.minFrictionPercentageCurrent;
            MinDurabilityPercentageCurrent = data.minDurabilityPercentageCurrent;
        }

        public object Save()
        {
            SaveData saveData = new SaveData()
            { 
                minSpeedPercentageCurrent = MinSpeedPercentageCurrent,
                minHandlingPercentageCurrent = MinHandlingPercentageCurrent,
                minAccelerationPercentageCurrent = MinAccelerationPercentageCurrent,
                minFrictionPercentageCurrent = MinFrictionPercentageCurrent,
                minDurabilityPercentageCurrent = MinDurabilityPercentageCurrent,
            };

            return saveData;
        }

        public class SaveData
        {
            public float minSpeedPercentageCurrent;
            public float minHandlingPercentageCurrent;
            public float minAccelerationPercentageCurrent;
            public float minFrictionPercentageCurrent;
            public float minDurabilityPercentageCurrent;
        }

        [PropertySpace(20)]
        [Button]
        public void ResetScheme()
        {
            UseSpeedAdjust = true;
            MaxSpeedPercentage = 1f;
            MinSpeedPercentage = 0.1f;
            SpeedPercentageValueRange = 0.1f;
            MinSpeedPercentageCurrent = MinSpeedPercentage;

            UseHandlingAdjust = true;
            MaxHandlingPercentage = 1f;
            MinHandlingPercentage = 0.1f;
            HandlingPercentageValueRange = 0.1f;
            MinHandlingPercentageCurrent = MinHandlingPercentage;

            UseAccelerationAdjust = true;
            MaxAccelerationPercentage = 1f;
            MinAccelerationPercentage = 0.1f;
            AccelerationPercentageValueRange = 0.1f;
            MinAccelerationPercentageCurrent = MinAccelerationPercentage;

            UseFrictionAdjust = true;
            MaxFrictionPercentage = 1f;
            MinFrictionPercentage = 0.1f;
            FrictionPercentageValueRange = 0.1f;
            MinFrictionPercentageCurrent = MinFrictionPercentage;

            UseDurabilityAdjust = false;
            MaxDurabilityPercentage = 1f;
            MinDurabilityPercentage = 0.1f;
            DurabilityPercentageValueRange = 0.1f;
            MinDurabilityPercentageCurrent = MinDurabilityPercentage;

            VictoryPercentageStep = 0.05f;
            LoosePercentageStep = 0.1f;
            AdjustFromStart = true;
            VictoriesThreshold = 9;

            Debug.Log($"Opponents Tune Scheme has been reset => " +
                $"[Use Speed Adjust: {UseSpeedAdjust}] " +
                $"[Use Handling Adjust: {UseHandlingAdjust}] " +
                $"[Use Acceleration Adjust: {UseAccelerationAdjust}] " +
                $"[Use Friction Adjust: {UseFrictionAdjust}]" +
                $"[Use Durability Adjust: {UseDurabilityAdjust}] " +
                $"[Adjust from start: {AdjustFromStart}]");
        }
    }
}
