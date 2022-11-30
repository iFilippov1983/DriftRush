﻿using System;
using UnityEngine;
using UnityEngine.Events;

namespace RaceManager.Cars
{
    public class CarTuner : IDisposable
    {
        private const int Threshold_0 = 25;
        private const int Threshold_1 = 50;
        private const int Threshold_2 = 75;
        private const int Threshold_3 = 100;

        private CarsDepot _playerCarDepot;
        private CarProfile _carProfile;
        private CarVisual _carVisual;

        public Action OnCurrentCarChanged;
        public Func<CharacteristicType, float, int> OnCharacteristicValueChanged;
        public UnityEvent<TuneData> OnCharValueLimit = new UnityEvent<TuneData>();

        public CarTuner(CarsDepot playerCarDepot)
        {
            _playerCarDepot = playerCarDepot;
            _carProfile = _playerCarDepot.CurrentCarProfile;

            OnCharacteristicValueChanged += TuneCar;
        }

        public void SetTuner(CarVisual carVisual)
        {
            _carVisual = carVisual;
        }

        private int TuneCar(CharacteristicType characteristics, float value)
        {
            switch (characteristics)
            {
                case CharacteristicType.Speed:
                    TuneVisual(CharacteristicType.Speed, value);
                    TuneSpeed(value);
                    break;
                case CharacteristicType.Mobility:
                    TuneVisual(CharacteristicType.Mobility, value);
                    TuneMobility(value);
                    break;
                case CharacteristicType.Durability:
                    TuneVisual(CharacteristicType.Durability, value);
                    TuneDurability(value);
                    break;
                case CharacteristicType.Acceleration:
                    TuneVisual(CharacteristicType.Acceleration, value);
                    TuneAcceleration(value);
                    break;
            }

            _playerCarDepot.UpdateProfile(_carProfile);

            return _carProfile.CarCharacteristics.AvailableFactorsToUse;
        }

        private void TuneSpeed(float value)
        {
            CarProfile.Characteristics c = _carProfile.CarCharacteristics;
            Speed s = _carProfile.Speed;

            float newSpeedValue = CalculateValue(c.MaxSpeedFactor, c.MinSpeedFactor, s.Max, s.Min, value);
            _carProfile.CarConfig.MaxSpeed = newSpeedValue;
        }

        private void TuneMobility(float value)
        {
            CarProfile.Characteristics c = _carProfile.CarCharacteristics;
            Mobility m = _carProfile.Mobility;

            float newForwardFriction_f = CalculateValue(c.MaxMobilityFactor, c.MinMobilityFactor, m.f_frictionForward_Max, m.f_frictionForward_Min, value);
            _carProfile.CarConfig.FWheelsForwardFriction = newForwardFriction_f;

            float newSidewaysFriction_f = CalculateValue(c.MaxMobilityFactor, c.MinMobilityFactor, m.f_frictionSideway_Max, m.f_frictionSideway_Min, value);
            _carProfile.CarConfig.FWheelsSidewaysFriction = newSidewaysFriction_f;

            float newForwardFriction_r = CalculateValue(c.MaxMobilityFactor, c.MinMobilityFactor, m.r_frictionForward_Max, m.r_frictionForward_Min, value);
            _carProfile.CarConfig.RWheelsForwardFriction = newForwardFriction_r;

            float newSidewaysFriction_r = CalculateValue(c.MaxMobilityFactor, c.MinMobilityFactor, m.r_frictionSideway_Max, m.r_frictionSideway_Min, value);
            _carProfile.CarConfig.RWheelsSidewaysFriction = newSidewaysFriction_r;

            float newMaxSteerAngle = CalculateValue(c.MaxMobilityFactor, c.MinMobilityFactor, m.steerAngle_Max, m.steerAngle_Min, value);
            _carProfile.CarConfig.MaxSteerAngle = newMaxSteerAngle;

            float newHelpSteerPower = CalculateValue(c.MaxMobilityFactor, c.MinMobilityFactor, m.helpSteerPower_Max, m.helpSteerPower_Min, value);
            _carProfile.CarConfig.HelpSteerPower = newHelpSteerPower;

            float newSteerAngleChangeSpeed = CalculateValue(c.MaxMobilityFactor, c.MinMobilityFactor, m.steerAngleChangeSpeed_Max, m.steerAngleChangeSpeed_Min, value);
            _carProfile.CarConfig.SteerAngleChangeSpeed = newSteerAngleChangeSpeed;
        }

        private void TuneDurability(float value)
        {
            CarProfile.Characteristics c = _carProfile.CarCharacteristics;
            Durability d = _carProfile.Durability;

            float newDUrability = CalculateValue(c.MaxDurabilityFactor, c.MinDurabilityFactor, d.durabilityMax, d.durabilityMin, value);
            _carProfile.CarConfig.Durability = newDUrability;
        }

        private void TuneAcceleration(float value)
        {
            CarProfile.Characteristics c = _carProfile.CarCharacteristics;
            Acceleration a = _carProfile.Acceleration;

            float newMaxTorque = CalculateValue(c.MaxAccelerationFactor, c.MinAccelerationFactor, a.torqueMax, a.torqueMin, value);
            _carProfile.CarConfig.MaxMotorTorque = newMaxTorque;

            float newBrakeTorque = CalculateValue(c.MaxAccelerationFactor, c.MinAccelerationFactor, a.brakeTorqueMax, a.brakeTorqueMin, value);
            _carProfile.CarConfig.MaxBrakeTorque = newBrakeTorque;

            float newRPMToNextGearPercent = CalculateValue(c.MaxAccelerationFactor, c.MinAccelerationFactor, a.rpmToNextGearMax, a.rpmToNextGearMin, value);
            _carProfile.CarConfig.RPMToNextGearPercent = newRPMToNextGearPercent;
        }

        private float CalculateValue(int maxFactor, int minFactor, float maxProp, float minProp, float sliderValue)
        { 
            float koef = (float) minFactor / (float) maxFactor;
            float charUnit = (maxProp - minProp) * koef;
            float newValue = minProp + charUnit * sliderValue;

            return newValue;
        }

        private void TuneVisual(CharacteristicType cType, float value)
        {
            var characteristics = _carProfile.CarCharacteristics;

            int currentFactor = cType switch
            {
                CharacteristicType.Speed => characteristics.CurrentSpeedFactor,
                CharacteristicType.Mobility => characteristics.CurrentMobilityFactor,
                CharacteristicType.Durability => characteristics.CurrentDurabilityFactor,
                CharacteristicType.Acceleration => characteristics.CurrentAccelerationFactor,
                _ => 0
            };

            int available = _carProfile.CarCharacteristics.AvailableFactorsToUse;
            bool canSet = CanSetValue(currentFactor, Mathf.RoundToInt(value), available, out int newValue);

            if (!canSet)
            {
                OnCharValueLimit?.Invoke(new TuneData { cType = cType, value = newValue, available = available }) ;
            }

            _ = cType switch
            {
                CharacteristicType.Speed => _carProfile.CarCharacteristics.CurrentSpeedFactor = newValue,
                CharacteristicType.Mobility => _carProfile.CarCharacteristics.CurrentMobilityFactor = newValue,
                CharacteristicType.Durability => _carProfile.CarCharacteristics.CurrentDurabilityFactor = newValue,
                CharacteristicType.Acceleration => _carProfile.CarCharacteristics.CurrentAccelerationFactor = newValue,
                _ => 0
            };

            PartLevel newLevel = GetPartLevel(cType);

            var carConfigVisual = _carProfile.CarConfigVisual;

            PartLevel currentLevel = cType switch
            {
                CharacteristicType.Speed => carConfigVisual.CurrentWheelsLevel = newLevel,
                CharacteristicType.Mobility => carConfigVisual.CurrentSuspentionLevel = newLevel,
                CharacteristicType.Durability => carConfigVisual.CurrentBumpersLevel = newLevel,
                CharacteristicType.Acceleration => carConfigVisual.CurrentBodyKitsLevel = newLevel,
                _ => throw new NotImplementedException()
            };

            (PartType, PartLevel) d = cType switch
            {
                CharacteristicType.Speed => (PartType.Wheel, currentLevel),
                CharacteristicType.Mobility => (PartType.Suspention, currentLevel),
                CharacteristicType.Durability => (PartType.Bumper, currentLevel),
                CharacteristicType.Acceleration => (PartType.BodyKit, currentLevel),
                _ => throw new NotImplementedException()
            };

            WheelsSetType wst = carConfigVisual.CurrentWheelsSetType;
            _carVisual.SetPartsVisual(d.Item1, d.Item2, wst);
        }

        private bool CanSetValue(int oldValue, int newValue, int maxDifference, out int outValue)
        {
            if (newValue - oldValue > maxDifference)
            {
                outValue = oldValue + maxDifference;
                return false;
            }
            
            outValue = newValue;
            return true;
        }

        private PartLevel GetPartLevel(CharacteristicType characteristics)
        {
            var c = _carProfile.CarCharacteristics;
            int percentage = characteristics switch
            {
                CharacteristicType.Speed => Mathf.RoundToInt(c.CurrentSpeedFactor * 100 / c.MaxSpeedFactor),
                CharacteristicType.Mobility => Mathf.RoundToInt(c.CurrentMobilityFactor * 100 / c.MaxMobilityFactor),
                CharacteristicType.Durability => Mathf.RoundToInt(c.CurrentDurabilityFactor * 100 / c.MaxDurabilityFactor),
                CharacteristicType.Acceleration => Mathf.RoundToInt(c.CurrentAccelerationFactor * 100 / c.MaxAccelerationFactor),
                _ => throw new NotImplementedException(),
            };

            PartLevel pLevel = PartLevel.Zero;
            if (percentage <= Threshold_0)
                pLevel = PartLevel.Zero;
            else if (Threshold_0 < percentage && percentage <= Threshold_1)
                pLevel = PartLevel.First;
            else if (Threshold_1 < percentage && percentage <= Threshold_2)
                pLevel = PartLevel.Second;
            else if (Threshold_2 < percentage && percentage <= Threshold_3)
                pLevel = PartLevel.Third;

            //Debug.Log($"CHAR: {characteristics} => PERCENTAGE: {percentage} => LEVEL: {pLevel}");

            return pLevel;
        }

        public void ChangeCar()
        {
            _playerCarDepot.UpdateProfile(_carProfile);
            _carProfile = _playerCarDepot.CurrentCarProfile;

            OnCurrentCarChanged?.Invoke();
        }

        public void Dispose()
        {
            OnCharacteristicValueChanged -= TuneCar;
        }
    }

    public struct TuneData
    {
        public CharacteristicType cType;
        public int value;
        public int available;
    }
}