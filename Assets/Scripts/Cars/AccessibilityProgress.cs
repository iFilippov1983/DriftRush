﻿using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Cars
{
    [Serializable]
    public enum AccessStepType
    {
        GrantsCarAccess = 0,
        GrantsSomething
    }

    [Serializable]
    public class AccessibilityProgress
    {
        [JsonProperty]
        [SerializeField]
        private List<AccessibilityStep> _steps;

        private AccessibilityStep _currentStep;

        public AccessibilityStep CurrentStep
        {
            get
            {
                try
                {
                    _currentStep = _steps.First(s => s.AccessGranted == false);
                }
                catch (Exception)
                {
                    _currentStep = _steps.Last(s => s.AccessGranted == true);
                }

                return _currentStep;
            }
        }

        public int CurrentStepPointsToAccess => CurrentStep.PointsToAccess;

        public bool CarIsAvailable
        {
            get
            {
                var step = CurrentStep.Type == AccessStepType.GrantsCarAccess && CurrentStep.AccessGranted
                    ? CurrentStep
                    : _steps.Find(s => s.Type == AccessStepType.GrantsCarAccess && s.AccessGranted);

                return step != null;
            }
        }

        public bool CurrentStepCanBeGranted(int currentPointsAmount) => CurrentStep.IsReached(currentPointsAmount);

        public bool TryGrantCurrentAndSwitch(out AccessStepType accessStepType)
        {
            var nextStep = _steps.First(s => s.AccessGranted == false);

            if (CurrentStep.AccessGranted)
            {
                accessStepType = CurrentStep.Type;
                return false;
            }
            else 
            {
                accessStepType = CurrentStep.Type;
                CurrentStep.GrantAccess();
                if (nextStep != null)
                    _currentStep = nextStep;
                return true;
            }
        }


        [JsonObject]
        [Serializable]
        public class AccessibilityStep
        {
            [JsonProperty]
            [SerializeField] private AccessStepType _type;
            [JsonProperty]
            [SerializeField] private int _pointsToAccess;
            [JsonProperty]
            [SerializeField] private int _accessCost;
            [JsonProperty]
            [SerializeField] private bool _accessGranted = false;

            public AccessibilityStep(AccessStepType type, int pointsToAccess)
            {
                _type = type;
                _pointsToAccess = pointsToAccess;
            }

            public AccessStepType Type => _type;
            public int PointsToAccess => _pointsToAccess;
            public int AccessCost => _accessCost;
            public bool AccessGranted => _accessGranted;
            public bool IsReached(int currentPointsAmount) => currentPointsAmount >= _pointsToAccess;
            public void GrantAccess() => _accessGranted = true;
        }
    }
}
