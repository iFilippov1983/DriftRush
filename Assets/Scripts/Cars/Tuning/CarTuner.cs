using System;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace RaceManager.Cars
{
    public class CarTuner : IDisposable
    {
        private const int Threshold_0 = 25;
        private const int Threshold_1 = 50;
        private const int Threshold_2 = 75;
        private const int Threshold_3 = 100;

        protected CarsDepot _carDepot;
        protected CarProfile _carProfile;
        protected CarVisual _carVisual;

        public Action OnCurrentCarChanged;
        public Func<CharacteristicType, float, bool, int> OnCharacteristicValueChanged;
        public UnityEvent<TuneData> OnCharValueLimit = new UnityEvent<TuneData>();

        //Injected
        public CarTuner(CarsDepot carDepot)
        {
            _carDepot = carDepot;
            SetCarProfile();

            OnCharacteristicValueChanged += TuneCar;
        }

        public void SetTuner(CarVisual carVisual) => _carVisual = carVisual;
        public void SetCarProfile() => _carProfile = _carDepot.CurrentCarProfile;

        public void SetRandomCarView()
        {
            TuneVisualRandom(CharacteristicType.Speed);
            TuneVisualRandom(CharacteristicType.Handling);
            TuneVisualRandom(CharacteristicType.Acceleration);
            TuneVisualRandom(CharacteristicType.Durability);
        }

        private int TuneCar(CharacteristicType characteristics, float value, bool tuneVisual)
        {
            switch (characteristics)
            {
                case CharacteristicType.Speed:
                    if(tuneVisual)
                        TuneVisual(CharacteristicType.Speed, value);
                    TuneSpeed(value);
                    break;
                case CharacteristicType.Handling:
                    if(tuneVisual)
                        TuneVisual(CharacteristicType.Handling, value);
                    TuneHandling(value);
                    break;
                case CharacteristicType.Acceleration:
                    if(tuneVisual)
                        TuneVisual(CharacteristicType.Acceleration, value);
                    TuneAcceleration(value);
                    break;
                case CharacteristicType.Friction:
                    if (tuneVisual)
                        TuneVisual(CharacteristicType.Friction, value);
                    TuneFriction(value);
                    break;
                case CharacteristicType.Durability:
                    if (tuneVisual)
                        TuneVisual(CharacteristicType.Durability, value);
                    TuneDurability(value);
                    break;
            }

            _carDepot.UpdateProfile(_carProfile);

            return _carProfile.CarCharacteristics.AvailableFactorsToUse;
        }

        private void TuneSpeed(float value)
        {
            CarProfile.Characteristics c = _carProfile.CarCharacteristics;
            Speed s = _carProfile.Speed;

            float newSpeedValue = CalculateValue(c.MaxSpeedFactor, c.MinSpeedFactor, s.Max, s.Min, value);
            _carProfile.CarConfig.MaxSpeed = newSpeedValue;
        }

        private void TuneHandling(float value)
        {
            CarProfile.Characteristics c = _carProfile.CarCharacteristics;
            Handling h = _carProfile.Handling;

            float newMaxSteerAngle = CalculateValue(c.MaxHandlingFactor, c.MinHandlingFactor, h.steerAngle_Max, h.steerAngle_Min, value);
            _carProfile.CarConfig.MaxSteerAngle = newMaxSteerAngle;

            float newHelpSteerPower = CalculateValue(c.MaxHandlingFactor, c.MinHandlingFactor, h.helpSteerPower_Max, h.helpSteerPower_Min, value);
            _carProfile.CarConfig.HelpSteerPower = newHelpSteerPower;

            float newSteerAngleChangeSpeed = CalculateValue(c.MaxHandlingFactor, c.MinHandlingFactor, h.steerAngleChangeSpeed_Max, h.steerAngleChangeSpeed_Min, value);
            _carProfile.CarConfig.SteerAngleChangeSpeed = newSteerAngleChangeSpeed;
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

        private void TuneFriction(float value)
        {
            CarProfile.Characteristics c = _carProfile.CarCharacteristics;
            Friction f = _carProfile.Friction;

            float newForwardFriction_f = CalculateValue(c.MaxFrictionFactor, c.MinFrictionFactor, f.f_frictionForward_Max, f.f_frictionForward_Min, value);
            _carProfile.CarConfig.FWheelsForwardFriction = newForwardFriction_f;

            float newSidewaysFriction_f = CalculateValue(c.MaxFrictionFactor, c.MinFrictionFactor, f.f_frictionSideway_Max, f.f_frictionSideway_Min, value);
            _carProfile.CarConfig.FWheelsSidewaysFriction = newSidewaysFriction_f;

            float newForwardFriction_r = CalculateValue(c.MaxFrictionFactor, c.MinFrictionFactor, f.r_frictionForward_Max, f.r_frictionForward_Min, value);
            _carProfile.CarConfig.RWheelsForwardFriction = newForwardFriction_r;

            float newSidewaysFriction_r = CalculateValue(c.MaxFrictionFactor, c.MinFrictionFactor, f.r_frictionSideway_Max, f.r_frictionSideway_Min, value);
            _carProfile.CarConfig.RWheelsSidewaysFriction = newSidewaysFriction_r;
        }

        private void TuneDurability(float value)
        {
            CarProfile.Characteristics c = _carProfile.CarCharacteristics;
            Durability d = _carProfile.Durability;

            float newDUrability = CalculateValue(c.MaxDurabilityFactor, c.MinDurabilityFactor, d.durabilityMax, d.durabilityMin, value);
            _carProfile.CarConfig.Durability = newDUrability;
        }

        private float CalculateValue(int maxFactor, int minFactor, float maxValue, float minValue, float sliderValue)
        { 
            float koef = (float) minFactor / (float) maxFactor;
            float charUnit = (maxValue - minValue) * koef;
            float newValue = minValue + charUnit * sliderValue;

            return newValue;
        }

        private void TuneVisual(CharacteristicType cType, float value)
        {
            var characteristics = _carProfile.CarCharacteristics;

            int currentFactor = cType switch
            {
                CharacteristicType.Speed => characteristics.CurrentSpeedFactor,
                CharacteristicType.Handling => characteristics.CurrentHandlingFactor,
                CharacteristicType.Acceleration => characteristics.CurrentAccelerationFactor,
                CharacteristicType.Friction => characteristics.CurrentFrictionFactor,
                CharacteristicType.Durability => characteristics.CurrentDurabilityFactor,
                _ => throw new NotImplementedException()
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
                CharacteristicType.Handling => _carProfile.CarCharacteristics.CurrentHandlingFactor = newValue,
                CharacteristicType.Acceleration => _carProfile.CarCharacteristics.CurrentAccelerationFactor = newValue,
                CharacteristicType.Friction => _carProfile.CarCharacteristics.CurrentFrictionFactor = newValue,
                CharacteristicType.Durability => _carProfile.CarCharacteristics.CurrentDurabilityFactor = newValue,
                _ => throw new NotImplementedException()
            };

            PartLevel newLevel = GetPartLevel(cType);

            var carConfigVisual = _carProfile.CarConfigVisual;

            PartLevel currentLevel = cType switch
            {
                CharacteristicType.Speed => carConfigVisual.CurrentWheelsLevel = newLevel,
                CharacteristicType.Handling => carConfigVisual.CurrentSuspentionLevel = newLevel,
                CharacteristicType.Acceleration => carConfigVisual.CurrentBodyKitsLevel = newLevel,
                CharacteristicType.Friction => carConfigVisual.CurrentTiresLevel = newLevel,
                CharacteristicType.Durability => carConfigVisual.CurrentBumpersLevel = newLevel,
                _ => throw new NotImplementedException()
            };

            (PartType type, PartLevel level) = cType switch
            {
                CharacteristicType.Speed => (PartType.Wheel, currentLevel),
                CharacteristicType.Handling => (PartType.Suspention, currentLevel),
                CharacteristicType.Acceleration => (PartType.BodyKit, currentLevel),
                CharacteristicType.Friction => (PartType.Tire, currentLevel),
                CharacteristicType.Durability => (PartType.Bumper, currentLevel),
                _ => throw new NotImplementedException()
            };

            WheelsSetType wst = carConfigVisual.CurrentWheelsSetType;
            _carVisual.SetPartsVisual(type, level, wst);
        }

        private void TuneVisualRandom(CharacteristicType cType)
        {
            PartLevel newLevel = GetPartLevelRandom(cType);

            var carConfigVisual = _carProfile.CarConfigVisual;

            PartLevel currentLevel = cType switch
            {
                CharacteristicType.Speed => carConfigVisual.CurrentWheelsLevel = newLevel,
                CharacteristicType.Handling => carConfigVisual.CurrentSuspentionLevel = newLevel,
                CharacteristicType.Acceleration => carConfigVisual.CurrentBodyKitsLevel = newLevel,
                CharacteristicType.Friction => carConfigVisual.CurrentTiresLevel = newLevel,
                CharacteristicType.Durability => carConfigVisual.CurrentBumpersLevel = newLevel,
                _ => throw new NotImplementedException()
            };

            (PartType type, PartLevel level) = cType switch
            {
                CharacteristicType.Speed => (PartType.Wheel, currentLevel),
                CharacteristicType.Handling => (PartType.Suspention, currentLevel),
                CharacteristicType.Acceleration => (PartType.BodyKit, currentLevel),
                CharacteristicType.Friction => (PartType.Tire, currentLevel),
                CharacteristicType.Durability => (PartType.Bumper, currentLevel),
                _ => throw new NotImplementedException()
            };

            WheelsSetType wst = carConfigVisual.CurrentWheelsSetType;
            _carVisual.SetPartsVisual(type, level, wst);
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
                CharacteristicType.Handling => Mathf.RoundToInt(c.CurrentHandlingFactor * 100 / c.MaxHandlingFactor),
                CharacteristicType.Acceleration => Mathf.RoundToInt(c.CurrentAccelerationFactor * 100 / c.MaxAccelerationFactor),
                CharacteristicType.Friction => Mathf.RoundToInt(c.CurrentFrictionFactor * 100 / c.MaxFrictionFactor),
                CharacteristicType.Durability => Mathf.RoundToInt(c.CurrentDurabilityFactor * 100 / c.MaxDurabilityFactor),
                _ => 0,
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

        private PartLevel GetPartLevelRandom(CharacteristicType characteristics)
        {
            int randomPercentage = Mathf.RoundToInt(Random.value * 100);

            PartLevel pLevel = PartLevel.Zero;
            if (randomPercentage <= Threshold_0)
                pLevel = PartLevel.Zero;
            else if (Threshold_0 < randomPercentage && randomPercentage <= Threshold_1)
                pLevel = PartLevel.First;
            else if (Threshold_1 < randomPercentage && randomPercentage <= Threshold_2)
                pLevel = PartLevel.Second;
            else if (Threshold_2 < randomPercentage && randomPercentage <= Threshold_3)
                pLevel = PartLevel.Third;

            //Debug.Log($"[Random view] CHAR: {characteristics} => PERCENTAGE: {randomPercentage} => LEVEL: {pLevel}");

            return pLevel;
        }

        public void ChangeCar()
        {
            _carDepot.UpdateProfile(_carProfile);
            SetCarProfile();

            OnCurrentCarChanged?.Invoke();
        }

        public void Dispose()
        {
            OnCharacteristicValueChanged -= TuneCar;
        }
    }
}